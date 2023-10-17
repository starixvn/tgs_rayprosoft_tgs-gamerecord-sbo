namespace Tgs.GameRecord.SBO.Core.WorkerServices;

public record GameHistoryWorkerCommand(
    Uri ApiUrl,
    string CompanyKey,
    string Agent,
    string Portfolio,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
