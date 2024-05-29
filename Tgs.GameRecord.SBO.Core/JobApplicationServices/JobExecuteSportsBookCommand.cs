namespace Tgs.GameRecord.SBO.Core.JobApplicationServices;

public record JobExecuteSportsBookCommand(
	Uri ApiUrl,
	string CompanyKey,
	string Agent);
