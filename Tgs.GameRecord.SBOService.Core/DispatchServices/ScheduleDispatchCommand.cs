namespace Tgs.GameRecord.SBOService.Core.DispatchServices;

public record ScheduleDispatchCommand(
    Uri ApiUrl,
    string CompanyKey,
    string Agent,
    string Portfolio);
