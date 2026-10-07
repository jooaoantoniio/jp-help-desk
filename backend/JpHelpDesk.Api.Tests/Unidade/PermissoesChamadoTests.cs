using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using JpHelpDesk.Api.Services.Security;

namespace JpHelpDesk.Api.Tests.Unidade;

public class PermissoesChamadoTests
{
    private static readonly UsuarioLogado Admin = new(1, "Admin", PerfilUsuario.Admin);
    private static readonly UsuarioLogado Tecnico = new(2, "Técnico", PerfilUsuario.Tecnico);
    private static readonly UsuarioLogado Solicitante = new(3, "Solicitante", PerfilUsuario.Usuario);
    private static readonly UsuarioLogado OutroUsuario = new(4, "Outro", PerfilUsuario.Usuario);

    private static Chamado ChamadoDoSolicitante(StatusChamado status) =>
        new() { Id = 10, SolicitanteId = Solicitante.Id, Status = status };

    [Fact]
    public void Equipe_e_solicitante_veem_o_chamado_outros_usuarios_nao()
    {
        var chamado = ChamadoDoSolicitante(StatusChamado.Aberto);

        Assert.True(PermissoesChamado.PodeVisualizar(chamado, Admin));
        Assert.True(PermissoesChamado.PodeVisualizar(chamado, Tecnico));
        Assert.True(PermissoesChamado.PodeVisualizar(chamado, Solicitante));
        Assert.False(PermissoesChamado.PodeVisualizar(chamado, OutroUsuario));
    }

    [Theory]
    [InlineData(StatusChamado.Aberto, true)]
    [InlineData(StatusChamado.EmAtendimento, false)]
    [InlineData(StatusChamado.Resolvido, false)]
    public void Solicitante_so_edita_enquanto_aberto(StatusChamado status, bool esperado)
    {
        var chamado = ChamadoDoSolicitante(status);

        Assert.Equal(esperado, PermissoesChamado.PodeEditar(chamado, Solicitante));
        Assert.True(PermissoesChamado.PodeEditar(chamado, Tecnico));
    }

    [Theory]
    [InlineData(StatusChamado.Aberto, new[] { StatusChamado.Cancelado })]
    [InlineData(StatusChamado.EmAtendimento, new StatusChamado[0])]
    [InlineData(StatusChamado.AguardandoUsuario, new StatusChamado[0])]
    [InlineData(StatusChamado.Resolvido, new[] { StatusChamado.Fechado, StatusChamado.EmAtendimento })]
    [InlineData(StatusChamado.Fechado, new StatusChamado[0])]
    public void Proximos_status_do_solicitante(StatusChamado atual, StatusChamado[] esperados)
    {
        var proximos = PermissoesChamado.ProximosStatus(ChamadoDoSolicitante(atual), Solicitante);

        Assert.Equal(esperados.Order(), proximos.Order());
    }

    [Fact]
    public void Equipe_recebe_todo_o_fluxo_valido()
    {
        var chamado = ChamadoDoSolicitante(StatusChamado.EmAtendimento);

        Assert.Equal(FluxoStatusChamado.ProximosStatus(StatusChamado.EmAtendimento), PermissoesChamado.ProximosStatus(chamado, Tecnico));
    }

    [Fact]
    public void Usuario_nao_altera_status_de_chamado_alheio()
    {
        var chamado = ChamadoDoSolicitante(StatusChamado.Aberto);

        Assert.False(PermissoesChamado.PodeAlterarStatus(chamado, OutroUsuario, StatusChamado.Cancelado));
        Assert.Empty(PermissoesChamado.ProximosStatus(chamado, OutroUsuario));
    }
}
