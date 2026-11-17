using Calmaria.Domain.Comunidade;
using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Privacidade;
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

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<PerfilSensorial> PerfisSensoriais => Set<PerfilSensorial>();
    public DbSet<Relato> Relatos => Set<Relato>();

    public DbSet<VarreduraRegistro> Varreduras => Set<VarreduraRegistro>();
    public DbSet<AchadoRegistro> AchadosDeVarredura => Set<AchadoRegistro>();
    public DbSet<EventoDeFluxoRegistro> EventosDeFluxo => Set<EventoDeFluxoRegistro>();
    public DbSet<AlertaEmCache> AlertasEmCache => Set<AlertaEmCache>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CalmariaDbContext).Assembly);
}
