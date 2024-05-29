namespace Tgs.GameRecord.SBO.Core.JobApplicationServices;

public record JobExecuteVirtualSportsCommand(
	Uri ApiUrl,
	string CompanyKey,
	string Agent);
