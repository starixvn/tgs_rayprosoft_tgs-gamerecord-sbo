namespace Tgs.GameRecord.SBO.Core;

public record NatsSettingOptions
{
    public string Group { get; set; } = default!;
    public string Subject { get; set; } = default!;
}
