using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.SBO.Common;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.SBO.Controllers;

[Route("[controller]")]
[ApiController]
public class SBOController : ControllerBase
{
    /// <summary>
    /// Games the record manual asynchronous.
    /// </summary>
    /// <param name="messageSender">The message sender.</param>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Post
    ///         {
    ///             "apiUrl": "https://ex-api-demo-yy.568win.com/",
    ///             "companyKey": "49ED28A987624CA0A3F834C3B8A69AAB",
    ///             "agent": "STAG_GPI_CNY",
    ///             "portfolio": "SportsBook",
    ///             "startTime": "2023-11-21T11:50:00+08:00",
    ///             "endTime": "2023-11-21T12:20:00+08:00"
    ///         }
    /// </remarks>
    [HttpPost("manual")]
    public ValueTask GameRecordManualAsync(
        [FromServices] IMessageSender messageSender,
        SBOGameRecordMessage source)
        => messageSender.PublishAsync(
            subject: SBONatsConst.ManualPublish,
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source)),
            cancellationToken: HttpContext.RequestAborted);

    /// <summary>
    /// Games the record schedule asynchronous.
    /// </summary>
    /// <param name="messageSender">The message sender.</param>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Post
    ///         {
    ///             "apiUrl": "https://ex-api-demo-yy.568win.com/",
    ///             "companyKey": "49ED28A987624CA0A3F834C3B8A69AAB",
    ///             "agent": "STAG_GPI_CNY",
    ///             "portfolio": "SportsBook",
    ///             "startTime": "2023-11-21T11:50:00+08:00",
    ///             "endTime": "2023-11-21T12:20:00+08:00"
    ///         }
    /// </remarks>
    [HttpPost("schedule")]
    public ValueTask GameRecordScheduleAsync(
        [FromServices] IMessageSender messageSender,
        SBOGameRecordMessage source)
        => messageSender.PublishAsync(
            subject: SBONatsConst.SchedulePublish,
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source)),
            cancellationToken: HttpContext.RequestAborted);
}
