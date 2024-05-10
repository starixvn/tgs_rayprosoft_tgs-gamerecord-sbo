using System.Text;
using System.Text.Json;
using Tgs.GameRecord.SBO.Common;
using Tgs.GameRecord.SBO.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.SBO.Messages;

internal class ScheduleMessageHandler(
	ILogger<ScheduleMessageHandler> logger,
	ICommandService<PullRecordCommand> service) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var json = Encoding.UTF8.GetString(data.Span);
		var message = JsonSerializer.Deserialize<SBOGameRecordMessage>(json)
			?? throw new NullReferenceException("The request data resolves to a null value");

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{message}",
			DateTime.Now.ToString("o"),
			subject,
			message);

		var command = new PullRecordCommand(
			message.ApiUrl,
			message.CompanyKey,
			message.Agent,
			message.Portfolio,
			message.StartTime,
			message.EndTime);

		return service.ExecuteAsync(
			command: command,
			cancellationToken: cancellationToken);
	}
}
