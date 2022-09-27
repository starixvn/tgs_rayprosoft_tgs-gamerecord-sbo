namespace Tgs.GameRecord.SBOService;

public record ScheduleMessageModel(
    Uri ApiUrl,
    string CompanyKey,
    string Agent,
    string Portfolio);
