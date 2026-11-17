using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Calmaria.Tests.Api;

/// <summary>
/// A API inteira em memória, com SQLite no lugar do PostgreSQL e um relógio fixo. Cada teste cria a sua, então
/// um teste não vê os dados do outro.
/// </summary>
internal sealed class CalmariaApiFactory(DateTimeOffset agora, IReadOnlyDictionary<string, string>? configuracao = null)
    : WebApplicationFactory<Program>
{
    /// <summary>Chave administrativa só dos testes.</summary>
    public const string ChaveAdmin = "chave-de-teste-calmaria";

    private readonly SqliteConnection conexao = AbrirConexao();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Privacidade:ChaveAdmin", ChaveAdmin);
        // Sem log em arquivo e sem varrer o repositório de verdade: cada teste de varredura passa as pastas dele.
        builder.UseSetting("LogEmArquivo:Pasta", "");
        builder.UseSetting("Privacidade:Varredura:PastaDeCodigo", "");
        foreach (var (chave, valor) in configuracao ?? new Dictionary<string, string>())
            builder.UseSetting(chave, valor);
        builder.ConfigureServices(services =>
        {
            // Tira o PostgreSQL registrado no Program.cs e põe o SQLite em memória.
            services.RemoveAll<DbContextOptions<CalmariaDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<CalmariaDbContext>>();
            services.AddDbContext<CalmariaDbContext>(o => o.UseSqlite(conexao));

            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(new RelogioFixo(agora));
        });
    }

    /// <summary>Cria as tabelas e o seed (lugares e dispositivos).</summary>
    public void PrepararBanco()
    {
        using var escopo = Services.CreateScope();
        escopo.ServiceProvider.GetRequiredService<CalmariaDbContext>().Database.EnsureCreated();
    }

    public async Task AdicionarLeituraAsync(int lugarId, string dispositivoId, double somDb, double luzLux,
        int movimentosMin, DateTimeOffset quando)
    {
        using var escopo = Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<CalmariaDbContext>();
        var calculadora = escopo.ServiceProvider.GetRequiredService<ICalculadoraIndice>();
        db.Leituras.Add(Leitura.Criar(dispositivoId, lugarId, somDb, luzLux, movimentosMin, calculadora, quando.UtcDateTime));
        await db.SaveChangesAsync();
    }

    /// <summary>Roda algo com os serviços da API num escopo próprio (banco, calculadora, relógio).</summary>
    public async Task<T> ComEscopoAsync<T>(Func<IServiceProvider, Task<T>> acao)
    {
        using var escopo = Services.CreateScope();
        return await acao(escopo.ServiceProvider);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            conexao.Dispose();
    }

    private static SqliteConnection AbrirConexao()
    {
        // O banco em memória do SQLite vive enquanto a conexão estiver aberta.
        var c = new SqliteConnection("DataSource=:memory:");
        c.Open();
        return c;
    }

    private sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora;
    }
}
