using Microsoft.Extensions.Options;
using Tgs.GameInterface.SBOService.Protos;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBOService.Core.ApplicationServices;

internal class HandleRecordCommandService : ICommandService<HandleRecordCommand>
{
    private readonly IGameRecordHandleClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;

    public HandleRecordCommandService(
        IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
        IGameRecordHandleClient client)
    {
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var status = command.Record.Status == GameHistoryStatus.Running ? GameRecordStatus.Open : GameRecordStatus.Closed;
        DateTimeOffset? externalClosedOn = command.Record.Status != GameHistoryStatus.Running ? command.Record.ExternalClosedOn.ToDateTimeOffset() : null;
        var settleAmount = command.Record.Status != GameHistoryStatus.Running ? command.Record.Settle.ToDecimal().GetValueOrDefault(0) : 0;
        var playerWinLoss = command.Record.Status != GameHistoryStatus.Running ? command.Record.PlayerWinLoss.ToDecimal().GetValueOrDefault(0) : 0;
        var clientStatus =
            command.Record.Status == GameHistoryStatus.Won
            ? GameRecordClientStatus.Win
            : command.Record.Status == GameHistoryStatus.Lose
            ? GameRecordClientStatus.Loss
            : command.Record.Status == GameHistoryStatus.Draw
            ? GameRecordClientStatus.Draw
            : command.Record.Status == GameHistoryStatus.Void
            ? GameRecordClientStatus.Void
            : GameRecordClientStatus.Open;

        await _client
            .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            externalRoundId: command.Record.ExternalRoundId,
            externalGameId: command.GameExternalId,
            playerCode: command.Record.PlayerCode,
            status: status,
            clientStatus: clientStatus,
            externalCreatedOn: command.Record.ExternalCreatedOn.ToDateTimeOffset(),
            externalClosedOn: externalClosedOn,
            bet: command.Record.Bet.ToDecimal().GetValueOrDefault(0),
            validBet: command.Record.ValidBet.ToDecimal().GetValueOrDefault(0),
            settle: settleAmount,
            commission: 0,
            turnover: command.Record.Turnover.ToDecimal().GetValueOrDefault(0),
            playerWinLoss: playerWinLoss,
            additionalData: command.Record.AdditionalData,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
