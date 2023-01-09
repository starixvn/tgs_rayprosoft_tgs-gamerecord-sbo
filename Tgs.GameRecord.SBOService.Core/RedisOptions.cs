namespace Tgs.GameRecord.SBOService.Core;

public record RedisOptions
{
    public string ConnectionString { get; set; } = "localhost";
}
