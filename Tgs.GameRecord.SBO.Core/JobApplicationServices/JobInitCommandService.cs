using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.ServiceUtility;
using Tgs.GameInterface.SBOService.Client;

namespace Tgs.GameRecord.SBO.Core.JobApplicationServices;

internal class JobInitCommandService : ICommandService<JobInitCommand>
{
    private readonly IGameCommandClient _client;
    private readonly IGameInterfaceSBOClient _gameInterfaceSBOClient;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ILogger<JobInitCommandService> _logger;
    private readonly RecurringJobOptions _recurringJobOptions;

    public JobInitCommandService(
        ILogger<JobInitCommandService> logger,
        IOptions<GameRecordInfoOptions> options,
        IGameCommandClient client,
        IGameInterfaceSBOClient gameInterfaceSBOClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameRecordInfo = options.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _gameInterfaceSBOClient = gameInterfaceSBOClient ?? throw new ArgumentNullException(nameof(gameInterfaceSBOClient));
        _recurringJobOptions = new RecurringJobOptions()
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "China Standard Time" : "Asia/Shanghai")
        };
    }

    public async ValueTask ExecuteAsync(JobInitCommand command, CancellationToken cancellationToken = default)
    {
        using (var connection = JobStorage.Current.GetConnection())
        {
            foreach (var recurringJob in connection.GetRecurringJobs())
            {
                RecurringJob.RemoveIfExists(recurringJob.Id);
            }
        }

        try
        {
            var settingConfiguration = await _client
                .SettingGetConfigurationStringAsync(
                key: _gameRecordInfo.GameProviderConfiguration,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new Exception($"This game product setting does not exist.({_gameRecordInfo.GameProviderConfiguration})");

            var settingConfigurationJson = JsonNode.Parse(settingConfiguration)
                ?? throw new Exception($"This game product setting does not exist.({_gameRecordInfo.GameProviderConfiguration})");

            var gameProviderConfigs = await _client
                .GameProviderConfigurationQueryAsync(
                gameProviderCode: _gameRecordInfo.GameProviderCode,
                cancellationToken: cancellationToken)
                .ToArrayAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? Enumerable.Empty<GameProviderConfigInfo>();

            if (gameProviderConfigs == null || !gameProviderConfigs.Any())
            {
                return;
            }

            var gameProviderAgentMapper = new Dictionary<string, JobExecuteCommand>();

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

                if (gameProviderAgentMapper.ContainsKey(gameProviderConfig.Name))
                {
                    continue;
                }

                await _gameInterfaceSBOClient
                    .AgentPresetBetSettingsAsync(
                    apiUrl: apiUrl,
                    companyKey: companyKey,
                    username: agent,
                    currency: currency,
                    min: min,
                    max: max,
                    maxPerMatch: maxPerMatch,
                    casinoTableLimit: casinoTableLimit,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var sportsBookJobExecuteCommand = new JobExecuteCommand(
                    ApiUrl: apiUrl,
                    CompanyKey: companyKey,
                    Agent: agent,
                    Portfolio: "SportsBook");

                var virtualSportsJobExecuteCommand = new JobExecuteCommand(
                    ApiUrl: apiUrl,
                    CompanyKey: companyKey,
                    Agent: agent,
                    Portfolio: "VirtualSports");

                gameProviderAgentMapper.Add(gameProviderConfig.Name, sportsBookJobExecuteCommand);

                RecurringJob.AddOrUpdate<IHangfireUtensil>(
                    recurringJobId: $"{gameProviderConfig.Name}-{sportsBookJobExecuteCommand.Portfolio}",
                    methodCall: service => service.JobExecuteAsync(sportsBookJobExecuteCommand),
                    cronExpression: Cron.Minutely(),
                    options: _recurringJobOptions);
                RecurringJob.AddOrUpdate<IHangfireUtensil>(
                    recurringJobId: $"{gameProviderConfig.Name}-{virtualSportsJobExecuteCommand.Portfolio}",
                    methodCall: service => service.JobExecuteAsync(virtualSportsJobExecuteCommand),
                    cronExpression: Cron.Minutely(),
                    options: _recurringJobOptions);

                //初始化強制第一次處理
                _ = BackgroundJob.Enqueue<IHangfireUtensil>(service => service.JobExecuteAsync(sportsBookJobExecuteCommand));
                _ = BackgroundJob.Enqueue<IHangfireUtensil>(service => service.JobExecuteAsync(virtualSportsJobExecuteCommand));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LogOn:{logOn} Message:遊戲廠商初始化排程發生例外", DateTime.Now.ToString("o"));
        }
        finally
        {
            _ = BackgroundJob.Schedule<IHangfireUtensil>(service => service.JobCheckAsync(), TimeSpan.FromMinutes(1));
        }
    }
}
