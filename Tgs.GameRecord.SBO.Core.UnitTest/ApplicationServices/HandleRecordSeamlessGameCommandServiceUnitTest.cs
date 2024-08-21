using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.SBO.Core.ApplicationServices;

namespace Tgs.GameRecord.SBO.Core.UnitTest.ApplicationServices;

public class HandleRecordSeamlessGameCommandServiceUnitTest
{
	[Fact]
	public async Task Lose()
	{
		var faceLogger = NullLogger<HandleRecordSeamlessGameCommandService>.Instance;
		var fakeGameRecordInfoOptions = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "SBOTF",
			GameExternals =
			[
				new GameExternal
				{
					Portfolio="SportsBook",
					GameExternalId="sports_book"
				},
				 new GameExternal
				{
					Portfolio="VirtualSports",
					GameExternalId="virtual_sports"
				},
				 new GameExternal
				{
					Portfolio="SeamlessGame",
					GameExternalId="seamless_game"
				}
			],
			GameProviderConfiguration = "SBOTFConfiguration",
			HangfirePrefix = "gamerecord-sbo-hangfire:",
			HangfireLocking = "gamerecord-sbo-locking:",
			IntervalMinutes = 3,
			IntervalMinutesMax = 15,
			IntervalMinutesMinimum = 3,
			LockMinute = 2
		});
		var fakePolicySettingsOptions = Options.Create(new PolicySettingsOptions
		{
			RetryCount = 1,
			RetrySeconds = 1,
		});
		var fakeGameRecordHandleClient = Substitute.For<IGameRecordHandleClient>();

		var command = new HandleRecordSeamlessGameCommand(

			ExternalGameId: "seamless_game",
			Info: new GameInterface.SBOService.Protos.GameRecordBySeamlessGameInfo
			{
				PlayerCode = "4umv4giizorc",
				ExternalRoundId = "Funky_fkg_300344323845",
				ExternalWagerId = "FunkyGames_55440870_Funky_fkg_300344323845",
				ExternalGameId = "FunkyGames",
				ExternalCreatedOn = DateTimeOffset.Parse("2024-05-20T06:57:23.21-04:00").ToTimestamp(),
				ExternalClosedOn = DateTimeOffset.Parse("2024-05-20T06:57:26.9-04:00").ToTimestamp(),
				Status = GameInterface.SBOService.Protos.GameHistoryStatus.Lose,
				Bet = 50.0m,
				Settle = 0m,
				Turnover = 50.0m,
				WinLoss = -50.0m,
				AdditionalData = "{\"mainBetRef\":null,\"gamePeriodId\":\"Funky_fkg_300344323845\",\"gameRoundId\":\"Funky_fkg_300344323845\",\"gameType\":\"FunkyGames\",\"turnoverStake\":50.000000,\"orderDetail\":\"\",\"gameResult\":\"Funky_fkg_300344323845\",\"gameId\":286,\"gpId\":16,\"isGameProviderPromotion\":false,\"orderTime\":\"2024-05-20T06:57:23.210\",\"modifyDate\":\"2024-05-20T06:57:26.900\",\"settleTime\":\"2024-05-20T06:57:26.900\",\"winLostDate\":\"2024-05-20T00:00:00.000\",\"refNo\":\"FunkyGames_55440870_Funky_fkg_300344323845\",\"username\":\"PROD_4umv4giizorc\",\"currency\":\"IDR\",\"stake\":50.000000,\"winLost\":-50.000000,\"status\":\"lose\",\"topDownline\":null}"
			});

		var sut = new HandleRecordSeamlessGameCommandService(
			faceLogger,
			fakeGameRecordInfoOptions,
			fakePolicySettingsOptions,
			fakeGameRecordHandleClient);

		await sut.ExecuteAsync(command);

		_ = fakeGameRecordHandleClient
			.Received(1)
			.HandleAsync(
			source: Arg.Is(new Handle.Common.ViewModels.GameRecordHandleViewModel
			{
				GameProviderCode = fakeGameRecordInfoOptions.Value.GameProviderCode,
				PlayerCode = command.Info.PlayerCode,
				ExternalRoundId = "Funky_fkg_300344323845",
				ExternalBetId = "FunkyGames_55440870_Funky_fkg_300344323845",
				ExternalGameId = "seamless_game",
				ExternalCreatedOn = command.Info.ExternalCreatedOn.ToDateTimeOffset(),
				ExternalClosedOn = command.Info.ExternalClosedOn.ToDateTimeOffset(),
				ExternalPayoffOn = command.Info.ExternalClosedOn.ToDateTimeOffset(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch,
				BetState = Handle.Common.Protos.BetState.Loss,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 50.0m,
				ValidBet = 50.0m,
				Settle = 0.0M,
				Commission = 0.0M,
				Turnover = 50.0m,
				WinLoss = -50.0m,
				Jackpot = 0.0M,
				AdditionalData = command.Info.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Win()
	{
		var faceLogger = NullLogger<HandleRecordSeamlessGameCommandService>.Instance;
		var fakeGameRecordInfoOptions = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "SBOTF",
			GameExternals =
			[
				new GameExternal
				{
					Portfolio="SportsBook",
					GameExternalId="sports_book"
				},
				 new GameExternal
				{
					Portfolio="VirtualSports",
					GameExternalId="virtual_sports"
				},
				 new GameExternal
				{
					Portfolio="SeamlessGame",
					GameExternalId="seamless_game"
				}
			],
			GameProviderConfiguration = "SBOTFConfiguration",
			HangfirePrefix = "gamerecord-sbo-hangfire:",
			HangfireLocking = "gamerecord-sbo-locking:",
			IntervalMinutes = 3,
			IntervalMinutesMax = 15,
			IntervalMinutesMinimum = 3,
			LockMinute = 2
		});
		var fakePolicySettingsOptions = Options.Create(new PolicySettingsOptions
		{
			RetryCount = 1,
			RetrySeconds = 1,
		});
		var fakeGameRecordHandleClient = Substitute.For<IGameRecordHandleClient>();

		var command = new HandleRecordSeamlessGameCommand(

			ExternalGameId: "seamless_game",
			Info: new GameInterface.SBOService.Protos.GameRecordBySeamlessGameInfo
			{
				PlayerCode = "4umv4giizorc",
				ExternalRoundId = "Funky_fkg_300344324325",
				ExternalWagerId = "FunkyGames_55440870_Funky_fkg_300344324325",
				ExternalGameId = "FunkyGames",
				ExternalCreatedOn = DateTimeOffset.Parse("2024-05-20T06:58:42.047-04:00").ToTimestamp(),
				ExternalClosedOn = DateTimeOffset.Parse("2024-05-20T06:58:47.467-04:00").ToTimestamp(),
				Status = GameInterface.SBOService.Protos.GameHistoryStatus.Won,
				Bet = 1.0m,
				Settle = 1.66m,
				Turnover = 1.0m,
				WinLoss = 0.66m,
				AdditionalData = "{\"mainBetRef\":null,\"gamePeriodId\":\"Funky_fkg_300344324325\",\"gameRoundId\":\"Funky_fkg_300344324325\",\"gameType\":\"FunkyGames\",\"turnoverStake\":1.000000,\"orderDetail\":\"\",\"gameResult\":\"Funky_fkg_300344324325\",\"gameId\":285,\"gpId\":16,\"isGameProviderPromotion\":false,\"orderTime\":\"2024-05-20T06:58:42.047\",\"modifyDate\":\"2024-05-20T06:58:47.467\",\"settleTime\":\"2024-05-20T06:58:47.467\",\"winLostDate\":\"2024-05-20T00:00:00.000\",\"refNo\":\"FunkyGames_55440870_Funky_fkg_300344324325\",\"username\":\"PROD_4umv4giizorc\",\"currency\":\"IDR\",\"stake\":1.000000,\"winLost\":0.660000,\"status\":\"won\",\"topDownline\":null}"
			});

		var sut = new HandleRecordSeamlessGameCommandService(
			faceLogger,
			fakeGameRecordInfoOptions,
			fakePolicySettingsOptions,
			fakeGameRecordHandleClient);

		await sut.ExecuteAsync(command);

		_ = fakeGameRecordHandleClient
			.Received(1)
			.HandleAsync(
			source: Arg.Is(new Handle.Common.ViewModels.GameRecordHandleViewModel
			{
				GameProviderCode = fakeGameRecordInfoOptions.Value.GameProviderCode,
				PlayerCode = command.Info.PlayerCode,
				ExternalRoundId = "Funky_fkg_300344324325",
				ExternalBetId = "FunkyGames_55440870_Funky_fkg_300344324325",
				ExternalGameId = "seamless_game",
				ExternalCreatedOn = command.Info.ExternalCreatedOn.ToDateTimeOffset(),
				ExternalClosedOn = command.Info.ExternalClosedOn.ToDateTimeOffset(),
				ExternalPayoffOn = command.Info.ExternalClosedOn.ToDateTimeOffset(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch,
				BetState = Handle.Common.Protos.BetState.Win,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 1.0m,
				ValidBet = 1.0m,
				Settle = 1.66m,
				Commission = 0.0M,
				Turnover = 1.0m,
				WinLoss = 0.66m,
				Jackpot = 0.0M,
				AdditionalData = command.Info.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
