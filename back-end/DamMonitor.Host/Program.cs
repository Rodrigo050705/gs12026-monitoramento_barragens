using System.Text;
using DamMonitor.Host.Messaging;
using DamMonitor.Host.Services;
using DamMonitor.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Rebus.Config;
using Rebus.ServiceProvider;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var erros = context.ModelState
            .Where(item => item.Value?.Errors.Count > 0)
            .ToDictionary(
                item => item.Key,
                item => item.Value!.Errors.Select(_ => "Valor inválido.").ToArray());

        return new BadRequestObjectResult(new
        {
            erro = "Requisição inválida.",
            detalhes = erros
        });
    };
});
builder.Services.AddOpenApi();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsuariosService>();
builder.Services.AddScoped<BarragensService>();
builder.Services.AddScoped<SensoresService>();
builder.Services.AddScoped<LeiturasService>();
builder.Services.AddScoped<AlertasService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<IotIngestionService>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration.GetValue<string>("Jwt:Secret")
            ?? throw new InvalidOperationException("Jwt:Secret não foi configurado.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer") ?? "DamMonitor",
            ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience") ?? "DamMonitor.Mobile",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AutoRegisterHandlersFromAssemblyOf<SensorReadingMessageHandler>();
builder.Services.AddHostedService<MqttSensorSubscriberService>();
builder.Services.AddRebus(configure => configure.Transport(transport => transport.UseRabbitMq(
    builder.Configuration.GetValue<string>("RabbitMq:ConnectionString")
        ?? throw new InvalidOperationException("RabbitMq:ConnectionString não foi configurado."),
    builder.Configuration.GetValue<string>("Rebus:QueueName") ?? "sensor-readings")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
