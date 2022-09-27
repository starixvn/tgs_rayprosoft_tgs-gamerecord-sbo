namespace Tgs.GameRecord.SBOService.Core.WorkerServices;

public record ManualWorkerCommand(
    Uri ApiUrl,
    string CompanyKey,
    string Agent,
    string Portfolio,
    DateTime StartTime,
    DateTime EndTime);
