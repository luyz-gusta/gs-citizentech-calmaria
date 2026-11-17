using Calmaria.Infrastructure.Privacidade;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calmaria.Infrastructure.Persistencia.Configuracoes;

// As tabelas do módulo Privacidade, na migração Privacidade. Os enums vão como texto, para ler no psql.

internal sealed class VarreduraConfiguracao : IEntityTypeConfiguration<VarreduraRegistro>
{
    public void Configure(EntityTypeBuilder<VarreduraRegistro> builder)
    {
        builder.ToTable("Varreduras");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Faixa).HasConversion<string>().HasMaxLength(10);
        builder.HasIndex(v => v.ConcluidaEm);
        builder.HasMany(v => v.Achados).WithOne().HasForeignKey(a => a.VarreduraId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class AchadoConfiguracao : IEntityTypeConfiguration<AchadoRegistro>
{
    public void Configure(EntityTypeBuilder<AchadoRegistro> builder)
    {
        builder.ToTable("AchadosDeVarredura");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Fonte).HasMaxLength(20);
        builder.Property(a => a.Destino).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Local).HasMaxLength(300);
        builder.Property(a => a.RegraId).HasMaxLength(60);
        builder.Property(a => a.Categoria).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Trecho).HasMaxLength(120);
    }
}

internal sealed class EventoDeFluxoConfiguracao : IEntityTypeConfiguration<EventoDeFluxoRegistro>
{
    public void Configure(EntityTypeBuilder<EventoDeFluxoRegistro> builder)
    {
        builder.ToTable("EventosDeFluxo");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Origem).HasMaxLength(200);
        builder.Property(e => e.Destino).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Categoria).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(e => e.OcorridoEm);
    }
}

internal sealed class AlertaEmCacheConfiguracao : IEntityTypeConfiguration<AlertaEmCache>
{
    public void Configure(EntityTypeBuilder<AlertaEmCache> builder)
    {
        builder.ToTable("AlertasEmCache");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Categoria).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Destino).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Modelo).HasMaxLength(100);
        builder.Property(a => a.ParaCidadao).HasMaxLength(500);
        builder.Property(a => a.ParaEquipe).HasMaxLength(500);
        builder.HasIndex(a => new { a.Categoria, a.Destino, a.Mitigado, a.Modelo }).IsUnique();
    }
}
