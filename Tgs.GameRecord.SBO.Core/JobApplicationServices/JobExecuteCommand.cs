namespace Tgs.GameRecord.SBO.Core.JobApplicationServices;

public record JobExecuteCommand(
    Uri ApiUrl,
    string CompanyKey,
    string Agent,
    string Portfolio);
