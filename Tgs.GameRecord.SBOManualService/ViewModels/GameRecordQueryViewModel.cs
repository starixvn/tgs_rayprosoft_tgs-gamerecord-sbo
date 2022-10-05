namespace Tgs.GameRecord.SBOManualService.ViewModels;

public record GameRecordQueryViewModel(
    Uri ApiUrl,
    string CompanyKey,
    string Agent,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
