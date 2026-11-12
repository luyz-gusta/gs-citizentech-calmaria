using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Persistencia.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calmaria.Infrastructure.Persistencia.Configuracoes;

internal sealed class LugarConfiguracao : IEntityTypeConfiguration<Lugar>
{
    public void Configure(EntityTypeBuilder<Lugar> builder)
    {
        builder.HasKey(l => l.Id);
        // Os lugares do seed usam os ids do contrato (1 a 7); os criados pela API começam em 100.
        builder.Property(l => l.Id).HasIdentityOptions(startValue: 100);

        // Herança numa tabela só (TPH). O discriminador é a categoria; o tipo (ESTACAO, MERCADO...) é coluna comum.
        builder.HasDiscriminator<string>("Categoria")
            .HasValue<LugarMonitorado>("MONITORADO")
            .HasValue<Refugio>("REFUGIO");
        builder.Property<string>("Categoria").HasMaxLength(20);

        builder.Property(l => l.Nome).HasMaxLength(120).IsRequired();
        builder.Property(l => l.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.Distrito).HasMaxLength(60).IsRequired();
        builder.Property(l => l.Endereco).HasMaxLength(200);
        builder.Property(l => l.HorarioFuncionamento).HasMaxLength(20);
        builder.Ignore(l => l.Coordenada);
        builder.Ignore(l => l.EhRefugio);
        builder.HasIndex(l => l.Distrito);
    }
}

internal sealed class LugarMonitoradoConfiguracao : IEntityTypeConfiguration<LugarMonitorado>
{
    public void Configure(EntityTypeBuilder<LugarMonitorado> builder) =>
        builder.HasData(LugaresIniciais.Monitorados);
}

internal sealed class RefugioConfiguracao : IEntityTypeConfiguration<Refugio>
{
    public void Configure(EntityTypeBuilder<Refugio> builder)
    {
        builder.Property(r => r.Descricao).HasMaxLength(300);
        builder.HasData(LugaresIniciais.Refugios);
    }
}

internal sealed class DispositivoConfiguracao : IEntityTypeConfiguration<Dispositivo>
{
    public void Configure(EntityTypeBuilder<Dispositivo> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasMaxLength(60);
        builder.HasOne<Lugar>().WithMany().HasForeignKey(d => d.LugarId).OnDelete(DeleteBehavior.Restrict);
        builder.HasData(LugaresIniciais.Dispositivos);
    }
}

internal sealed class LeituraConfiguracao : IEntityTypeConfiguration<Leitura>
{
    public void Configure(EntityTypeBuilder<Leitura> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.DispositivoId).HasMaxLength(60).IsRequired();
        builder.Property(l => l.Nivel).HasConversion<string>().HasMaxLength(20);
        builder.HasOne<Lugar>().WithMany().HasForeignKey(l => l.LugarId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Dispositivo>().WithMany().HasForeignKey(l => l.DispositivoId).OnDelete(DeleteBehavior.Restrict);
        // A consulta mais comum: as leituras de um lugar num período, da mais nova para a mais velha.
        builder.HasIndex(l => new { l.LugarId, l.RegistradaEm });
    }
}

internal sealed class PrevisaoBarulhoConfiguracao : IEntityTypeConfiguration<PrevisaoBarulho>
{
    public void Configure(EntityTypeBuilder<PrevisaoBarulho> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Distrito).HasMaxLength(60).IsRequired();
        builder.Property(p => p.Nivel).HasConversion<string>().HasMaxLength(20);
        // Uma previsão por distrito e dia: importar o CSV de novo atualiza em vez de duplicar.
        builder.HasIndex(p => new { p.Distrito, p.Data }).IsUnique();
    }
}
