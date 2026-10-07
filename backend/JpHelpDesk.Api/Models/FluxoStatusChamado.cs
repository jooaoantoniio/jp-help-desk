using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.Models;

/// <summary>
/// Máquina de estados do chamado: define para quais status cada status pode avançar.
/// </summary>
public static class FluxoStatusChamado
{
    private static readonly Dictionary<StatusChamado, StatusChamado[]> Transicoes = new()
    {
        [StatusChamado.Aberto] = [StatusChamado.EmAtendimento, StatusChamado.Cancelado],
        [StatusChamado.EmAtendimento] = [StatusChamado.AguardandoUsuario, StatusChamado.Resolvido, StatusChamado.Cancelado],
        [StatusChamado.AguardandoUsuario] = [StatusChamado.EmAtendimento, StatusChamado.Resolvido, StatusChamado.Cancelado],
        [StatusChamado.Resolvido] = [StatusChamado.Fechado, StatusChamado.EmAtendimento],
        [StatusChamado.Fechado] = [],
        [StatusChamado.Cancelado] = []
    };

    public static IReadOnlyList<StatusChamado> ProximosStatus(StatusChamado atual) => Transicoes[atual];

    public static bool PodeAlterar(StatusChamado de, StatusChamado para) => Transicoes[de].Contains(para);

    /// <summary>Status finais não aceitam mais alterações.</summary>
    public static bool EhFinal(StatusChamado status) => Transicoes[status].Length == 0;

    /// <summary>Status em que o chamado ainda pode ser (re)atribuído a um técnico.</summary>
    public static bool PermiteAtribuicao(StatusChamado status) =>
        status is StatusChamado.Aberto or StatusChamado.EmAtendimento or StatusChamado.AguardandoUsuario;
}
