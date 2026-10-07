using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.Services.Security;

/// <summary>
/// Regras de permissão que dependem do chamado (dono, status), além do perfil do usuário.
/// ADMIN e TECNICO (a "equipe") atuam em qualquer chamado; USUARIO só nos que abriu.
/// </summary>
public static class PermissoesChamado
{
    // Transições que o solicitante (perfil USUARIO) pode fazer no próprio chamado.
    private static readonly (StatusChamado De, StatusChamado Para)[] TransicoesSolicitante =
    [
        (StatusChamado.Aberto, StatusChamado.Cancelado),        // desistir antes do atendimento
        (StatusChamado.Resolvido, StatusChamado.Fechado),       // confirmar a solução
        (StatusChamado.Resolvido, StatusChamado.EmAtendimento)  // reabrir: o problema voltou
    ];

    public static bool EhEquipe(UsuarioLogado usuario) =>
        usuario.Perfil is PerfilUsuario.Admin or PerfilUsuario.Tecnico;

    public static bool EhSolicitante(Chamado chamado, UsuarioLogado usuario) =>
        chamado.SolicitanteId == usuario.Id;

    public static bool PodeVisualizar(Chamado chamado, UsuarioLogado usuario) =>
        EhEquipe(usuario) || EhSolicitante(chamado, usuario);

    public static bool PodeEditar(Chamado chamado, UsuarioLogado usuario) =>
        EhEquipe(usuario) || (EhSolicitante(chamado, usuario) && chamado.Status == StatusChamado.Aberto);

    public static bool PodeAlterarStatus(Chamado chamado, UsuarioLogado usuario, StatusChamado novoStatus) =>
        EhEquipe(usuario)
        || (EhSolicitante(chamado, usuario) && TransicoesSolicitante.Contains((chamado.Status, novoStatus)));

    /// <summary>
    /// Próximos status válidos no fluxo E permitidos para este usuário (o frontend mostra só estes botões).
    /// </summary>
    public static IReadOnlyList<StatusChamado> ProximosStatus(Chamado chamado, UsuarioLogado usuario) =>
        FluxoStatusChamado.ProximosStatus(chamado.Status)
            .Where(status => PodeAlterarStatus(chamado, usuario, status))
            .ToList();
}
