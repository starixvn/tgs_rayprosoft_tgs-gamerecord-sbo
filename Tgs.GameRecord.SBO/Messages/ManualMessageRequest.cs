namespace Tgs.GameRecord.SBO.Messages;

public record ManualMessageRequest(
    Uri ApiUrl,
    string CompanyKey,
    string Agent,
    string Portfolio,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
