using System.Net;
using System.Text.Json;

namespace JpHelpDesk.Api.Tests;

public static class RespostaHttpExtensions
{
    /// <summary>Verifica o status HTTP e devolve o corpo JSON (mostra o corpo na mensagem se o status for outro).</summary>
    public static async Task<JsonElement> EsperarAsync(this HttpResponseMessage resposta, HttpStatusCode status)
    {
        var texto = await resposta.Content.ReadAsStringAsync();
        Assert.True(resposta.StatusCode == status, $"Esperado {(int)status}, recebido {(int)resposta.StatusCode}: {texto}");
        return texto.Length == 0 ? default : JsonDocument.Parse(texto).RootElement.Clone();
    }

    /// <summary>Mensagens de validação do campo em um ValidationProblemDetails.</summary>
    public static string[] ErrosDoCampo(this JsonElement problema, string campo) =>
        problema.GetProperty("errors").GetProperty(campo).EnumerateArray().Select(e => e.GetString()!).ToArray();
}
