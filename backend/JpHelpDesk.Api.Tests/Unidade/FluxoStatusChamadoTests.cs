using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.Tests.Unidade;

public class FluxoStatusChamadoTests
{
    [Theory]
    [InlineData(StatusChamado.Aberto, StatusChamado.EmAtendimento)]
    [InlineData(StatusChamado.Aberto, StatusChamado.Cancelado)]
    [InlineData(StatusChamado.EmAtendimento, StatusChamado.AguardandoUsuario)]
    [InlineData(StatusChamado.EmAtendimento, StatusChamado.Resolvido)]
    [InlineData(StatusChamado.AguardandoUsuario, StatusChamado.EmAtendimento)]
    [InlineData(StatusChamado.Resolvido, StatusChamado.Fechado)]
    [InlineData(StatusChamado.Resolvido, StatusChamado.EmAtendimento)] // reabertura
    public void Permite_transicoes_do_fluxo(StatusChamado de, StatusChamado para)
    {
        Assert.True(FluxoStatusChamado.PodeAlterar(de, para));
    }

    [Theory]
    [InlineData(StatusChamado.Aberto, StatusChamado.Resolvido)]      // não pula o atendimento
    [InlineData(StatusChamado.Aberto, StatusChamado.Fechado)]
    [InlineData(StatusChamado.Resolvido, StatusChamado.Cancelado)]
    [InlineData(StatusChamado.Fechado, StatusChamado.EmAtendimento)] // fechado é definitivo
    [InlineData(StatusChamado.Cancelado, StatusChamado.Aberto)]
    [InlineData(StatusChamado.EmAtendimento, StatusChamado.EmAtendimento)]
    public void Bloqueia_transicoes_fora_do_fluxo(StatusChamado de, StatusChamado para)
    {
        Assert.False(FluxoStatusChamado.PodeAlterar(de, para));
    }

    [Fact]
    public void Somente_fechado_e_cancelado_sao_finais()
    {
        var finais = Enum.GetValues<StatusChamado>().Where(FluxoStatusChamado.EhFinal);

        Assert.Equal([StatusChamado.Fechado, StatusChamado.Cancelado], finais.Order());
    }

    [Fact]
    public void Todo_status_tem_transicoes_definidas()
    {
        // Um status novo no enum sem entrada no fluxo quebraria em tempo de execução (KeyNotFoundException).
        foreach (var status in Enum.GetValues<StatusChamado>())
        {
            _ = FluxoStatusChamado.ProximosStatus(status);
        }
    }

    [Theory]
    [InlineData(StatusChamado.Aberto, true)]
    [InlineData(StatusChamado.EmAtendimento, true)]
    [InlineData(StatusChamado.AguardandoUsuario, true)]
    [InlineData(StatusChamado.Resolvido, false)]
    [InlineData(StatusChamado.Fechado, false)]
    [InlineData(StatusChamado.Cancelado, false)]
    public void Atribuicao_so_enquanto_o_chamado_esta_em_aberto(StatusChamado status, bool esperado)
    {
        Assert.Equal(esperado, FluxoStatusChamado.PermiteAtribuicao(status));
    }
}
