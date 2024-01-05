using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Tgs.GameInterface.SBOService.Protos;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBO.Core.ApplicationServices;

internal class HandleRecordCommandService(
    IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
    IGameRecordHandleClient client) : ICommandService<HandleRecordCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfo = gameRecordInfoOptions.Value;

    public ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        DateTimeOffset? externalClosedOn = DateTimeOffset.UnixEpoch;
        var settleAmount = 0.0m;
        var playerWinLoss = 0.0m;
        var wagerState =
            command.Info.Status == GameHistoryStatus.Won
            ? WagerStates.Win
            : command.Info.Status == GameHistoryStatus.Lose
            ? WagerStates.Lose
            : command.Info.Status == GameHistoryStatus.Draw
            ? WagerStates.Draw
            : command.Info.Status == GameHistoryStatus.Void
            ? WagerStates.Void
            : WagerStates.Running;
        if (wagerState != WagerStates.Running)
        {
            externalClosedOn = command.Info.ExternalClosedOn.ToDateTimeOffset();
            settleAmount = command.Info.Settle.ToDecimal().GetValueOrDefault(0);
            playerWinLoss = command.Info.PlayerWinLoss.ToDecimal().GetValueOrDefault(0);
        }

        return client
            .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            playerCode: command.Info.PlayerCode,
            wagerState: wagerState,
            wagerType: WagerTypes.Normal,
            freeGameState: FreeGameStates.None,
            gambleState: GambleStates.None,
            cashoutState: CashoutStates.None,
            resettleState: ResettleStates.None,
            externalRoundId: command.Info.ExternalWagerId,
            externalWagerId: command.Info.ExternalWagerId,
            externalGameId: command.GameExternalId,
            externalCreatedOn: command.Info.ExternalCreatedOn.ToDateTimeOffset(),
            externalClosedOn: externalClosedOn,
            externalPayoffOn: externalClosedOn,
            externalResettleOn: DateTimeOffset.UnixEpoch,
            bet: command.Info.Bet.ToDecimal().GetValueOrDefault(0),
            validBet: command.Info.ValidBet.ToDecimal().GetValueOrDefault(0),
            settle: settleAmount,
            commission: 0,
            turnover: command.Info.Turnover.ToDecimal().GetValueOrDefault(0),
            winLoss: playerWinLoss,
            additionalData: JsonNode.Parse(command.Info.AdditionalData)!,
            cancellationToken: cancellationToken);
    }
}
