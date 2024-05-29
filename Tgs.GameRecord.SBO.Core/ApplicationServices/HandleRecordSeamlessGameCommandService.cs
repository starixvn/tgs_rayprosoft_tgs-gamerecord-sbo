using System.Text.Json.Nodes;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameInterface.SBOService.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.SBO.Core.ApplicationServices;

internal class HandleRecordSeamlessGameCommandService(
	ILogger<HandleRecordSeamlessGameCommandService> logger,
	IOptions<GameRecordInfoOptions> options,
	IMessageSender messageSender) : ICommandService<HandleRecordSeamlessGameCommand>
{
	private readonly GameRecordInfoOptions _gameRecordInfo = options.Value;

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

		var request = new Handle.Common.Protos.GameRecordHandleRequest
		{
			GameProviderCode = _gameRecordInfo.GameProviderCode,
			PlayerCode = command.Info.PlayerCode,
			ExternalRoundId = command.Info.ExternalRoundId,
			ExternalBetId = command.Info.ExternalWagerId,
			ExternalGameId = command.ExternalGameId,
			ExternalCreatedOn = command.Info.ExternalCreatedOn,
			ExternalClosedOn = externalClosedOn.ToTimestamp(),
			ExternalPayoffOn = externalClosedOn.ToTimestamp(),
			ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
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

		await messageSender.PublishAsync(
			subject: Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest,
			data: request,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This publication requests information.({request})",
			DateTime.Now.ToString("o"),
			Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest,
			new
			{
				request.GameProviderCode,
				request.PlayerCode,
				request.ExternalRoundId,
				request.ExternalBetId,
				request.ExternalGameId,
				ExternalCreatedOn = request.ExternalCreatedOn.ToDateTimeOffset(),
				ExternalClosedOn = request.ExternalClosedOn.ToDateTimeOffset(),
				request.BetState
			});
	}
}
