namespace JpHelpDesk.Api.Tests;

/// <summary>Relógio parado num instante conhecido, para testar código que depende de data/hora.</summary>
public sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
{
    public DateTimeOffset Agora { get; set; } = agora;

    public override DateTimeOffset GetUtcNow() => Agora;
}
