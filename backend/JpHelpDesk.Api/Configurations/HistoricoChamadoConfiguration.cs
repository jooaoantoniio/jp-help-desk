using JpHelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JpHelpDesk.Api.Configurations;

public class HistoricoChamadoConfiguration : IEntityTypeConfiguration<HistoricoChamado>
{
    public void Configure(EntityTypeBuilder<HistoricoChamado> builder)
    {
        builder.ToTable("HistoricosChamado");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Descricao).IsRequired().HasMaxLength(1000);

        builder.HasOne(h => h.Chamado)
            .WithMany(c => c.Historicos)
            .HasForeignKey(h => h.ChamadoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Usuario)
            .WithMany(u => u.Historicos)
            .HasForeignKey(h => h.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => new { h.ChamadoId, h.DataRegistro });
    }
}
