// EXEMPLO VULNERÁVEL, DE PROPÓSITO. Fica fora da solução e não compila sozinho: serve para o Semgrep e para a
// Auditoria do Protótipo. É como o controller de relatos seria se o comentário fosse uma string comum.
// A senha abaixo é fictícia.
//
// O que está errado:
//   1. O comentário cru vai para o log (que a equipe e a VM leem) e para o console (journald na VM).
//   2. O comentário cru vai para o LLM externo, que é outra empresa.
//   3. O comentário cru é gravado e devolvido na listagem pública.
//   4. A senha do banco está escrita no código.

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Exemplos.Vulneravel;

public sealed record NovoRelatoDto(Guid UsuarioId, int LugarId, int Nota, string? Comentario);

public sealed class Relato
{
    public long Id { get; set; }
    public int LugarId { get; set; }
    public string? Comentario { get; set; }
}

public sealed class ExemploDbContext(DbContextOptions<ExemploDbContext> opcoes) : DbContext(opcoes)
{
    public const string Conexao = "Host=localhost;Database=calmaria;Username=calmaria;Password=calmaria123";

    public DbSet<Relato> Relatos => Set<Relato>();
}

[ApiController]
[Route("api/relatos")]
public sealed class RelatoControllerVulneravel(
    ExemploDbContext db,
    ILogger<RelatoControllerVulneravel> logger,
    HttpClient llm) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] NovoRelatoDto corpo)
    {
        // 1. Dado do cidadão cru no log e no console.
        logger.LogInformation("Relato recebido no lugar {Lugar}: {Comentario}", corpo.LugarId, corpo.Comentario);
        Console.WriteLine($"Comentário: {corpo.Comentario}");

        // 2. Dado do cidadão cru no LLM externo.
        await llm.PostAsJsonAsync("chat/completions", new { texto = corpo.Comentario });

        // 3. Gravado e publicado cru.
        var relato = new Relato { LugarId = corpo.LugarId, Comentario = corpo.Comentario };
        db.Relatos.Add(relato);
        await db.SaveChangesAsync();
        return Created($"/api/relatos/{relato.Id}", relato);
    }
}
