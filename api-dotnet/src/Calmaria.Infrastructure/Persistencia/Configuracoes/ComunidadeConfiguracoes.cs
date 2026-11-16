using Calmaria.Domain.Comunidade;
using Calmaria.Domain.Sensorial;
using Calmaria.Privacidade.Taint;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Calmaria.Infrastructure.Persistencia.Configuracoes;

/// <summary>
/// Texto sanitizado no banco é string. Ler de volta é o único caso em que a persistência recria o tipo, porque o
/// que foi gravado já passou pelo Sanitizador.
/// </summary>
internal static class ConversorDeTextoSanitizado
{
    // Os dois campos (apelido e comentário) aceitam nulo; o EF trata o nulo sozinho e só chama o conversor com valor.
    public static readonly ValueConverter<TextoSanitizado?, string> Instancia =
        new(t => t!.Valor, s => TextoSanitizado.Reidratar(s));
}

internal sealed class UsuarioConfiguracao : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Apelido).HasConversion(ConversorDeTextoSanitizado.Instancia).HasMaxLength(60);
    }
}

internal sealed class PerfilSensorialConfiguracao : IEntityTypeConfiguration<PerfilSensorial>
{
    public void Configure(EntityTypeBuilder<PerfilSensorial> builder)
    {
        builder.ToTable("PerfisSensoriais");
        builder.HasKey(p => p.UsuarioId);
        builder.HasOne<Usuario>().WithOne().HasForeignKey<PerfilSensorial>(p => p.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(p => p.ContatoApoioNome).HasMaxLength(60);
        builder.Property(p => p.ContatoApoioTelefone).HasMaxLength(20);
    }
}

internal sealed class RelatoConfiguracao : IEntityTypeConfiguration<Relato>
{
    public void Configure(EntityTypeBuilder<Relato> builder)
    {
        builder.HasKey(r => r.Id);
        // O comentário tem até 500 caracteres antes; as marcas ([removido], <SAUDE>) podem deixá-lo um pouco maior.
        builder.Property(r => r.Comentario).HasConversion(ConversorDeTextoSanitizado.Instancia).HasMaxLength(1000);

        // Apagar o usuário apaga os relatos dele (LGPD). Lugar com relato não se apaga sem querer.
        builder.HasOne<Usuario>().WithMany().HasForeignKey(r => r.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Lugar>().WithMany().HasForeignKey(r => r.LugarId).OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(r => r.Gatilhos, g =>
        {
            g.ToTable("RelatoGatilhos");
            g.WithOwner().HasForeignKey("RelatoId");
            g.Property<int>("Id");
            g.HasKey("Id");
            g.Property(x => x.Gatilho).HasConversion<string>().HasMaxLength(20);
        });
        builder.Navigation(r => r.Gatilhos).HasField("gatilhos").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(r => new { r.LugarId, r.CriadoEm });
        builder.HasIndex(r => new { r.UsuarioId, r.LugarId, r.CriadoEm });
    }
}
