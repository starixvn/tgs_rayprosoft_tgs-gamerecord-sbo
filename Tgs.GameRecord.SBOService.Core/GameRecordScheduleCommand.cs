namespace Tgs.GameRecord.SBOService.Core;

public record GameRecordScheduleCommand(
    Uri GameRecordApiUrl,
    Guid GameProviderId,
    string GameProviderCode,
    string GameProviderAgentId,
    string GameProviderCert,
    DateTimeOffset TimeFrom);
