using Calmaria.Domain.Sensorial;

namespace Calmaria.Infrastructure.Persistencia.Seed;

/// <summary>
/// Os lugares do protótipo (escopo global, seção 5.7). Lugares reais da região; coordenadas e distritos são
/// aproximados e a equipe confere na observação. O lugar 7 (o refúgio escolhido na observação) entra depois.
/// </summary>
internal static class LugaresIniciais
{
    public static readonly LugarMonitorado[] Monitorados =
    [
        new(1, "Estação Consolação (Linha 2)", TipoLugar.Estacao, new Coordenada(-23.5575, -46.6609), "CONSOLACAO",
            endereco: "Av. Paulista com Rua da Consolação", horarioFuncionamento: "04:40-00:00"),
        new(2, "Estação Paulista (Linha 4)", TipoLugar.Estacao, new Coordenada(-23.5553, -46.6622), "CONSOLACAO",
            endereco: "Rua da Consolação com Av. Paulista", horarioFuncionamento: "04:40-00:00"),
        new(3, "Estação Trianon-Masp (Linha 2)", TipoLugar.Estacao, new Coordenada(-23.5614, -46.6566), "BELA VISTA",
            endereco: "Av. Paulista, em frente ao MASP", horarioFuncionamento: "04:40-00:00"),
        new(4, "Supermercado da Rua Augusta", TipoLugar.Mercado, new Coordenada(-23.5560, -46.6580), "CONSOLACAO",
            endereco: "Rua Augusta (o mercado é escolhido na observação)", horarioFuncionamento: "07:00-23:00")
    ];

    public static readonly Refugio[] Refugios =
    [
        new(5, "Parque Trianon", new Coordenada(-23.5617, -46.6573), "BELA VISTA",
            descricao: "Parque com sombra, bancos e menos barulho que a Avenida Paulista.",
            endereco: "Rua Peixoto Gomide, 949", horarioFuncionamento: "06:00-18:00"),
        new(6, "Casa das Rosas", new Coordenada(-23.5712, -46.6446), "BELA VISTA",
            descricao: "Casa e jardim de um centro cultural, com áreas quietas para sentar.",
            endereco: "Av. Paulista, 37")
    ];

    public static readonly Dispositivo[] Dispositivos =
    [
        new("esp32-consolacao-01", 1),
        new("esp32-paulista-01", 2),
        new("esp32-trianon-01", 3),
        new("esp32-mercado-01", 4)
    ];
}
