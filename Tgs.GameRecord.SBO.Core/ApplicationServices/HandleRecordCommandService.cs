using Microsoft.Extensions.Options;
using Tgs.GameInterface.SBOService.Protos;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBO.Core.ApplicationServices;

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

    public ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {

        DateTimeOffset? externalClosedOn = DateTimeOffset.UnixEpoch;
        var settleAmount = 0.0m;
        var playerWinLoss = 0.0m;
        var wagerState =
            command.Record.Status == GameHistoryStatus.Won
            ? WagerStates.Win
            : command.Record.Status == GameHistoryStatus.Lose
            ? WagerStates.Lose
            : command.Record.Status == GameHistoryStatus.Draw
            ? WagerStates.Draw
            : command.Record.Status == GameHistoryStatus.Void
            ? WagerStates.Void
            : WagerStates.Running;
        if (wagerState != WagerStates.Running)
        {
            externalClosedOn = command.Record.ExternalClosedOn.ToDateTimeOffset();
            settleAmount = command.Record.Settle.ToDecimal().GetValueOrDefault(0);
            playerWinLoss = command.Record.PlayerWinLoss.ToDecimal().GetValueOrDefault(0);
        }

        return _client
            .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            playerCode: command.Record.PlayerCode,
            wagerState: wagerState,
            wagerType: WagerTypes.Normal,
            freeGameState: FreeGameStates.None,
            gambleState: GambleStates.None,
            cashoutState: CashoutStates.None,
            resettleState: ResettleStates.None,
            externalRoundId: command.Record.ExternalRoundId,
            externalWagerId: command.Record.ExternalRoundId,
            externalGameId: command.GameExternalId,
            externalCreatedOn: command.Record.ExternalCreatedOn.ToDateTimeOffset(),
            externalClosedOn: externalClosedOn,
            externalPayoffOn: externalClosedOn,
            externalResettleOn: DateTimeOffset.UnixEpoch,
            bet: command.Record.Bet.ToDecimal().GetValueOrDefault(0),
            validBet: command.Record.ValidBet.ToDecimal().GetValueOrDefault(0),
            settle: settleAmount,
            commission: 0,
            turnover: command.Record.Turnover.ToDecimal().GetValueOrDefault(0),
            winLoss: playerWinLoss,
            additionalData: command.Record.AdditionalData,
            cancellationToken: cancellationToken);
    }
}
