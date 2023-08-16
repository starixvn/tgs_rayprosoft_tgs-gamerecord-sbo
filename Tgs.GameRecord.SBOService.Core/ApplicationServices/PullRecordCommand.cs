using Tgs.GameInterface.SBOService.Protos;

namespace Tgs.GameRecord.SBOService.Core.ApplicationServices;

public record PullRecordCommand(
    string GameExternalId,
    IEnumerable<GameHistoryInfo> Records);
