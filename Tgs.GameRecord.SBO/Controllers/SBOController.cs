using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.SBO.Common;
using Tgs.GameRecord.SBO.ViewModels;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.SBO.Controllers;

[Route("[controller]")]
[ApiController]
public class SBOController : ControllerBase
{
	/// <summary>
	/// Games the record schedule sports book asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("schedule/sportsbook")]
	public ValueTask GameRecordScheduleSportsBookAsync(
		[FromServices] IMessageSender messageSender,
		SBOGameRecordViewModel source)
		=> messageSender.PublishAsync(
			subject: SBONatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(new SBOGameRecordMessage
			{
				ApiUrl = source.ApiUrl,
				Agent = source.Agent,
				CompanyKey = source.CompanyKey,
				Portfolio = "SportsBook",
				StartTime = source.StartTime,
				EndTime = source.EndTime
			}),
			cancellationToken: HttpContext.RequestAborted);

	/// <summary>
	/// Games the record schedule virtual sports asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("schedule/virtualsports")]
	public ValueTask GameRecordScheduleVirtualSportsAsync(
		[FromServices] IMessageSender messageSender,
		SBOGameRecordViewModel source)
		=> messageSender.PublishAsync(
			subject: SBONatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(new SBOGameRecordMessage
			{
				ApiUrl = source.ApiUrl,
				Agent = source.Agent,
				CompanyKey = source.CompanyKey,
				Portfolio = "VirtualSports",
				StartTime = source.StartTime,
				EndTime = source.EndTime
			}),
			cancellationToken: HttpContext.RequestAborted);

	/// <summary>
	/// Games the record schedule seamless game asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("schedule/seamlessgame")]
	public ValueTask GameRecordScheduleSeamlessGameAsync(
		[FromServices] IMessageSender messageSender,
		SBOGameRecordViewModel source)
		=> messageSender.PublishAsync(
			subject: SBONatsConst.ScheduleSeamlessGamePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(new SBOGameRecordMessage
			{
				ApiUrl = source.ApiUrl,
				Agent = source.Agent,
				CompanyKey = source.CompanyKey,
				Portfolio = "SeamlessGame",
				StartTime = source.StartTime,
				EndTime = source.EndTime
			}),
			cancellationToken: HttpContext.RequestAborted);

	/// <summary>
	/// Games the record manual asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("manual/sportsbook")]
	public ValueTask GameRecordManualAsync(
		[FromServices] IMessageSender messageSender,
		SBOGameRecordViewModel source)
		=> messageSender.PublishAsync(
			subject: SBONatsConst.ManualPublish,
			data: JsonSerializer.SerializeToUtf8Bytes(new SBOGameRecordMessage
			{
				ApiUrl = source.ApiUrl,
				Agent = source.Agent,
				CompanyKey = source.CompanyKey,
				Portfolio = "SportsBook",
				StartTime = source.StartTime,
				EndTime = source.EndTime
			}),
			cancellationToken: HttpContext.RequestAborted);

	/// <summary>
	/// Games the record manual virtual sports asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("manual/virtualsports")]
	public ValueTask GameRecordManualVirtualSportsAsync(
		[FromServices] IMessageSender messageSender,
		SBOGameRecordViewModel source)
		=> messageSender.PublishAsync(
			subject: SBONatsConst.ManualPublish,
			data: JsonSerializer.SerializeToUtf8Bytes(new SBOGameRecordMessage
			{
				ApiUrl = source.ApiUrl,
				Agent = source.Agent,
				CompanyKey = source.CompanyKey,
				Portfolio = "VirtualSports",
				StartTime = source.StartTime,
				EndTime = source.EndTime
			}),
			cancellationToken: HttpContext.RequestAborted);

	/// <summary>
	/// Games the record manual seamless game asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("manual/seamlessgame")]
	public ValueTask GameRecordManualSeamlessGameAsync(
		[FromServices] IMessageSender messageSender,
		SBOGameRecordViewModel source)
		=> messageSender.PublishAsync(
			subject: SBONatsConst.ManualSeamlessGamePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(new SBOGameRecordMessage
			{
				ApiUrl = source.ApiUrl,
				Agent = source.Agent,
				CompanyKey = source.CompanyKey,
				Portfolio = "SeamlessGame",
				StartTime = source.StartTime,
				EndTime = source.EndTime
			}),
			cancellationToken: HttpContext.RequestAborted);
}
