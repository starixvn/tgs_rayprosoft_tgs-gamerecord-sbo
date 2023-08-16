using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBOService.Core.ApplicationServices;

internal class PullRecordCommandService : ICommandService<PullRecordCommand>
{
    private readonly ILogger<PullRecordCommandService> _logger;
    private readonly ICommandService<HandleRecordCommand> _service;

    public PullRecordCommandService(
        ILogger<PullRecordCommandService> logger,
        ICommandService<HandleRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(PullRecordCommand command, CancellationToken cancellationToken = default)
    {
        foreach (var record in command.Records)
        {
            try
            {
                var recordCommand = new HandleRecordCommand(
                    GameExternalId: command.GameExternalId,
                    Record: record);
                await _service
                .ExecuteAsync(
                     command: recordCommand,
                     cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "遊戲紀錄處理發生例外recordhistory})", record);
            }
        }
    }
}
