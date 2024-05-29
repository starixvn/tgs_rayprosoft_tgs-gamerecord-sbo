namespace Tgs.GameRecord.SBO.Core.ApplicationServices;

public record HandleRecordSeamlessGameCommand(
	string ExternalGameId,
	GameInterface.SBOService.Protos.GameRecordBySeamlessGameInfo Info);
