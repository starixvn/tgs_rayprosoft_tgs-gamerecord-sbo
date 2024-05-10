namespace Tgs.GameRecord.SBO.Core;

public record RedisOptions
{
	public string ConnectionString { get; set; } = "localhost";
}
