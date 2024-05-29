namespace Tgs.GameRecord.SBO.Core.JobApplicationServices;

public record JobExecuteSeamlessGameCommand(
	Uri ApiUrl,
	string CompanyKey,
	string Agent);
