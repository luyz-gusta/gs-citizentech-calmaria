using Calmaria.Domain.Sensorial;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Infrastructure.Persistencia;

/// <summary>
/// O banco da Calmaria (PostgreSQL). As configurações de cada tabela ficam em <c>Configuracoes/</c>, uma classe
/// por entidade. O modelo não usa nada exclusivo do PostgreSQL, para os testes rodarem em SQLite.
/// </summary>
public sealed class CalmariaDbContext(DbContextOptions<CalmariaDbContext> options) : DbContext(options)
{
    public DbSet<Lugar> Lugares => Set<Lugar>();
    public DbSet<Dispositivo> Dispositivos => Set<Dispositivo>();
    public DbSet<Leitura> Leituras => Set<Leitura>();
    public DbSet<PrevisaoBarulho> PrevisoesBarulho => Set<PrevisaoBarulho>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CalmariaDbContext).Assembly);
}
