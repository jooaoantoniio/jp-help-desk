using System.ComponentModel.DataAnnotations;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.DTOs.Chamados;

/// <summary>
/// Campos disponíveis para ordenar a listagem de chamados.
/// </summary>
public enum OrdenacaoChamado
{
    DataAbertura,
    Prioridade,
    Status,
    Titulo,
    Id
}

/// <summary>
/// Filtros, ordenação e paginação da listagem de chamados.
/// </summary>
public class ChamadoQuery : PaginacaoQuery, IValidatableObject
{
    /// <summary>ID exato do chamado.</summary>
    public int? Id { get; set; }

    /// <summary>Texto contido no título.</summary>
    public string? Titulo { get; set; }

    public StatusChamado? Status { get; set; }

    public PrioridadeChamado? Prioridade { get; set; }

    public int? CategoriaId { get; set; }

    /// <summary>ID do usuário que abriu o chamado.</summary>
    public int? SolicitanteId { get; set; }

    /// <summary>ID do técnico responsável.</summary>
    public int? TecnicoId { get; set; }

    /// <summary>Abertos a partir desta data/hora (ISO 8601, ex.: 2026-10-01T00:00:00-03:00).</summary>
    public DateTimeOffset? AbertoDe { get; set; }

    /// <summary>Abertos até esta data/hora (ISO 8601).</summary>
    public DateTimeOffset? AbertoAte { get; set; }

    /// <summary>Campo de ordenação (padrão: DATA_ABERTURA).</summary>
    public OrdenacaoChamado OrdenarPor { get; set; } = OrdenacaoChamado.DataAbertura;

    /// <summary>Ordem decrescente (padrão: true — mais recentes/importantes primeiro).</summary>
    public bool Decrescente { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AbertoDe.HasValue && AbertoAte.HasValue && AbertoDe > AbertoAte)
        {
            yield return new ValidationResult(
                "A data inicial deve ser menor ou igual à data final.",
                [nameof(AbertoDe), nameof(AbertoAte)]);
        }
    }
}
