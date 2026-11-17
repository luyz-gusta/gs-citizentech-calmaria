// EXEMPLO CORRIGIDO: o mesmo controller do RelatoControllerVulneravel.cs, com as correções da Calmaria. Fica fora
// da solução e serve para o Semgrep mostrar o depois. Na API de verdade, a versão final está em
// api-dotnet/src/Calmaria.Api/Comunidade/RelatosController.cs, com o comentário já tipado como TextoNaoConfiavel.
//
// O que mudou:
//   1. O comentário passa pelo Sanitizador antes de qualquer saída; o log leva só o que foi removido.
//   2. O LLM recebe só o achado mascarado (categoria e trecho como 529.***.***-25), nunca o texto.
//   3. Grava e publica o texto sanitizado.
//   4. A conexão vem da variável de ambiente ConnectionStrings__Calmaria.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Exemplos.Corrigido;

public sealed record NovoRelatoDto(Guid UsuarioId, int LugarId, int Nota, string? Comentario);

public sealed class Relato
{
    public long Id { get; set; }
    public int LugarId { get; set; }
    public string? Comentario { get; set; }
}

public sealed class ExemploDbContext(DbContextOptions<ExemploDbContext> opcoes) : DbContext(opcoes)
{
    public static string Conexao(IConfiguration configuracao) =>
        configuracao.GetConnectionString("Calmaria") ?? throw new InvalidOperationException("Defina ConnectionStrings__Calmaria.");

    public DbSet<Relato> Relatos => Set<Relato>();
}

public interface ISanitizador
{
    ResultadoSanitizacao Sanitizar(string? texto);
}

public sealed record AchadoMascarado(string Categoria, string Trecho);

public sealed record ResultadoSanitizacao(string Texto, IReadOnlyList<AchadoMascarado> Removidos);

public interface ITradutorDeRisco
{
    Task TraduzirAsync(AchadoMascarado achado);
}

[ApiController]
[Route("api/relatos")]
public sealed class RelatoControllerCorrigido(
    ExemploDbContext db,
    ILogger<RelatoControllerCorrigido> logger,
    ISanitizador sanitizador,
    ITradutorDeRisco tradutor) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] NovoRelatoDto corpo)
    {
        // 1. Primeiro o Sanitizador; depois disso, só o texto limpo existe.
        var limpo = sanitizador.Sanitizar(corpo.Comentario);
        logger.LogInformation("Relato recebido no lugar {Lugar}; {Removidos} dado(s) removido(s) do comentário.",
            corpo.LugarId, limpo.Removidos.Count);

        // 2. Ao LLM vai só o achado mascarado.
        foreach (var achado in limpo.Removidos)
            await tradutor.TraduzirAsync(achado);

        // 3. Grava e publica o texto sanitizado.
        var relato = new Relato { LugarId = corpo.LugarId, Comentario = limpo.Texto };
        db.Relatos.Add(relato);
        await db.SaveChangesAsync();
        return Created($"/api/relatos/{relato.Id}", relato);
    }
}
