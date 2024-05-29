namespace Tgs.GameRecord.SBO.Core.ApplicationServices;

public record PullRecordSeamlessGameCommand(
	Uri ApiUrl,
	string CompanyKey,
	string Agent,
	string Portfolio,
	DateTimeOffset StartTime,
	DateTimeOffset EndTime);
