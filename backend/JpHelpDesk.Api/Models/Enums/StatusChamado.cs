namespace JpHelpDesk.Api.Models.Enums;

/// <summary>
/// Situação de um chamado no seu ciclo de vida.
/// </summary>
public enum StatusChamado
{
    Aberto,
    EmAtendimento,
    AguardandoUsuario,
    Resolvido,
    Fechado,
    Cancelado
}
