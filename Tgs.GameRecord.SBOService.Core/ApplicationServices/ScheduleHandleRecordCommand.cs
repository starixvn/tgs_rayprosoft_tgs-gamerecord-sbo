using Tgs.GameInterface.SBOService.Protos;

namespace Tgs.GameRecord.SBOService.Core.ApplicationServices;

public record ScheduleHandleRecordCommand(
    string GameExternalId,
    GameHistoryInfo Record);
