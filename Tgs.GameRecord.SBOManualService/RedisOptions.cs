namespace Tgs.GameRecord.SBOManualService;

public record RedisOptions
{
    public string ConnectionString { get; set; } = "localhost";
}
