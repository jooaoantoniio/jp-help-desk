using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace JpHelpDesk.Api.Tests.Integracao;

/// <summary>Formato das respostas de erro (Fase 16): ProblemDetails em português, sem detalhes internos.</summary>
public class ValidacaoTests(ApiFactory api)
{
    private static StringContent Json(string texto) => new(texto, Encoding.UTF8, "application/json");

    [Fact]
    public async Task Erro_de_validacao_usa_problem_json_titulo_em_portugues_e_campos_em_camelCase()
    {
        var admin = await api.ClienteComoAsync(ApiFactory.Admin);

        var resposta = await admin.PostAsync("/api/categorias", Json("{}"));

        var problema = await resposta.EsperarAsync(HttpStatusCode.BadRequest);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Um ou mais campos são inválidos.", problema.GetProperty("title").GetString());
        // Campo vazio: só "obrigatório", sem a mensagem de tamanho junto.
        Assert.Equal(["O nome é obrigatório."], problema.ErrosDoCampo("nome"));
    }

    [Fact]
    public async Task Enum_invalido_lista_valores_aceitos_sem_expor_tipos_internos()
    {
        var admin = await api.ClienteComoAsync(ApiFactory.Admin);

        var resposta = await admin.PostAsync("/api/usuarios",
            Json("""{"nome":"Fulano","email":"fulano@teste.com","senha":"Senha@Forte1","perfil":"CHEFE"}"""));

        var problema = await resposta.EsperarAsync(HttpStatusCode.BadRequest);
        Assert.Equal(["Valor inválido. Valores aceitos: ADMIN, TECNICO, USUARIO."], problema.ErrosDoCampo("perfil"));
        Assert.DoesNotContain("System.", problema.GetRawText());
        Assert.False(problema.GetProperty("errors").TryGetProperty("request", out _));
    }

    [Theory]
    [InlineData("""{"titulo":"Teste","categoriaId":"abc"}""", "categoriaId", "Informe um número inteiro.")]
    [InlineData("""{"nome":""", "corpo", "O corpo da requisição não é um JSON válido.")]
    [InlineData("[1,2]", "corpo", "O corpo da requisição deve ser um objeto JSON.")]
    public async Task Json_invalido_tem_mensagem_amigavel(string corpo, string campo, string mensagem)
    {
        var admin = await api.ClienteComoAsync(ApiFactory.Admin);

        var problema = await (await admin.PostAsync("/api/chamados", Json(corpo))).EsperarAsync(HttpStatusCode.BadRequest);

        Assert.Equal([mensagem], problema.ErrosDoCampo(campo));
    }

    [Fact]
    public async Task Query_string_invalida_em_portugues()
    {
        var admin = await api.ClienteComoAsync(ApiFactory.Admin);

        var problema = await (await admin.GetAsync("/api/chamados?pagina=abc&status=XYZ")).EsperarAsync(HttpStatusCode.BadRequest);

        Assert.Equal(["O valor 'abc' é inválido."], problema.ErrosDoCampo("pagina"));
        Assert.StartsWith("Valor 'XYZ' inválido.", problema.ErrosDoCampo("status")[0]);
    }

    [Fact]
    public async Task Email_duplicado_devolve_409()
    {
        var (_, email) = await api.CriarUsuarioAsync();
        var admin = await api.ClienteComoAsync(ApiFactory.Admin);

        var resposta = await admin.PostAsJsonAsync("/api/usuarios",
            new { nome = "Duplicado", email = email.ToUpperInvariant(), senha = "Senha@Forte1", perfil = "USUARIO" });

        await resposta.EsperarAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Rota_inexistente_devolve_404_explicado()
    {
        var admin = await api.ClienteComoAsync(ApiFactory.Admin);

        var problema = await (await admin.GetAsync("/api/nao-existe")).EsperarAsync(HttpStatusCode.NotFound);

        Assert.Equal("Recurso não encontrado", problema.GetProperty("title").GetString());
    }
}
