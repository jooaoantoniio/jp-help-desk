using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JpHelpDesk.Api.Data.Converters;

/// <summary>
/// Grava enums como texto em UPPER_SNAKE_CASE (ex.: EmAtendimento -> "EM_ATENDIMENTO")
/// e converte de volta ao ler do banco.
/// </summary>
public class UpperSnakeCaseEnumConverter<TEnum>() : ValueConverter<TEnum, string>(
    value => JsonNamingPolicy.SnakeCaseUpper.ConvertName(value.ToString()),
    text => Enum.Parse<TEnum>(text.Replace("_", string.Empty), true))
    where TEnum : struct, Enum;
