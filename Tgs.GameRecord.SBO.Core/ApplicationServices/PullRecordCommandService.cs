using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameInterface.SBOService.Client;
using Tgs.GameInterface.SBOService.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBO.Core.ApplicationServices;

internal class PullRecordCommandService(
    ILogger<PullRecordCommandService> logger,
    IOptions<GameRecordInfoOptions> options,
    IGameInterfaceSBOClient client,
    ICommandService<HandleRecordCommand> service) : ICommandService<PullRecordCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfoOptions = options.Value;

    public async ValueTask ExecuteAsync(PullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var gameExternal = _gameRecordInfoOptions.GameExternals.Where(x => x.Portfolio.Equals(command.Portfolio, StringComparison.OrdinalIgnoreCase)).Single();

        var gameHistoryInfos = gameExternal.Portfolio.Equals("VirtualSports", StringComparison.OrdinalIgnoreCase)
            ? await client.GameWagerByVirtualSportsAsync(
                apiUrl: command.ApiUrl,
                companyKey: command.CompanyKey,
                agent: command.Agent,
                startTime: command.StartTime,
                endTime: command.EndTime,
                cancellationToken: cancellationToken)
            .ToArrayAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? Enumerable.Empty<GameWagerInfo>()
            : await client.GameWagerBySportsBookAsync(
                apiUrl: command.ApiUrl,
                companyKey: command.CompanyKey,
                agent: command.Agent,
                startTime: command.StartTime,
                endTime: command.EndTime,
                cancellationToken: cancellationToken)
            .ToArrayAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? Enumerable.Empty<GameWagerInfo>();

        logger.LogInformation("LogOn:{logOn} | Request:{request} | Response:{response}",
            DateTime.UtcNow.ToString("O"),
            command,
            gameHistoryInfos);

        if (!gameHistoryInfos.Any())
        {
            return;
        }

        foreach (var info in gameHistoryInfos)
        {
            try
            {
                var recordCommand = new HandleRecordCommand(
                    GameExternalId: gameExternal.GameExternalId,
                    Info: info);
                await service.ExecuteAsync(
                     command: recordCommand,
                     cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "遊戲紀錄處理發生例外{info})", info);
            }
        }
    }
}
