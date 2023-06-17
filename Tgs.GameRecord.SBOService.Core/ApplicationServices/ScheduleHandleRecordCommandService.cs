using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.GameInterface.SBOService.Protos;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBOService.Core.ApplicationServices;

internal class ScheduleHandleRecordCommandService : ICommandService<ScheduleHandleRecordCommand>
{
    private readonly IGameRoundsWorkClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly IUtensil _utensil;

    public ScheduleHandleRecordCommandService(
        IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
        IUtensil utensil,
        IGameRoundsWorkClient client)
    {
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask ExecuteAsync(ScheduleHandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var gameId = await _utensil
            .GetGameIdAsync(
                gameProviderId,
                command.GameExternalId,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var success = true;

        var playerModel = await _utensil
            .GetPlayerAsync(
                playerCode: command.Record.PlayerCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (playerModel == null)
        {
            success = false;
        }

        Guid? gameProviderConfigurationId = default;
        if (playerModel != null)
        {
            gameProviderConfigurationId = await _utensil
                .GetBrandGameProviderConfigurationIdAsync(
                    brandId: playerModel.BrandId,
                    gameProviderId: gameProviderId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (!gameProviderConfigurationId.HasValue)
            {
                success = false;
            }
        }

        if (success)
        {
            var status = command.Record.Status == GameHistoryStatus.Running ? RoundStatus.Open : RoundStatus.Closed;
            DateTimeOffset? externalClosedOn = command.Record.Status != GameHistoryStatus.Running ? command.Record.ExternalClosedOn.ToDateTimeOffset() : null;
            var settleAmount = command.Record.Status != GameHistoryStatus.Running ? command.Record.Settle.ToDecimal().GetValueOrDefault(0) : 0;
            var playerWinLoss = command.Record.Status != GameHistoryStatus.Running ? command.Record.PlayerWinLoss.ToDecimal().GetValueOrDefault(0) : 0;
            var clientStatus =
                command.Record.Status == GameHistoryStatus.Won
                ? RoundClientStatus.Win
                : command.Record.Status == GameHistoryStatus.Lose
                ? RoundClientStatus.Loss
                : command.Record.Status == GameHistoryStatus.Draw
                ? RoundClientStatus.Draw
                : command.Record.Status == GameHistoryStatus.Void
                ? RoundClientStatus.Void
                : RoundClientStatus.Open;

            await _client
            .RoundBidding(
                gameProviderId: gameProviderId,
                gameProviderCode: _gameRecordInfo.GameProviderCode,
                externalRoundId: command.Record.ExternalRoundId,
                gameId: gameId,
                externalGameId: command.GameExternalId,
                playerCode: command.Record.PlayerCode,
                walletId: playerModel!.DefaultWalletId,
                currencyCode: playerModel!.DefaultWalletCurrencyCode,
                brandId: playerModel!.BrandId,
                gameProviderConfigurationId: gameProviderConfigurationId!.Value,
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
        else
        {
            await _client
            .RoundMissing(
                gameProviderId: gameProviderId,
                gameProviderCode: _gameRecordInfo.GameProviderCode,
                externalRoundId: command.Record.ExternalRoundId,
                gameId: gameId,
                externalGameId: command.GameExternalId,
                playerCode: command.Record.PlayerCode,
                data: JsonSerializer.Serialize(command.Record),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        }
    }
}
