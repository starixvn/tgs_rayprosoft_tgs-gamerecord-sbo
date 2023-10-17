using System.Text;
using System.Text.Json;
using Tgs.GameRecord.SBO.Core.WorkerServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.SBO.Messages;

public class ManualMessageHandler : IMessageProcessor
{
    private readonly ICommandService<GameHistoryWorkerCommand> _service;

    public ManualMessageHandler(ICommandService<GameHistoryWorkerCommand> service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var request = JsonSerializer.Deserialize<ManualMessageRequest>(json)!;

        var gameRecordCommand = new GameHistoryWorkerCommand(
            request.ApiUrl,
            request.CompanyKey,
            request.Agent,
            request.Portfolio,
            request.StartTime.UtcDateTime,
            request.EndTime.UtcDateTime);

        await _service.ExecuteAsync(
            command: gameRecordCommand,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var response = new ManualMessageResponse(true);
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
    }
}
