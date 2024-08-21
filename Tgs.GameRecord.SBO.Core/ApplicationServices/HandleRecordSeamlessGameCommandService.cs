using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Tgs.GameInterface.SBOService.Protos;
using Tgs.GameRecord.Handle.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBO.Core.ApplicationServices;

internal class HandleRecordSeamlessGameCommandService(
	ILogger<HandleRecordSeamlessGameCommandService> logger,
	IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
	IOptions<PolicySettingsOptions> policySettingsOptions,
	IGameRecordHandleClient client) : ICommandService<HandleRecordSeamlessGameCommand>
{
	private readonly GameRecordInfoOptions _gameRecordInfoOptions = gameRecordInfoOptions.Value;
	private readonly PolicySettingsOptions _policySettingsOptions = policySettingsOptions.Value;

	public async ValueTask ExecuteAsync(HandleRecordSeamlessGameCommand command, CancellationToken cancellationToken = default)
	{
		DateTimeOffset externalClosedOn = DateTimeOffset.UnixEpoch;
		var settle = 0.0m;
		var winLoss = 0.0m;
		var betState =
			command.Info.Status == GameHistoryStatus.Won
			? Handle.Common.Protos.BetState.Win
			: command.Info.Status == GameHistoryStatus.Lose
			? Handle.Common.Protos.BetState.Loss
			: command.Info.Status == GameHistoryStatus.Draw
			? Handle.Common.Protos.BetState.Draw
			: command.Info.Status == GameHistoryStatus.Void
			? Handle.Common.Protos.BetState.Void
			: Handle.Common.Protos.BetState.Running;
		if (betState != Handle.Common.Protos.BetState.Running)
		{
			externalClosedOn = command.Info.ExternalClosedOn.ToDateTimeOffset();
			settle = command.Info.Settle.ToDecimal().GetValueOrDefault(0);
			winLoss = command.Info.WinLoss.ToDecimal().GetValueOrDefault(0);
		}

		var source = new Handle.Common.ViewModels.GameRecordHandleViewModel
		{
			GameProviderCode = _gameRecordInfoOptions.GameProviderCode,
			PlayerCode = command.Info.PlayerCode,
			ExternalRoundId = command.Info.ExternalRoundId,
			ExternalBetId = command.Info.ExternalWagerId,
			ExternalGameId = command.ExternalGameId,
			ExternalCreatedOn = command.Info.ExternalCreatedOn.ToDateTimeOffset(),
			ExternalClosedOn = externalClosedOn,
			ExternalPayoffOn = externalClosedOn,
			ExternalResettleOn = DateTimeOffset.UnixEpoch,
			BetState = betState,
			BetType = Handle.Common.Protos.BetType.Normal,
			FreeGameState = Handle.Common.Protos.FreeGameState.None,
			GambleState = Handle.Common.Protos.GambleState.None,
			CashoutState = Handle.Common.Protos.CashoutState.None,
			ResettleState = Handle.Common.Protos.ResettleState.None,
			JackpotState = Handle.Common.Protos.JackpotState.None,
			Bet = command.Info.Bet.ToDecimal().GetValueOrDefault(0),
			ValidBet = command.Info.Turnover.ToDecimal().GetValueOrDefault(0),
			Settle = settle,
			Commission = 0.0m,
			Turnover = command.Info.Turnover.ToDecimal().GetValueOrDefault(0),
			WinLoss = winLoss,
			Jackpot = 0.0M,
			AdditionalData = command.Info.AdditionalData
		};

		var retryCount = _policySettingsOptions.RetryCount;
		var retrySeconds = _policySettingsOptions.RetrySeconds;
		var retryPolicy = Policy
			.Handle<HttpRequestException>()
			.WaitAndRetryAsync(
				retryCount: retryCount,
				retryAttempt => TimeSpan.FromSeconds(Math.Pow(retrySeconds, retryAttempt)),
				onRetry: (exception, retryTime, retryCount, context) => logger.LogWarning(exception, "{logWarning}",
					new
					{
						LogOn = DateTime.Now.ToString("u"),
						RetryTime = retryTime,
						RetryCount = retryCount,
						Data = new
						{
							source.GameProviderCode,
							source.PlayerCode,
							source.ExternalRoundId,
							source.ExternalBetId,
							source.ExternalGameId,
							ExternalCreatedOn = source.ExternalCreatedOn.ToString("u"),
							ExternalClosedOn = source.ExternalClosedOn.ToString("u"),
							source.BetState
						},
						exception.Message
					}));

		await retryPolicy.ExecuteAsync(() => client.HandleAsync(
			source: source,
			cancellationToken: cancellationToken).AsTask())
			.ConfigureAwait(false);
	}
}
