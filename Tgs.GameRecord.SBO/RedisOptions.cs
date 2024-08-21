namespace Tgs.GameRecord.SBO;

public record RedisOptions
{
	public string ConnectionString { get; set; } = "localhost";
}
