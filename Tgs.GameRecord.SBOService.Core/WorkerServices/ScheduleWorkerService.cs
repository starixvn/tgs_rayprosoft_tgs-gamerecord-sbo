using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameInterface.SBOService.Client;
using Tgs.GameInterface.SBOService.Protos;
using Tgs.GameRecord.SBOService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBOService.Core.WorkerServices;

internal class ScheduleWorkerService : ICommandService<ScheduleWorkerCommand>
{
    private readonly IGameInterfaceSBOClient _client;
    private readonly ILogger<ScheduleWorkerService> _logger;
    private readonly ICommandService<GamePullRecordCommand> _service;
    private readonly GameRecordInfoOptions _gameRecordInfoOptions;

    public ScheduleWorkerService(
        IOptions<GameRecordInfoOptions> options,
        IGameInterfaceSBOClient client,
        ILogger<ScheduleWorkerService> logger,
        ICommandService<GamePullRecordCommand> service)
    {
        _gameRecordInfoOptions = options.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(ScheduleWorkerCommand command, CancellationToken cancellationToken = default)
    {
        var gameExternal = _gameRecordInfoOptions.GameExternals.Where(x => x.Portfolio.Equals(command.Portfolio, StringComparison.OrdinalIgnoreCase)).Single();

        var gameHistoryInfos = gameExternal.Portfolio.Equals("VirtualSports", StringComparison.OrdinalIgnoreCase)
            ? await _client
                   .GameHistoryByVirtualSportsAsync(
                       apiUrl: command.ApiUrl,
                       companyKey: command.CompanyKey,
                       agent: command.Agent,
                       startTime: command.StartTime,
                       endTime: command.EndTime,
                       cancellationToken: cancellationToken)
                   .ToArrayAsync(cancellationToken).ConfigureAwait(false) ?? Enumerable.Empty<GameHistoryInfo>()
            : await _client
                   .GameHistoryBySportsBookAsync(
                       apiUrl: command.ApiUrl,
                       companyKey: command.CompanyKey,
                       agent: command.Agent,
                       startTime: command.StartTime,
                       endTime: command.EndTime,
                       cancellationToken: cancellationToken)
                   .ToArrayAsync(cancellationToken).ConfigureAwait(false) ?? Enumerable.Empty<GameHistoryInfo>();

        _logger.LogInformation("LogOn:{logOn} | Request:{request} | Response:{response}",
            DateTime.UtcNow.ToString("O"),
            command,
            gameHistoryInfos);

        if (!gameHistoryInfos.Any())
        {
            return;
        }

        var gamePullRecordCommand = new GamePullRecordCommand(
                GameExternalId: gameExternal.GameExternalId,
                Records: gameHistoryInfos);

        await _service
            .ExecuteAsync(
                command: gamePullRecordCommand,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
