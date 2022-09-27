namespace Tgs.GameRecord.SBOService;

public record RedisOptions
{
    public string ConnectionString { get; set; } = "localhost";
}
