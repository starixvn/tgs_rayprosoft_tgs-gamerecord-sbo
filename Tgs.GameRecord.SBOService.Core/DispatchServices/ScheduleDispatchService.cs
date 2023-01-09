using Microsoft.Extensions.Options;
using Tgs.GameRecord.SBOService.Core.WorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBOService.Core.DispatchServices;

internal class ScheduleDispatchService : ICommandService<ScheduleDispatchCommand>
{
    private readonly ICommandService<GameHistoryWorkerCommand> _commandService;
    private readonly GameRecordInfoOptions _gameRecordInfoOptions;
    private readonly double _intervalMinute;
    private readonly IScheduleDispatch _scheduleDispatch;

    public ScheduleDispatchService(
        IOptions<GameRecordInfoOptions> options,
        ICommandService<GameHistoryWorkerCommand> commandService,
        IScheduleDispatch scheduleDispatch
        )
    {
        _gameRecordInfoOptions = options.Value;
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _scheduleDispatch = scheduleDispatch ?? throw new ArgumentNullException(nameof(scheduleDispatch));
        _intervalMinute = (_gameRecordInfoOptions.IntervalMinutes > _gameRecordInfoOptions.IntervalMinutesMax)
                        ? _gameRecordInfoOptions.IntervalMinutesMax
                        : (_gameRecordInfoOptions.IntervalMinutes < _gameRecordInfoOptions.IntervalMinutesMinimum)
                        ? _gameRecordInfoOptions.IntervalMinutesMinimum
                        : _gameRecordInfoOptions.IntervalMinutes;
    }

    public async ValueTask ExecuteAsync(ScheduleDispatchCommand command, CancellationToken cancellationToken = default)
    {
        var key = $"gp:gamerecord:{_gameRecordInfoOptions.GameProviderCode}:{command.Portfolio}:{command.Agent}".ToLower();
        if (!await _scheduleDispatch.ExistAsync(key: key).ConfigureAwait(false))
        {
            if (await _scheduleDispatch.LockingAsync(key: key, command: command).ConfigureAwait(false))
            {
                try
                {
                    //每次查詢時間範圍最多為 15 分鐘，最新信息為 3 分鐘前，當前時間小於3分鐘會廠商會出現錯誤8006
                    //當前時間22/09/12 14:15
                    //查詢時間22/09/12 14:15~14:00會回應{"status":"8006","err_text":"Time does not meet."}
                    //查詢時間22/09/12 14:12~13:56會回應{"status":"8009","err_text":"Time interval exceeds the allowable range."}
                    //查詢時間22/09/12 14:12~13:57會回應{"status":"0000","err_text":"","data:":"[]"}
                    var endTime = DateTimeOffset.ParseExact(DateTimeOffset.UtcNow.ToString(@"yyyy-MM-dd\THH:mm:00K"), @"yyyy-MM-dd\THH:mm:ssK", null, styles: System.Globalization.DateTimeStyles.RoundtripKind);

                    var startTime = endTime.AddMinutes(_intervalMinute * -1);

                    var workerCommand = new GameHistoryWorkerCommand(
                         ApiUrl: command.ApiUrl,
                         CompanyKey: command.CompanyKey,
                         Agent: command.Agent,
                         Portfolio: command.Portfolio,
                         StartTime: startTime.UtcDateTime,
                         EndTime: endTime.UtcDateTime);

                    await _commandService
                            .ExecuteAsync(
                                command: workerCommand,
                                cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    await _scheduleDispatch.UnlockAsync(key: key).ConfigureAwait(false);
                }
            }
        }
    }
}
