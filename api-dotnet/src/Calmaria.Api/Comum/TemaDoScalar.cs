namespace Calmaria.Api.Comum;

/// <summary>
/// O tema da Calmaria no Scalar (peça AD-01 do design): acento ardósia (#3D5A73 no claro, #B2C9D8 no escuro) e a
/// fonte Atkinson Hyperlegible, a mesma do app e do painel. As cores são as de <c>design/tokens/resolvido.json</c>.
/// </summary>
internal static class TemaDoScalar
{
    public const string Css = """
        @import url('https://fonts.googleapis.com/css2?family=Atkinson+Hyperlegible+Mono:wght@400;600&family=Atkinson+Hyperlegible+Next:wght@400;600;700&display=swap');

        :root, .light-mode, .dark-mode {
          --scalar-font: "Atkinson Hyperlegible Next", system-ui, sans-serif;
          --scalar-font-code: "Atkinson Hyperlegible Mono", ui-monospace, monospace;
        }

        .light-mode {
          --scalar-color-accent: #3D5A73;
        }

        .dark-mode {
          --scalar-color-accent: #B2C9D8;
        }
        """;
}
