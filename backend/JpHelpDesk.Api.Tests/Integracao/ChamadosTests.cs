using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace JpHelpDesk.Api.Tests.Integracao;

public class ChamadosTests(ApiFactory api)
{
    public static object NovoChamado(string titulo, string prioridade = "MEDIA") => new
    {
        titulo,
        descricao = "Descrição criada pelos testes de integração.",
        prioridade,
        categoriaId = 1
    };

    private static async Task<JsonElement> AlterarStatus(HttpClient cliente, int id, string status, HttpStatusCode esperado = HttpStatusCode.OK) =>
        await (await cliente.PatchAsJsonAsync($"/api/chamados/{id}/status", new { status, observacao = $"Teste: {status}" }))
            .EsperarAsync(esperado);

    [Fact]
    public async Task Ciclo_completo_do_chamado_com_historico()
    {
        var usuario = await api.ClienteComoAsync(ApiFactory.Usuario);
        var tecnico = await api.ClienteComoAsync(ApiFactory.Tecnico);

        // 1. Usuário abre: 201 + Location
        var abertura = await usuario.PostAsJsonAsync("/api/chamados", NovoChamado("Impressora sem toner", "ALTA"));
        var chamado = await abertura.EsperarAsync(HttpStatusCode.Created);
        var id = chamado.GetProperty("id").GetInt32();
        Assert.EndsWith($"/api/chamados/{id}", abertura.Headers.Location?.ToString());
        Assert.Equal("ABERTO", chamado.GetProperty("status").GetString());

        // 2. Iniciar sem técnico atribuído é regra de negócio (400)
        var semTecnico = await AlterarStatus(tecnico, id, "EM_ATENDIMENTO", HttpStatusCode.BadRequest);
        Assert.Equal("Atribua um técnico antes de iniciar o atendimento.", semTecnico.GetProperty("detail").GetString());

        // 3. Técnico assume, inicia e resolve
        var assumido = await (await tecnico.PatchAsync($"/api/chamados/{id}/assumir", null)).EsperarAsync(HttpStatusCode.OK);
        Assert.Equal("Técnico de Suporte", assumido.GetProperty("tecnico").GetProperty("nome").GetString());
        await AlterarStatus(tecnico, id, "EM_ATENDIMENTO");
        var resolvido = await AlterarStatus(tecnico, id, "RESOLVIDO");

        // 4. O solicitante só pode fechar ou reabrir
        Assert.Equal(["EM_ATENDIMENTO", "FECHADO"],
            (await (await usuario.GetAsync($"/api/chamados/{id}")).EsperarAsync(HttpStatusCode.OK))
                .GetProperty("proximosStatus").EnumerateArray().Select(s => s.GetString()).Order());
        var fechado = await AlterarStatus(usuario, id, "FECHADO");
        Assert.Equal("FECHADO", fechado.GetProperty("status").GetString());
        Assert.NotEqual(JsonValueKind.Null, fechado.GetProperty("dataFechamento").ValueKind);
        Assert.Empty(fechado.GetProperty("proximosStatus").EnumerateArray());
        Assert.Equal("RESOLVIDO", resolvido.GetProperty("status").GetString());

        // 5. Fechado é definitivo
        await AlterarStatus(tecnico, id, "EM_ATENDIMENTO", HttpStatusCode.BadRequest);

        // 6. Histórico registra cada passo, na ordem
        var historico = await (await usuario.GetAsync($"/api/chamados/{id}/historico")).EsperarAsync(HttpStatusCode.OK);
        var tipos = historico.EnumerateArray().Select(h => h.GetProperty("tipo").GetString()).ToArray();
        Assert.Equal(["ABERTURA", "ATRIBUICAO", "ALTERACAO_STATUS", "ALTERACAO_STATUS", "ALTERACAO_STATUS"], tipos);
    }

    [Fact]
    public async Task Transicao_fora_do_fluxo_e_recusada()
    {
        var admin = await api.ClienteComoAsync(ApiFactory.Admin);
        var chamado = await (await admin.PostAsJsonAsync("/api/chamados", NovoChamado("Pular etapas"))).EsperarAsync(HttpStatusCode.Created);

        await AlterarStatus(admin, chamado.GetProperty("id").GetInt32(), "RESOLVIDO", HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Solicitante_cancela_o_proprio_chamado_aberto()
    {
        var usuario = await api.ClienteComoAsync(ApiFactory.Usuario);
        var chamado = await (await usuario.PostAsJsonAsync("/api/chamados", NovoChamado("Abri por engano"))).EsperarAsync(HttpStatusCode.Created);
        var id = chamado.GetProperty("id").GetInt32();

        await (await usuario.DeleteAsync($"/api/chamados/{id}")).EsperarAsync(HttpStatusCode.NoContent);

        var cancelado = await (await usuario.GetAsync($"/api/chamados/{id}")).EsperarAsync(HttpStatusCode.OK);
        Assert.Equal("CANCELADO", cancelado.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Filtros_e_paginacao_da_listagem()
    {
        var admin = await api.ClienteComoAsync(ApiFactory.Admin);
        var marcador = $"Filtro {Guid.NewGuid():N}"[..20];
        foreach (var prioridade in new[] { "BAIXA", "CRITICA", "CRITICA" })
        {
            await (await admin.PostAsJsonAsync("/api/chamados", NovoChamado($"{marcador} {prioridade}", prioridade))).EsperarAsync(HttpStatusCode.Created);
        }

        var criticos = await (await admin.GetAsync($"/api/chamados?titulo={Uri.EscapeDataString(marcador)}&prioridade=CRITICA"))
            .EsperarAsync(HttpStatusCode.OK);
        Assert.Equal(2, criticos.GetProperty("totalItens").GetInt32());
        Assert.All(criticos.GetProperty("itens").EnumerateArray(), c => Assert.Equal("CRITICA", c.GetProperty("prioridade").GetString()));

        var pagina = await (await admin.GetAsync($"/api/chamados?titulo={Uri.EscapeDataString(marcador)}&tamanhoPagina=2&pagina=2"))
            .EsperarAsync(HttpStatusCode.OK);
        Assert.Equal(3, pagina.GetProperty("totalItens").GetInt32());
        Assert.Equal(2, pagina.GetProperty("totalPaginas").GetInt32());
        Assert.Single(pagina.GetProperty("itens").EnumerateArray());
    }
}
