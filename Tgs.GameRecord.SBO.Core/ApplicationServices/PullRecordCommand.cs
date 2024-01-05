using Tgs.GameInterface.SBOService.Protos;

namespace Tgs.GameRecord.SBO.Core.ApplicationServices;

public record PullRecordCommand(
    Uri ApiUrl,
    string CompanyKey,
    string Agent,
    string Portfolio,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
