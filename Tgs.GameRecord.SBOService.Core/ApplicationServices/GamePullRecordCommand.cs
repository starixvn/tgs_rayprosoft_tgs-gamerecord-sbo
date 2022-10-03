using Tgs.GameInterface.SBOService.Protos;

namespace Tgs.GameRecord.SBOService.Core.ApplicationServices;

public record GamePullRecordCommand(
    string GameExternalId,
    IEnumerable<GameHistoryInfo> Records);
