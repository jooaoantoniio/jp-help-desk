using JpHelpDesk.Api.Data.Converters;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace JpHelpDesk.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Chamado> Chamados => Set<Chamado>();
    public DbSet<HistoricoChamado> HistoricosChamado => Set<HistoricoChamado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as classes IEntityTypeConfiguration<T> da pasta Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Todos os enums são gravados como texto UPPER_SNAKE_CASE.
        configurationBuilder.Properties<PerfilUsuario>()
            .HaveConversion<UpperSnakeCaseEnumConverter<PerfilUsuario>>().HaveMaxLength(20);
        configurationBuilder.Properties<StatusChamado>()
            .HaveConversion<UpperSnakeCaseEnumConverter<StatusChamado>>().HaveMaxLength(30);
        configurationBuilder.Properties<PrioridadeChamado>()
            .HaveConversion<UpperSnakeCaseEnumConverter<PrioridadeChamado>>().HaveMaxLength(20);
        configurationBuilder.Properties<TipoHistorico>()
            .HaveConversion<UpperSnakeCaseEnumConverter<TipoHistorico>>().HaveMaxLength(30);
    }
}
