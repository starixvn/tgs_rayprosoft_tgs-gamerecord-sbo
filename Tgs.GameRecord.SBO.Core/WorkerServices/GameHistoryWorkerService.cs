using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameInterface.SBOService.Client;
using Tgs.GameInterface.SBOService.Protos;
using Tgs.GameRecord.SBO.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBO.Core.WorkerServices;

internal class GameHistoryWorkerService : ICommandService<GameHistoryWorkerCommand>
{
    private readonly IGameInterfaceSBOClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfoOptions;
    private readonly ILogger<GameHistoryWorkerService> _logger;
    private readonly ICommandService<PullRecordCommand> _service;

    public GameHistoryWorkerService(
        IOptions<GameRecordInfoOptions> options,
        IGameInterfaceSBOClient client,
        ILogger<GameHistoryWorkerService> logger,
        ICommandService<PullRecordCommand> service)
    {
        _gameRecordInfoOptions = options.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(GameHistoryWorkerCommand command, CancellationToken cancellationToken = default)
    {
        var gameExternal = _gameRecordInfoOptions.GameExternals.Where(x => x.Portfolio.Equals(command.Portfolio, StringComparison.OrdinalIgnoreCase)).Single();

        var gameHistoryInfos = gameExternal.Portfolio.Equals("VirtualSports", StringComparison.OrdinalIgnoreCase)
            ? await _client
            .GameWagerByVirtualSportsAsync(
                apiUrl: command.ApiUrl,
                companyKey: command.CompanyKey,
                agent: command.Agent,
                startTime: command.StartTime,
                endTime: command.EndTime,
                cancellationToken: cancellationToken)
            .ToArrayAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? Enumerable.Empty<GameWagerInfo>()
            : await _client
            .GameWagerBySportsBookAsync(
                apiUrl: command.ApiUrl,
                companyKey: command.CompanyKey,
                agent: command.Agent,
                startTime: command.StartTime,
                endTime: command.EndTime,
                cancellationToken: cancellationToken)
            .ToArrayAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? Enumerable.Empty<GameWagerInfo>();

        _logger.LogInformation("LogOn:{logOn} | Request:{request} | Response:{response}",
            DateTime.UtcNow.ToString("O"),
            command,
            gameHistoryInfos);

        if (!gameHistoryInfos.Any())
        {
            return;
        }

        var gamePullRecordCommand = new PullRecordCommand(
                GameExternalId: gameExternal.GameExternalId,
                Infos: gameHistoryInfos);

        await _service
            .ExecuteAsync(
                command: gamePullRecordCommand,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
