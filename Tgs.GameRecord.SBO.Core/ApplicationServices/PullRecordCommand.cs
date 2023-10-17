using Tgs.GameInterface.SBOService.Protos;

namespace Tgs.GameRecord.SBO.Core.ApplicationServices;

public record PullRecordCommand(
    string GameExternalId,
    IEnumerable<GameWagerInfo> Infos);
