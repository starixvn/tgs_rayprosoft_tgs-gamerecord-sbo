using Tgs.GameInterface.SBOService.Protos;

namespace Tgs.GameRecord.SBO.Core.ApplicationServices;

public record HandleRecordCommand(
	string GameExternalId,
	GameWagerInfo Info);
