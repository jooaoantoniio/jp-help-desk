using System.Net;
using System.Net.Http.Json;

namespace JpHelpDesk.Api.Tests.Integracao;

/// <summary>
/// Proteção contra força bruta. Usa uma instância própria da API com limite baixo
/// (o contador fica em memória, então não interfere nos outros testes).
/// </summary>
public class LimiteLoginTests : IAsyncLifetime
{
    private readonly ApiComLimiteBaixo _api = new();

    public ValueTask InitializeAsync() => _api.InitializeAsync();

    public async ValueTask DisposeAsync() => await _api.DisposeAsync();

    [Fact]
    public async Task Bloqueia_tentativas_acima_do_limite_com_429()
    {
        var cliente = _api.CreateClient();
        // Corpo inválido: o limite é contado antes da validação, então nem precisa de banco.
        var requisicao = () => cliente.PostAsJsonAsync("/api/auth/login", new { });

        await (await requisicao()).EsperarAsync(HttpStatusCode.BadRequest);
        await (await requisicao()).EsperarAsync(HttpStatusCode.BadRequest);
        var bloqueada = await (await requisicao()).EsperarAsync(HttpStatusCode.TooManyRequests);

        Assert.Equal("Muitas requisições", bloqueada.GetProperty("title").GetString());
    }

    private sealed class ApiComLimiteBaixo : ApiFactory
    {
        protected override int TentativasLoginPorMinuto => 2;
        protected override bool PrepararBanco => false;
    }
}
