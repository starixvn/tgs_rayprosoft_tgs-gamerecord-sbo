namespace Tgs.GameRecord.SBOService.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public IEnumerable<GameExternal> GameExternals { get; set; } = default!;
    public double IntervalMinutes { get; set; } = 3d;
    public double IntervalMinutesMax { get; set; } = 30d;
    public double IntervalMinutesMinimum { get; set; } = 1d;
    public double LockMinute { get; set; } = 2d;
}
