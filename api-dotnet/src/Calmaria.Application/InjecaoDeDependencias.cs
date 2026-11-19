using Calmaria.Application.Comunidade;
using Calmaria.Application.Sensorial;
using Calmaria.Domain.Comunidade;
using Calmaria.Domain.Sensorial;
using Microsoft.Extensions.DependencyInjection;

namespace Calmaria.Application;

/// <summary>Os casos de uso de cada módulo, registrados pelas interfaces que o domínio define.</summary>
public static class InjecaoDeDependencias
{
    /// <summary>Lugares, leituras, horários calmos, refúgio e previsões.</summary>
    public static IServiceCollection AddCasosDeUsoSensorial(this IServiceCollection services)
    {
        services.AddScoped<LugarService>();
        services.AddScoped<ILugarService>(sp => sp.GetRequiredService<LugarService>());
        services.AddScoped<ILugarConsulta>(sp => sp.GetRequiredService<LugarService>());
        services.AddScoped<ILugarCadastro>(sp => sp.GetRequiredService<LugarService>());
        services.AddScoped<ILeituraService, LeituraService>();
        services.AddScoped<IHorarioCalmoService, HorarioCalmoService>();
        services.AddScoped<IRefugioService, RefugioService>();
        services.AddScoped<IPrevisaoService, PrevisaoService>();
        return services;
    }

    /// <summary>Usuário anônimo, perfil sensorial e relatos. Precisa do Sensorial (lugares) e da Privacidade.</summary>
    public static IServiceCollection AddCasosDeUsoComunidade(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IRelatoService, RelatoService>();
        return services;
    }
}
