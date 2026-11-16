using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Mqtt;
using Calmaria.Infrastructure.Persistencia;
using Calmaria.Infrastructure.Sensorial;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Calmaria.Infrastructure;

/// <summary>Um método por módulo, para o Program.cs ficar curto e cada módulo se registrar sozinho.</summary>
public static class InjecaoDeDependencias
{
    /// <summary>O banco (PostgreSQL) e a verificação de saúde dele em /health.</summary>
    public static IServiceCollection AddPersistencia(this IServiceCollection services, string conexao)
    {
        services.AddDbContext<CalmariaDbContext>(o => o.UseNpgsql(conexao));
        services.AddHealthChecks().AddDbContextCheck<CalmariaDbContext>("banco");
        return services;
    }

    /// <summary>Lugares, leituras, índice, horários calmos, refúgio e o consumidor MQTT.</summary>
    public static IServiceCollection AddModuloSensorial(this IServiceCollection services, IConfiguration configuracao)
    {
        var opcoesIndice = configuracao.GetSection(IndiceOptions.Secao).Get<IndiceOptions>() ?? new IndiceOptions();
        services.AddSingleton(opcoesIndice);
        services.AddSingleton<ICalculadoraIndice, CalculadoraIndicePonderada>();
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<LugarService>();
        services.AddScoped<ILugarService>(sp => sp.GetRequiredService<LugarService>());
        services.AddScoped<ILugarConsulta>(sp => sp.GetRequiredService<LugarService>());
        services.AddScoped<ILeituraService, LeituraService>();
        services.AddScoped<IHorarioCalmoService, HorarioCalmoService>();
        services.AddScoped<IRefugioService, RefugioService>();

        var secaoMqtt = configuracao.GetSection(MqttOptions.Secao);
        services.Configure<MqttOptions>(secaoMqtt);
        services.AddSingleton<EstadoMqtt>();
        services.AddHealthChecks().AddCheck<MqttHealthCheck>("mqtt");
        if (secaoMqtt.GetValue<bool>(nameof(MqttOptions.Habilitado)))
            services.AddHostedService<LeiturasMqttWorker>();

        return services;
    }
}
