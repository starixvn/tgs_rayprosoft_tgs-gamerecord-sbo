namespace Tgs.GameRecord.SBOService.Core;

public record GameExternal
{
    public string GameExternalId { get; init; } = default!;

    public string Portfolio { get; init; } = default!;
}
