using System.Net;
using System.Net.Http.Json;

namespace JpHelpDesk.Api.Tests.Integracao;

public class PermissoesTests(ApiFactory api)
{
    [Fact]
    public async Task Health_check_e_publico()
    {
        await (await api.CreateClient().GetAsync("/api/health")).EsperarAsync(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(ApiFactory.Usuario)]
    [InlineData(ApiFactory.Tecnico)]
    public async Task Somente_admin_gerencia_usuarios(string email)
    {
        var cliente = await api.ClienteComoAsync(email);

        var corpo = await (await cliente.GetAsync("/api/usuarios")).EsperarAsync(HttpStatusCode.Forbidden);

        Assert.Equal("Acesso negado", corpo.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Tecnico_le_categorias_mas_nao_cria()
    {
        var cliente = await api.ClienteComoAsync(ApiFactory.Tecnico);

        await (await cliente.GetAsync("/api/categorias")).EsperarAsync(HttpStatusCode.OK);
        await (await cliente.PostAsJsonAsync("/api/categorias", new { nome = "Nao deve criar" })).EsperarAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Usuario_nao_ve_chamado_de_outra_pessoa()
    {
        var (_, email) = await api.CriarUsuarioAsync();
        var dono = api.ClienteComToken(await api.LoginAsync(email));
        var chamado = await (await dono.PostAsJsonAsync("/api/chamados", ChamadosTests.NovoChamado("Chamado privado do dono")))
            .EsperarAsync(HttpStatusCode.Created);
        var id = chamado.GetProperty("id").GetInt32();

        var outro = await api.ClienteComoAsync(ApiFactory.Usuario);

        // 404 (e não 403): não revela nem que o chamado existe.
        await (await outro.GetAsync($"/api/chamados/{id}")).EsperarAsync(HttpStatusCode.NotFound);
        await (await outro.GetAsync($"/api/chamados/{id}/historico")).EsperarAsync(HttpStatusCode.NotFound);
        var lista = await (await outro.GetAsync("/api/chamados?tamanhoPagina=50")).EsperarAsync(HttpStatusCode.OK);
        Assert.DoesNotContain(lista.GetProperty("itens").EnumerateArray(), c => c.GetProperty("id").GetInt32() == id);
    }

    [Fact]
    public async Task Usuario_comum_nao_assume_chamado()
    {
        var cliente = await api.ClienteComoAsync(ApiFactory.Usuario);
        var chamado = await (await cliente.PostAsJsonAsync("/api/chamados", ChamadosTests.NovoChamado("Tentativa de assumir")))
            .EsperarAsync(HttpStatusCode.Created);

        var resposta = await cliente.PatchAsync($"/api/chamados/{chamado.GetProperty("id").GetInt32()}/assumir", null);

        await resposta.EsperarAsync(HttpStatusCode.Forbidden);
    }
}
