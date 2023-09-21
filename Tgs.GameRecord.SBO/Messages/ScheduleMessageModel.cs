namespace Tgs.GameRecord.SBO.Messages;

public record ScheduleMessageModel(
    Uri ApiUrl,
    string CompanyKey,
    string Agent,
    string Portfolio);
