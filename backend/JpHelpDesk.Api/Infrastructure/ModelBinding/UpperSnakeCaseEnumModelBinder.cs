using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JpHelpDesk.Api.Infrastructure.ModelBinding;

/// <summary>
/// Converte enums vindos da URL/query string no formato UPPER_SNAKE_CASE
/// (ex.: ?status=EM_ATENDIMENTO), igual ao usado no JSON. Valores numéricos são rejeitados.
/// </summary>
public partial class UpperSnakeCaseEnumModelBinder : IModelBinder
{
    [GeneratedRegex("^[A-Za-z_]+$")]
    private static partial Regex FormatoValido();

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valor = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valor == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valor);

        var texto = valor.FirstValue;
        if (string.IsNullOrWhiteSpace(texto))
        {
            return Task.CompletedTask; // Parâmetro vazio = filtro não informado.
        }

        var tipoEnum = Nullable.GetUnderlyingType(bindingContext.ModelType) ?? bindingContext.ModelType;

        if (FormatoValido().IsMatch(texto)
            && Enum.TryParse(tipoEnum, texto.Replace("_", string.Empty), ignoreCase: true, out var resultado))
        {
            bindingContext.Result = ModelBindingResult.Success(resultado);
            return Task.CompletedTask;
        }

        var aceitos = Enum.GetNames(tipoEnum).Select(JsonNamingPolicy.SnakeCaseUpper.ConvertName);
        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            $"Valor '{texto}' inválido. Valores aceitos: {string.Join(", ", aceitos)}.");

        return Task.CompletedTask;
    }
}

/// <summary>
/// Aplica o <see cref="UpperSnakeCaseEnumModelBinder"/> a todo parâmetro enum fora do corpo da requisição.
/// </summary>
public class UpperSnakeCaseEnumModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var tipo = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
        return tipo.IsEnum ? new UpperSnakeCaseEnumModelBinder() : null;
    }
}
