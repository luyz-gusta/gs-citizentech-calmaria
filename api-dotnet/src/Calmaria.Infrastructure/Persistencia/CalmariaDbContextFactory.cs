using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Calmaria.Infrastructure.Persistencia;

/// <summary>
/// Usada só pelas ferramentas do EF Core (dotnet ef migrations add / bundle). Gerar migração não abre conexão;
/// para aplicar, a conexão vem da variável de ambiente ConnectionStrings__Calmaria.
/// </summary>
internal sealed class CalmariaDbContextFactory : IDesignTimeDbContextFactory<CalmariaDbContext>
{
    public CalmariaDbContext CreateDbContext(string[] args)
    {
        var conexao = Environment.GetEnvironmentVariable("ConnectionStrings__Calmaria")
                      ?? "Host=localhost;Port=5432;Database=calmaria;Username=calmaria";
        var opcoes = new DbContextOptionsBuilder<CalmariaDbContext>().UseNpgsql(conexao).Options;
        return new CalmariaDbContext(opcoes);
    }
}
