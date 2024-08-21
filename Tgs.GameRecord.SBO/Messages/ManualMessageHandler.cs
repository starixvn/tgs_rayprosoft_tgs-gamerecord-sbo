using System.Text;
using System.Text.Json;
using Tgs.GameRecord.SBO.Common;
using Tgs.GameRecord.SBO.Core;
using Tgs.GameRecord.SBO.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.SBO.Messages;

internal class ManualMessageHandler(
	ILogger<ManualMessageHandler> logger,
	IPullRecordDispatcher dispatcher) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var json = Encoding.UTF8.GetString(data.Span);
		var message = JsonSerializer.Deserialize<SBOGameRecordMessage>(json)
			?? throw new NullReferenceException("The request data resolves to a null value");

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = subject,
				Data = new
				{
					message.ApiUrl,
					message.CompanyKey,
					message.Agent,
					message.Portfolio,
					StartTime = message.StartTime.ToString("u"),
					EndTime = message.EndTime.ToString("u")
				},
				Message = "This message receiving handler."
			}));

		return dispatcher.PushCommandAsync(
			command: new PullRecordCommand(
				ApiUrl: message.ApiUrl,
				CompanyKey: message.CompanyKey,
				Agent: message.Agent,
				Portfolio: message.Portfolio,
				StartTime: message.StartTime,
				EndTime: message.EndTime),
			cancellationToken: cancellationToken);
	}
}
