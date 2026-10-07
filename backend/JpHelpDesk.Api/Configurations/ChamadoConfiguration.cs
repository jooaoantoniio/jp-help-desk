using JpHelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JpHelpDesk.Api.Configurations;

public class ChamadoConfiguration : IEntityTypeConfiguration<Chamado>
{
    public void Configure(EntityTypeBuilder<Chamado> builder)
    {
        builder.ToTable("Chamados");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Titulo).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Descricao).IsRequired().HasMaxLength(4000);

        builder.HasOne(c => c.Categoria)
            .WithMany(cat => cat.Chamados)
            .HasForeignKey(c => c.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Solicitante)
            .WithMany(u => u.ChamadosSolicitados)
            .HasForeignKey(c => c.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Tecnico)
            .WithMany(u => u.ChamadosAtribuidos)
            .HasForeignKey(c => c.TecnicoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices para filtros e dashboard.
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.Prioridade);
        builder.HasIndex(c => c.DataAbertura);
    }
}
