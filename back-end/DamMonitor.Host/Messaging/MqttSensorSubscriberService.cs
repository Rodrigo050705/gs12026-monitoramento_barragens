using System.Buffers;
using System.Text;
using System.Text.Json;
using DamMonitor.Host.Contracts;
using MQTTnet;
using Rebus.Bus;

namespace DamMonitor.Host.Messaging;

public sealed class MqttSensorSubscriberService(
    IConfiguration configuration,
    IBus bus,
    ILogger<MqttSensorSubscriberService> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = configuration.GetValue<string>("Mqtt:Host") ?? "localhost";
        var port = configuration.GetValue<int?>("Mqtt:Port") ?? 1883;
        var topics = configuration.GetSection("Mqtt:Topics").Get<string[]>()
            ?? ["barragens/sensor/piezometro", "barragens/sensor/nivel"];

        var mqttFactory = new MqttClientFactory();
        var mqttClient = mqttFactory.CreateMqttClient();

        mqttClient.ApplicationMessageReceivedAsync += async args =>
        {
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload.ToArray());

            try
            {
                var request = JsonSerializer.Deserialize<IotReadingRequest>(payload, JsonOptions);
                if (request is null)
                {
                    logger.LogWarning("Mensagem MQTT ignorada: payload vazio ou inválido no tópico {Topico}.", args.ApplicationMessage.Topic);
                    return;
                }

                if (!request.TryToMessage(out var message, out var error))
                {
                    logger.LogWarning("Mensagem MQTT ignorada no tópico {Topico}: {Erro}", args.ApplicationMessage.Topic, error);
                    return;
                }

                await bus.SendLocal(message);
            }
            catch (JsonException exception)
            {
                logger.LogWarning(exception, "Mensagem MQTT ignorada: JSON inválido no tópico {Topico}.", args.ApplicationMessage.Topic);
            }
        };

        var options = new MqttClientOptionsBuilder()
            .WithClientId($"dam-monitor-api-{Environment.MachineName}-{Guid.NewGuid():N}")
            .WithTcpServer(host, port)
            .WithCleanSession()
            .Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await mqttClient.ConnectAsync(options, stoppingToken);

                var subscribeOptionsBuilder = mqttFactory.CreateSubscribeOptionsBuilder();
                foreach (var topic in topics)
                {
                    subscribeOptionsBuilder.WithTopicFilter(topic.Trim());
                }

                await mqttClient.SubscribeAsync(subscribeOptionsBuilder.Build(), stoppingToken);
                logger.LogInformation("Assinante MQTT conectado em {Host}:{Port} para {Topicos}.", host, port, string.Join(", ", topics));

                while (mqttClient.IsConnected && !stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Falha na conexão MQTT. Nova tentativa em 5 segundos.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        if (mqttClient.IsConnected)
        {
            await mqttClient.DisconnectAsync(cancellationToken: CancellationToken.None);
        }
    }
}
