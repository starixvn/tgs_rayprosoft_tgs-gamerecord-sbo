namespace Tgs.GameRecord.SBO.Core;

public record GameExternal
{
    public string GameExternalId { get; init; } = default!;

    public string Portfolio { get; init; } = default!;
}
