using JpHelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JpHelpDesk.Api.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    private static readonly DateTimeOffset DataSeed = new(2026, 10, 7, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Descricao).HasMaxLength(250);

        builder.HasIndex(c => c.Nome).IsUnique();

        // Categorias iniciais (dados de referência, versionados na migration).
        builder.HasData(
            Nova(1, "Hardware", "Computadores, notebooks, monitores e periféricos"),
            Nova(2, "Software", "Instalação, atualização e erros de programas"),
            Nova(3, "Rede", "Internet, Wi-Fi, VPN e conectividade"),
            Nova(4, "Impressora", "Impressão, digitalização e toner"),
            Nova(5, "E-mail", "Caixa de e-mail, Outlook e listas de distribuição"),
            Nova(6, "Sistema", "ERP e sistemas internos da empresa"),
            Nova(7, "Acesso", "Senhas, permissões e criação de contas"),
            Nova(8, "Outros", "Solicitações que não se encaixam nas demais categorias"));
    }

    private static Categoria Nova(int id, string nome, string descricao) => new()
    {
        Id = id,
        Nome = nome,
        Descricao = descricao,
        Ativo = true,
        CriadoEm = DataSeed
    };
}
