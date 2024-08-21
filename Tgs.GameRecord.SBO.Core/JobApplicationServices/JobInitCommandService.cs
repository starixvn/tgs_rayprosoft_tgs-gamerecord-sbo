using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.GameInterface.SBOService.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBO.Core.JobApplicationServices;

internal class JobInitCommandService(
	ILogger<JobInitCommandService> logger,
	IOptions<GameRecordInfoOptions> options,
	IHangfireLockRepository hangfireLock,
	IGameCommandClient client,
	IGameInterfaceSBOClient gameInterfaceSBOClient) : ICommandService<JobInitCommand>
{
	private readonly GameRecordInfoOptions _gameRecordInfo = options.Value;

	private readonly RecurringJobOptions _recurringJobOptions = new()
	{
		TimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "China Standard Time" : "Asia/Shanghai")
	};

	public async ValueTask ExecuteAsync(JobInitCommand command, CancellationToken cancellationToken = default)
	{
		try
		{
			if (!await hangfireLock.LockingAsync(
				application: typeof(JobInitCommandService).Name,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false))
			{
				logger.LogWarning("LogOn:{logOn} Message:進行初始化動作，已經有其他服務進行", DateTime.Now.ToString("o"));
				return;
			}

			using (var connection = JobStorage.Current.GetConnection())
			{
				foreach (var recurringJob in connection.GetRecurringJobs())
				{
					RecurringJob.RemoveIfExists(recurringJob.Id);
				}
			}

			var settingConfiguration = await client.SettingGetConfigurationStringAsync(
				key: _gameRecordInfo.GameProviderConfiguration,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? throw new Exception($"This game product setting does not exist.({_gameRecordInfo.GameProviderConfiguration})");

			var settingConfigurationJson = JsonNode.Parse(settingConfiguration)
				?? throw new Exception($"This game product setting does not exist.({_gameRecordInfo.GameProviderConfiguration})");

			var gameProviderConfigs = await client.GameProviderConfigurationQueryAsync(
				gameProviderCode: _gameRecordInfo.GameProviderCode,
				cancellationToken: cancellationToken)
				.ToArrayAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? Enumerable.Empty<GameProviderConfigInfo>();

			if (gameProviderConfigs == null || !gameProviderConfigs.Any())
			{
				return;
			}

			var gameProviderAgentMapper = new Dictionary<string, JobExecuteSportsBookCommand>();

			foreach (var gameProviderConfig in gameProviderConfigs)
			{
				if (string.IsNullOrWhiteSpace(gameProviderConfig.ConfigurationData) || gameProviderConfig.Status == GameProviderConfigStatus.Inactive)
				{
					continue;
				}

				var configurationDataJson = JsonNode.Parse(gameProviderConfig.ConfigurationData)
					?? throw new Exception($"Game Provider Configuration does not exist.({gameProviderConfig.GameProviderConfigId})");

				var apiUrl = new Uri(configurationDataJson["ApiUrl"]?.ToString()
					?? settingConfigurationJson["ApiUrl"]?.ToString()
					?? throw new Exception($"Required parameters(ApiUrl)"));
				var companyKey = configurationDataJson["CompanyKey"]?.ToString()
					?? settingConfigurationJson["CompanyKey"]?.ToString()
					?? throw new Exception($"Required parameters(CompanyKey)");
				var agent = configurationDataJson["Agent"]?.ToString()
					?? throw new Exception($"Required parameters(Agent)");
				var currency = configurationDataJson["Currency"]?.ToString()
					?? throw new Exception($" Required parameters(Currency)");
				var min = configurationDataJson["Min"]?.GetValue<int>()
					?? settingConfigurationJson["Min"]?.GetValue<int>()
					?? throw new Exception($"Required parameters(Min)");
				var max = configurationDataJson["Max"]?.GetValue<int>()
					?? settingConfigurationJson["Max"]?.GetValue<int>()
					?? throw new Exception($"Required parameters(Max)");
				var maxPerMatch = configurationDataJson["MaxPerMatch"]?.GetValue<int>()
					?? settingConfigurationJson["MaxPerMatch"]?.GetValue<int>()
					?? throw new Exception($" Required parameters(MaxPerMatch)");
				var casinoTableLimit = configurationDataJson["CasinoTableLimit"]?.GetValue<int>()
					?? settingConfigurationJson["CasinoTableLimit"]?.GetValue<int>()
					?? throw new Exception($" Required parameters(CasinoTableLimit)");

				if (gameProviderAgentMapper.ContainsKey(agent))
				{
					continue;
				}

				await gameInterfaceSBOClient.AgentPresetBetSettingsAsync(
					apiUrl: apiUrl,
					companyKey: companyKey,
					agent: agent,
					currency: currency,
					min: min,
					max: max,
					maxPerMatch: maxPerMatch,
					casinoTableLimit: casinoTableLimit,
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				var jobExecuteSportsBookCommand = new JobExecuteSportsBookCommand(
					ApiUrl: apiUrl,
					CompanyKey: companyKey,
					Agent: agent);

				var jobExecuteVirtualSportsCommand = new JobExecuteVirtualSportsCommand(
					ApiUrl: apiUrl,
					CompanyKey: companyKey,
					Agent: agent);

				var jobExecuteSeamlessGameCommand = new JobExecuteSeamlessGameCommand(
					ApiUrl: apiUrl,
					CompanyKey: companyKey,
					Agent: agent);

				gameProviderAgentMapper.Add(agent, jobExecuteSportsBookCommand);

				RecurringJob.AddOrUpdate<IHangfireUtensilRepository>(
					recurringJobId: $"{gameProviderConfig.Name}-SportsBook|{agent}|{gameProviderConfig.GameProviderConfigId}",
					methodCall: service => service.JobExecuteSportsBookAsync(jobExecuteSportsBookCommand),
					cronExpression: Cron.Minutely(),
					options: _recurringJobOptions);
				RecurringJob.AddOrUpdate<IHangfireUtensilRepository>(
					recurringJobId: $"{gameProviderConfig.Name}-VirtualSports|{agent}|{gameProviderConfig.GameProviderConfigId}",
					methodCall: service => service.JobExecuteVirtualSportsAsync(jobExecuteVirtualSportsCommand),
					cronExpression: Cron.Minutely(),
					options: _recurringJobOptions);
				RecurringJob.AddOrUpdate<IHangfireUtensilRepository>(
					recurringJobId: $"{gameProviderConfig.Name}-SeamlessGame|{agent}|{gameProviderConfig.GameProviderConfigId}",
					methodCall: service => service.JobExecuteSeamlessGameAsync(jobExecuteSeamlessGameCommand),
					cronExpression: Cron.Minutely(),
					options: _recurringJobOptions);

				//初始化強制第一次處理
				_ = BackgroundJob.Enqueue<IHangfireUtensilRepository>(service => service.JobExecuteSportsBookAsync(jobExecuteSportsBookCommand));
				_ = BackgroundJob.Enqueue<IHangfireUtensilRepository>(service => service.JobExecuteVirtualSportsAsync(jobExecuteVirtualSportsCommand));
				_ = BackgroundJob.Enqueue<IHangfireUtensilRepository>(service => service.JobExecuteSeamlessGameAsync(jobExecuteSeamlessGameCommand));
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "LogOn:{logOn} Message:遊戲廠商初始化排程發生例外", DateTime.Now.ToString("o"));
		}
		finally
		{
			_ = BackgroundJob.Schedule<IHangfireUtensilRepository>(service => service.JobCheckAsync(), TimeSpan.FromMinutes(1));
		}
	}
}
