using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JpHelpDesk.Api.Infrastructure.ErrorHandling;

/// <summary>
/// Monta a resposta 400 de validação (ValidationProblemDetails) de todos os endpoints:
/// título em português, nomes de campo iguais aos do JSON (camelCase) e mensagens de JSON inválido
/// sem expor detalhes internos (nomes de tipos .NET, posição em bytes etc.).
/// </summary>
public static class RespostaValidacao
{
    public const string Titulo = "Um ou mais campos são inválidos.";

    /// <summary>Chave usada para erros que se referem ao corpo inteiro (ausente ou malformado).</summary>
    public const string CampoCorpo = "corpo";

    /// <summary>Usado em ApiBehaviorOptions.InvalidModelStateResponseFactory.</summary>
    public static IActionResult Criar(ActionContext contexto)
    {
        var parametroCorpo = contexto.ActionDescriptor.Parameters
            .FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Body);

        var erros = new Dictionary<string, List<string>>();
        var falhaAoLerCorpo = false;

        foreach (var (chave, entrada) in contexto.ModelState)
        {
            foreach (var erro in entrada.Errors)
            {
                var (campo, mensagem, doCorpo) = Traduzir(chave, erro, parametroCorpo?.ParameterType);
                falhaAoLerCorpo |= doCorpo;

                var mensagens = erros.TryGetValue(campo, out var existentes) ? existentes : erros[campo] = [];
                if (!mensagens.Contains(mensagem))
                {
                    mensagens.Add(mensagem);
                }
            }
        }

        // Se o corpo não pôde ser lido, o parâmetro fica nulo e o MVC acrescenta
        // "The request field is required." — ruído que não ajuda quem chamou a API.
        if (falhaAoLerCorpo && parametroCorpo is not null)
        {
            erros.Remove(ParaCamelCase(parametroCorpo.Name));
        }

        var problema = new ValidationProblemDetails(erros.ToDictionary(e => e.Key, e => e.Value.ToArray()))
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = Titulo,
            Status = StatusCodes.Status400BadRequest,
            Detail = "Corrija os campos indicados em \"errors\" e tente novamente."
        };
        problema.Extensions["traceId"] = Activity.Current?.Id ?? contexto.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problema) { ContentTypes = { "application/problem+json" } };
    }

    private static (string Campo, string Mensagem, bool DoCorpo) Traduzir(string chave, ModelError erro, Type? tipoCorpo)
    {
        if (erro.Exception is JsonException json)
        {
            // Erro de sintaxe (vírgula, aspas, chave faltando): o System.Text.Json embrulha a
            // exceção do leitor. Já um valor do tipo errado ("abc" num número) não tem essa exceção interna.
            if (json.InnerException is JsonException)
            {
                return (CampoCorpo, "O corpo da requisição não é um JSON válido.", true);
            }

            var caminho = chave.TrimStart('$').TrimStart('.');
            return caminho.Length == 0
                ? (CampoCorpo, "O corpo da requisição deve ser um objeto JSON.", true)
                : (caminho, MensagemParaTipo(TipoDoCampo(tipoCorpo, caminho)), true);
        }

        // Chave vazia = o próprio corpo (ex.: requisição sem corpo).
        if (chave.Length == 0)
        {
            return (CampoCorpo, erro.ErrorMessage, true);
        }

        var mensagem = string.IsNullOrEmpty(erro.ErrorMessage) ? "Valor inválido." : erro.ErrorMessage;
        return (ParaCamelCase(chave), mensagem, false);
    }

    /// <summary>Tipo da propriedade do DTO indicada pelo caminho JSON (ex.: "perfil" -> PerfilUsuario).</summary>
    private static Type? TipoDoCampo(Type? tipoRaiz, string caminho)
    {
        var tipo = tipoRaiz;
        foreach (var segmento in caminho.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            var nome = segmento.Split('[')[0];
            tipo = tipo?.GetProperty(nome, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.PropertyType;

            // "itens[0]": o erro é no elemento da coleção.
            if (tipo is not null && segmento.Contains('['))
            {
                tipo = tipo.IsArray ? tipo.GetElementType() : tipo.GenericTypeArguments.FirstOrDefault();
            }
        }

        return tipo is null ? null : Nullable.GetUnderlyingType(tipo) ?? tipo;
    }

    private static string MensagemParaTipo(Type? tipo) => tipo switch
    {
        { IsEnum: true } =>
            $"Valor inválido. Valores aceitos: {string.Join(", ", Enum.GetNames(tipo).Select(JsonNamingPolicy.SnakeCaseUpper.ConvertName))}.",
        _ when tipo == typeof(int) || tipo == typeof(long) || tipo == typeof(short) => "Informe um número inteiro.",
        _ when tipo == typeof(decimal) || tipo == typeof(double) || tipo == typeof(float) => "Informe um número.",
        _ when tipo == typeof(bool) => "Informe true ou false.",
        _ when tipo == typeof(DateTime) || tipo == typeof(DateTimeOffset) || tipo == typeof(DateOnly) =>
            "Informe uma data válida no formato ISO 8601 (ex.: 2026-10-07).",
        _ when tipo == typeof(string) => "Informe um texto.",
        _ => "Valor em formato inválido."
    };

    /// <summary>"TamanhoPagina" -> "tamanhoPagina"; "Itens[0].Nome" -> "itens[0].nome".</summary>
    private static string ParaCamelCase(string chave) =>
        string.Join('.', chave.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
