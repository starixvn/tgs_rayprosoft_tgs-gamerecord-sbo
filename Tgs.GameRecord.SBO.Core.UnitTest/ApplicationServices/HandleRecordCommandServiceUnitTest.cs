using System.Text.Json.Nodes;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.SBOService.Protos;
using Tgs.GameRecord.SBO.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.SBO.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
	[Fact]
	public async Task Lose()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
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
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new HandleRecordCommand(

			GameExternalId: "sports_book",
			Info: new GameWagerInfo
			{
				ExternalWagerId = "147997932",
				PlayerCode = "4ujsi7wb6lqg",
				ExternalGameId = "Mix Parlay",
				Status = GameHistoryStatus.Lose,
				ExternalCreatedOn = DateTimeOffset.Parse("2023-09-19T12:28:26.75+00:00").ToTimestamp(),
				ExternalClosedOn = DateTimeOffset.Parse("2023-09-19T15:56:15.557+00:00").ToTimestamp(),
				Bet = 100.0m,
				ValidBet = 100.0m,
				PlayerWinLoss = -100.0m,
				Turnover = 100.0m,
				Settle = 0m,
				AdditionalData = "{\"subBet\":[{\"betOption\":\"1\",\"marketType\":\"First Half 1x2\",\"sportType\":\"Football\",\"hdp\":0.0,\"odds\":2.58,\"league\":\"ENGLISH LEAGUE ONE\",\"match\":\"Port Vale vs Burton Albion\",\"status\":\"running\",\"winlostDate\":\"2023-09-19T00:00:00\",\"liveScore\":\"0:0\",\"htScore\":\"1:1\",\"ftScore\":\"0:0\",\"customeizedBetType\":\"\",\"kickOffTime\":\"2023-09-19T14:45:00\",\"isHalfWonLose\":false},{\"betOption\":\"1\",\"marketType\":\"First Half 1x2\",\"sportType\":\"Football\",\"hdp\":0.0,\"odds\":2.46,\"league\":\"English Football League Trophy\",\"match\":\"Derby County vs Lincoln City\",\"status\":\"lose\",\"winlostDate\":\"2023-09-19T00:00:00\",\"liveScore\":\"0:0\",\"htScore\":\"0:0\",\"ftScore\":\"2:0\",\"customeizedBetType\":\"\",\"kickOffTime\":\"2023-09-19T14:00:00\",\"isHalfWonLose\":false},{\"betOption\":\"1\",\"marketType\":\"First Half 1x2\",\"sportType\":\"Football\",\"hdp\":0.0,\"odds\":2.98,\"league\":\"ENGLISH CHAMPIONSHIP\",\"match\":\"Preston North End vs Birmingham City\",\"status\":\"running\",\"winlostDate\":\"2023-09-19T00:00:00\",\"liveScore\":\"0:0\",\"htScore\":\"0:0\",\"ftScore\":\"0:0\",\"customeizedBetType\":\"\",\"kickOffTime\":\"2023-09-19T14:45:00\",\"isHalfWonLose\":false}],\"sportsType\":\"Mix Parlay\",\"odds\":18.913,\"oddsStyle\":\"E\",\"isLive\":false,\"maxWinWithoutActualStake\":1791.3,\"ip\":\"125.59.208.175\",\"isSystemTagRisky\":false,\"isCustomerTagRisky\":false,\"voidReason\":\"\",\"isCashOut\":false,\"actualStake\":100.0,\"turnover\":100.0,\"isHalfWonLose\":false,\"turnoverByStake\":100.0,\"turnoverByActualStake\":100.0,\"netTurnoverByStake\":100.0,\"netTurnoverByActualStake\":100.0,\"orderTime\":\"2023-09-19T12:28:26.75\",\"modifyDate\":\"2023-09-19T15:56:29.323\",\"settleTime\":\"2023-09-19T15:56:15.557\",\"winLostDate\":\"2023-09-19T00:00:00\",\"refNo\":\"147997932\",\"username\":\"PROD_4ujsi7wb6lqg\",\"currency\":\"HKD\",\"stake\":100.0,\"winLost\":-100.0,\"status\":\"lose\",\"topDownline\":null}"
			});

		var service = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.Info.PlayerCode,
				ExternalRoundId = command.Info.ExternalWagerId,
				ExternalBetId = command.Info.ExternalWagerId,
				ExternalGameId = "sports_book",
				ExternalCreatedOn = command.Info.ExternalCreatedOn,
				ExternalClosedOn = command.Info.ExternalClosedOn,
				ExternalPayoffOn = command.Info.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Loss,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 100.0m,
				ValidBet = 100.0m,
				Settle = 0.0M,
				Commission = 0.0M,
				Turnover = 100.0m,
				WinLoss = -100.0m,
				Jackpot = 0.0M,
				AdditionalData = command.Info.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Running()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
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
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new HandleRecordCommand(

			GameExternalId: "sports_book",
			Info: new GameWagerInfo
			{
				ExternalWagerId = "4606913",
				PlayerCode = "4ugse0rsnufp",
				ExternalGameId = "Football",
				Status = GameHistoryStatus.Running,
				ExternalCreatedOn = DateTimeOffset.Parse("2023-09-20T23:45:18.127+00:00").ToTimestamp(),
				ExternalClosedOn = DateTimeOffset.Parse("0001-01-01T00:00:00+00:00").ToTimestamp(),
				Bet = 5.0m,
				ValidBet = 5.0m,
				PlayerWinLoss = 0.0m,
				Turnover = 0.0m,
				Settle = 0.0m,
				AdditionalData = "{\"subBet\":[{\"betOption\":\"Over\",\"marketType\":\"Over/Under\",\"sportType\":\"Football\",\"hdp\":6.0,\"odds\":-0.95,\"league\":\"USA Major League Soccer\",\"match\":\"LA Galaxy vs Minnesota United\",\"status\":\"running\",\"winlostDate\":\"2023-09-21T00:00:00\",\"liveScore\":\"1:3\",\"htScore\":\"0:0\",\"ftScore\":\"0:0\",\"customeizedBetType\":\"\",\"kickOffTime\":\"2023-09-20T22:30:00\",\"isHalfWonLose\":false}],\"sportsType\":\"Football\",\"odds\":-0.95,\"oddsStyle\":\"M\",\"isLive\":true,\"maxWinWithoutActualStake\":5.0,\"ip\":\"114.33.41.237\",\"isSystemTagRisky\":false,\"isCustomerTagRisky\":false,\"voidReason\":\"\",\"isCashOut\":false,\"actualStake\":4.75,\"turnover\":5.0,\"isHalfWonLose\":false,\"turnoverByStake\":5.0,\"turnoverByActualStake\":4.75,\"netTurnoverByStake\":0.0,\"netTurnoverByActualStake\":0.0,\"orderTime\":\"2023-09-20T23:45:18.127\",\"modifyDate\":\"2023-09-20T23:45:25.98\",\"settleTime\":\"0001-01-01T00:00:00\",\"winLostDate\":\"0001-01-01T00:00:00\",\"refNo\":\"4606913\",\"username\":\"STAG_4ugse0rsnufp\",\"currency\":\"CNY\",\"stake\":5.0,\"winLost\":0.0,\"status\":\"running\",\"topDownline\":null}"
			});

		var service = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.Info.PlayerCode,
				ExternalRoundId = command.Info.ExternalWagerId,
				ExternalBetId = command.Info.ExternalWagerId,
				ExternalGameId = "sports_book",
				ExternalCreatedOn = command.Info.ExternalCreatedOn,
				ExternalClosedOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				ExternalPayoffOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Running,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 5.0m,
				ValidBet = 5.0m,
				Settle = 0.0M,
				Commission = 0.0M,
				Turnover = 0.0M,
				WinLoss = 0.0M,
				Jackpot = 0.0M,
				AdditionalData = command.Info.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Win()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
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
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var command = new HandleRecordCommand(

			GameExternalId: "sports_book",
			Info: new GameWagerInfo
			{
				ExternalWagerId = "148204004",
				PlayerCode = "4uisqakfevb5",
				ExternalGameId = "Football",
				Status = GameHistoryStatus.Won,
				ExternalCreatedOn = DateTimeOffset.Parse("2023-09-20T07:56:21.58+00:00").ToTimestamp(),
				ExternalClosedOn = DateTimeOffset.Parse("2023-09-20T07:59:59.41+00:00").ToTimestamp(),
				Bet = 1219.0m,
				ValidBet = 1219.0m,
				PlayerWinLoss = 60.95m,
				Turnover = 1219.0m,
				Settle = 1279.95m,
				AdditionalData = "{\"subBet\":[{\"betOption\":\"Buriram United\",\"marketType\":\"Handicap\",\"sportType\":\"Football\",\"hdp\":0.25,\"odds\":0.1,\"league\":\"AFC CHAMPIONS LEAGUE\",\"match\":\"Buriram United vs Zhejiang Pro\",\"status\":\"running\",\"winlostDate\":\"2023-09-20T00:00:00\",\"liveScore\":\"4:1\",\"htScore\":\"3:1\",\"ftScore\":\"0:0\",\"customeizedBetType\":\"\",\"kickOffTime\":\"2023-09-20T06:00:00\",\"isHalfWonLose\":false}],\"sportsType\":\"Football\",\"odds\":0.1,\"oddsStyle\":\"H\",\"isLive\":true,\"maxWinWithoutActualStake\":121.9,\"ip\":\"219.76.15.132\",\"isSystemTagRisky\":false,\"isCustomerTagRisky\":false,\"voidReason\":\"\",\"isCashOut\":false,\"actualStake\":1219.0,\"turnover\":1219.0,\"isHalfWonLose\":true,\"turnoverByStake\":1219.0,\"turnoverByActualStake\":1219.0,\"netTurnoverByStake\":609.5,\"netTurnoverByActualStake\":609.5,\"orderTime\":\"2023-09-20T07:56:21.58\",\"modifyDate\":\"2023-09-20T07:59:59.41\",\"settleTime\":\"2023-09-20T07:59:59.41\",\"winLostDate\":\"2023-09-20T00:00:00\",\"refNo\":\"148204004\",\"username\":\"PROD_4uisqakfevb5\",\"currency\":\"HKD\",\"stake\":1219.0,\"winLost\":60.95,\"status\":\"won\",\"topDownline\":null}"
			});

		var service = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await service.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.Info.PlayerCode,
				ExternalRoundId = command.Info.ExternalWagerId,
				ExternalBetId = command.Info.ExternalWagerId,
				ExternalGameId = "sports_book",
				ExternalCreatedOn = command.Info.ExternalCreatedOn,
				ExternalClosedOn = command.Info.ExternalClosedOn,
				ExternalPayoffOn = command.Info.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Win,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 1219.0m,
				ValidBet = 1219.0m,
				Settle = 1279.95m,
				Commission = 0.0M,
				Turnover = 1219.0m,
				WinLoss = 60.95m,
				Jackpot = 0.0M,
				AdditionalData = command.Info.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
