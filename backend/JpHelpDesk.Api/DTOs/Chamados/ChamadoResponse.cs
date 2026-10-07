using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using JpHelpDesk.Api.Services.Security;

namespace JpHelpDesk.Api.DTOs.Chamados;

/// <summary>
/// Chamado retornado pela API. "proximosStatus" lista os status para os quais
/// o usuário atual pode alterar o chamado a partir do status atual.
/// </summary>
public record ChamadoResponse(
    int Id,
    string Titulo,
    string Descricao,
    StatusChamado Status,
    PrioridadeChamado Prioridade,
    ReferenciaResponse Categoria,
    ReferenciaResponse Solicitante,
    ReferenciaResponse? Tecnico,
    DateTimeOffset DataAbertura,
    DateTimeOffset? DataAtualizacao,
    DateTimeOffset? DataFechamento,
    IReadOnlyList<StatusChamado> ProximosStatus)
{
    /// <summary>
    /// Requer as navegações Categoria, Solicitante e Tecnico carregadas.
    /// </summary>
    public static ChamadoResponse FromEntity(Chamado chamado, UsuarioLogado usuario) => new(
        chamado.Id,
        chamado.Titulo,
        chamado.Descricao,
        chamado.Status,
        chamado.Prioridade,
        new ReferenciaResponse(chamado.Categoria.Id, chamado.Categoria.Nome),
        new ReferenciaResponse(chamado.Solicitante.Id, chamado.Solicitante.Nome),
        chamado.Tecnico is null ? null : new ReferenciaResponse(chamado.Tecnico.Id, chamado.Tecnico.Nome),
        chamado.DataAbertura,
        chamado.DataAtualizacao,
        chamado.DataFechamento,
        PermissoesChamado.ProximosStatus(chamado, usuario));
}
