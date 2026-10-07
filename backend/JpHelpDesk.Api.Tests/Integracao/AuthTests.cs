using System.Net;
using System.Net.Http.Json;

namespace JpHelpDesk.Api.Tests.Integracao;

public class AuthTests(ApiFactory api)
{
    [Fact]
    public async Task Login_valido_devolve_token_e_dados_sem_senha()
    {
        var resposta = await api.CreateClient().PostAsJsonAsync("/api/auth/login", new { email = ApiFactory.Admin, senha = ApiFactory.SenhaTeste });

        var corpo = await resposta.EsperarAsync(HttpStatusCode.OK);
        Assert.False(string.IsNullOrEmpty(corpo.GetProperty("token").GetString()));
        Assert.Equal("Bearer", corpo.GetProperty("tipoToken").GetString());
        Assert.Equal("ADMIN", corpo.GetProperty("usuario").GetProperty("perfil").GetString());
        Assert.DoesNotContain("senha", corpo.GetRawText(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Senha_errada_e_email_inexistente_tem_a_mesma_resposta()
    {
        // Mensagem única: não revela se o e-mail existe (evita enumeração de usuários).
        var cliente = api.CreateClient();
        var senhaErrada = await (await cliente.PostAsJsonAsync("/api/auth/login", new { email = ApiFactory.Admin, senha = "Errada@123" }))
            .EsperarAsync(HttpStatusCode.Unauthorized);
        var emailInexistente = await (await cliente.PostAsJsonAsync("/api/auth/login", new { email = "ninguem@jphelpdesk.com", senha = "Errada@123" }))
            .EsperarAsync(HttpStatusCode.Unauthorized);

        Assert.Equal("E-mail ou senha inválidos.", senhaErrada.GetProperty("detail").GetString());
        Assert.Equal(senhaErrada.GetProperty("detail").GetString(), emailInexistente.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Endpoint_protegido_sem_token_devolve_401_em_problem_json()
    {
        var resposta = await api.CreateClient().GetAsync("/api/chamados");

        var corpo = await resposta.EsperarAsync(HttpStatusCode.Unauthorized);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Não autenticado", corpo.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Token_adulterado_e_recusado()
    {
        var token = await api.LoginAsync(ApiFactory.Usuario);
        var partes = token.Split('.');
        // Troca o conteúdo (payload) mantendo a assinatura original: a assinatura deixa de conferir.
        var adulterado = $"{partes[0]}.{partes[1]}x.{partes[2]}";

        var resposta = await api.ClienteComToken(adulterado).GetAsync("/api/auth/me");

        await resposta.EsperarAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_devolve_o_usuario_do_token()
    {
        var cliente = await api.ClienteComoAsync(ApiFactory.Tecnico);

        var corpo = await (await cliente.GetAsync("/api/auth/me")).EsperarAsync(HttpStatusCode.OK);

        Assert.Equal(ApiFactory.Tecnico, corpo.GetProperty("email").GetString());
        Assert.Equal("TECNICO", corpo.GetProperty("perfil").GetString());
    }

    [Fact]
    public async Task Desativar_usuario_revoga_o_token_ja_emitido()
    {
        var (id, email) = await api.CriarUsuarioAsync();
        var cliente = api.ClienteComToken(await api.LoginAsync(email));
        await (await cliente.GetAsync("/api/auth/me")).EsperarAsync(HttpStatusCode.OK);

        var admin = await api.ClienteComoAsync(ApiFactory.Admin);
        await (await admin.DeleteAsync($"/api/usuarios/{id}")).EsperarAsync(HttpStatusCode.NoContent);

        // O mesmo token, ainda dentro da validade, deixa de funcionar.
        await (await cliente.GetAsync("/api/auth/me")).EsperarAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Trocar_senha_invalida_a_antiga_e_aceita_a_nova()
    {
        var (_, email) = await api.CriarUsuarioAsync();
        var cliente = api.ClienteComToken(await api.LoginAsync(email));

        var atualErrada = await cliente.PutAsJsonAsync("/api/auth/senha", new { senhaAtual = "Errada@123", novaSenha = "Nova@Senha99" });
        Assert.Equal("A senha atual está incorreta.", (await atualErrada.EsperarAsync(HttpStatusCode.BadRequest)).GetProperty("detail").GetString());

        var ok = await cliente.PutAsJsonAsync("/api/auth/senha", new { senhaAtual = ApiFactory.SenhaTeste, novaSenha = "Nova@Senha99" });
        await ok.EsperarAsync(HttpStatusCode.NoContent);

        var loginAntigo = await api.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, senha = ApiFactory.SenhaTeste });
        await loginAntigo.EsperarAsync(HttpStatusCode.Unauthorized);
        Assert.False(string.IsNullOrEmpty(await api.LoginAsync(email, "Nova@Senha99")));
    }
}
