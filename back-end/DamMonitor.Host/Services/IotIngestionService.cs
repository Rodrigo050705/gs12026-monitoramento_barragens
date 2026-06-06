using DamMonitor.Host.Contracts;
using Rebus.Bus;

namespace DamMonitor.Host.Services;

public sealed class IotIngestionService(IBus bus)
{
    public async Task<ServiceResult<object>> ReceiveAsync(IotReadingRequest request)
    {
        if (!request.TryToMessage(out var message, out var error))
        {
            return ServiceResult<object>.BadRequest(error ?? "Leitura IoT inválida.");
        }

        await bus.SendLocal(message);
        return ServiceResult<object>.Success(new { estado = "Recebido" });
    }
}
