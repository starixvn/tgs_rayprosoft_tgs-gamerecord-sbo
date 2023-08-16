using Tgs.GameInterface.SBOService.Protos;

namespace Tgs.GameRecord.SBOService.Core.ApplicationServices;

public record HandleRecordCommand(
    string GameExternalId,
    GameHistoryInfo Record);
