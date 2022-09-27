using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Tgs.GameInterface.SBOService.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBOService.Core.ApplicationServices;

internal class GamePullRecordCommandService : ICommandService<GamePullRecordCommand>
{
    private readonly ILogger<GamePullRecordCommandService> _logger;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly IMessageSender _messageSender;
    private readonly IUtensil _utensil;

    public GamePullRecordCommandService(
        ILogger<GamePullRecordCommandService> logger,
        IOptions<GameRecordInfoOptions> options,
        IMessageSender messageSender,
        IUtensil utensil)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameRecordInfo = options.Value;
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask ExecuteAsync(GamePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _utensil.GetGameProviderIdAsync().ConfigureAwait(false);
        var gameId = await _utensil.GetGameIdAsync(gameProviderId, command.GameExternalId).ConfigureAwait(false);
        foreach (var record in command.Records)
        {
            bool success = true;
            var playerModel = await _utensil.GetPlayerAsync(record.PlayerCode).ConfigureAwait(false);

            if (playerModel == null)
            {
                success = false;
            }

            Guid? gameProviderConfigurationId = default;
            if (playerModel != null)
            {
                gameProviderConfigurationId = await _utensil.BrandGetGameProviderConfigurationIdAsync(playerModel.BrandId, gameProviderId).ConfigureAwait(false);
                if (!gameProviderConfigurationId.HasValue)
                {
                    success = false;
                }
            }

            var status = record.Status == GameHistoryStatus.Running ? GameRoundStatus.Open : GameRoundStatus.Closed;
            DateTimeOffset? externalClosedOn = record.Status != GameHistoryStatus.Running ? record.ExternalClosedOn.ToDateTimeOffset() : null;
            var settleAmount = record.Status != GameHistoryStatus.Running ? record.Settle.ToDecimal() : null;
            var playerWinLoss = record.Status != GameHistoryStatus.Running ? record.PlayerWinLoss.ToDecimal() : null;
            var clientStatus =
                record.Status == GameHistoryStatus.Won
                ? GameRoundClientStatus.Win
                : record.Status == GameHistoryStatus.Lose
                ? GameRoundClientStatus.Loss
                : record.Status == GameHistoryStatus.Draw
                ? GameRoundClientStatus.Draw
                : record.Status == GameHistoryStatus.Void
                ? GameRoundClientStatus.Void
                : GameRoundClientStatus.Open;
            var data = new GameRoundData(
                ExternalRoundId: record.ExternalRoundId,
                GameProviderId: gameProviderId,
                GameProviderCode: _gameRecordInfo.GameProviderCode,
                PlayerCode: (playerModel == null) ? string.Empty : playerModel.PlayerCode,
                GameId: gameId,
                ExternalGameId: record.ExternalGameId,
                CurrencyCode: (playerModel == null) ? string.Empty : playerModel.DefaultWalletCurrencyCode,
                Status: status,
                GameProviderConfigurationId: gameProviderConfigurationId ?? Guid.Empty,
                WalletId: (playerModel == null) ? Guid.Empty : playerModel.DefaultWalletId,
                ExternalCreatedOn: record.ExternalCreatedOn.ToDateTimeOffset(),
                ExternalClosedOn: record.ExternalClosedOn.ToDateTimeOffset(),
                BrandId: (playerModel == null) ? Guid.Empty : playerModel.BrandId,
                BetAmount: record.Bet.ToDecimal(),
                ValidBetAmount: record.ValidBet.ToDecimal(),
                SettleAmount: settleAmount,
                Commission: 0,
                Turnover: record.Turnover.ToDecimal(),
                PlayerWinLoss: playerWinLoss,
                AdditionalData: record.AdditionalData,
                ClientStatus: clientStatus,
                Success: success);

            var subject = _gameRecordInfo.MongoGlobPattern;

            _logger.LogInformation("LogOn:{logOn}  Subject:{subject} MessageRequest:{request}", DateTime.Now.ToString("o"), subject, data);

            await _messageSender
                    .PublishAsync(
                        subject: subject,
                        data: data.ToBson(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
        }
    }
}
