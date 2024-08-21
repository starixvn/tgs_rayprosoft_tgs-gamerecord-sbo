using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameRecord.SBO.Common;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.SBO.Core.JobApplicationServices;

internal class JobExecuteVirtualSportsCommandService(
	ILogger<JobExecuteVirtualSportsCommandService> logger,
	IOptions<GameRecordInfoOptions> options,
	TimeProvider timeProvider,
	IMessageSender messageSender) : ICommandService<JobExecuteVirtualSportsCommand>
{
	private readonly double _intervalMinutes = options.Value.IntervalMinutesMax < options.Value.IntervalMinutes
		? options.Value.IntervalMinutesMax
		: options.Value.IntervalMinutesMinimum > options.Value.IntervalMinutes
		? options.Value.IntervalMinutesMinimum
		: options.Value.IntervalMinutes;

	public ValueTask ExecuteAsync(JobExecuteVirtualSportsCommand command, CancellationToken cancellationToken = default)
	{
		//每次查詢時間範圍最多為 15 分鐘，最新信息為 3 分鐘前，當前時間小於3分鐘會廠商會出現錯誤8006
		//當前時間22/09/12 14:15
		//查詢時間22/09/12 14:15~14:00會回應{"status":"8006","err_text":"Time does not meet."}
		//查詢時間22/09/12 14:12~13:56會回應{"status":"8009","err_text":"Time interval exceeds the allowable range."}
		//查詢時間22/09/12 14:12~13:57會回應{"status":"0000","err_text":"","data:":"[]"}
		var currentTime = timeProvider.GetUtcNow();
		var endTime = DateTimeOffset.ParseExact(currentTime.ToString(@"yyyy-MM-dd\THH:mm:00K"), @"yyyy-MM-dd\THH:mm:ssK", null, styles: System.Globalization.DateTimeStyles.RoundtripKind);
		var startTime = endTime.AddMinutes(_intervalMinutes * -1);

		var message = new SBOGameRecordMessage
		{
			ApiUrl = command.ApiUrl,
			CompanyKey = command.CompanyKey,
			Agent = command.Agent,
			Portfolio = "VirtualSports",
			StartTime = startTime,
			EndTime = endTime
		};

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = SBONatsConst.SchedulePublish,
				Data = new
				{
					message.ApiUrl,
					message.CompanyKey,
					message.Agent,
					message.Portfolio,
					StartTime = message.StartTime.ToString("u"),
					EndTime = message.EndTime.ToString("u")
				},
				Message = "This message publishing handler."
			}));

		return messageSender.PublishAsync(
			subject: SBONatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(message),
			cancellationToken: cancellationToken);
	}
}
