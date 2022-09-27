using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.SBOManualService.ViewModels;
using Tgs.GameRecord.SBOService.Core.WorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBOManualService.Controllers;

[ApiController]
[Route("[controller]")]
public class ManualController : ControllerBase
{
    /// <summary>
    /// Games the history by sports book asynchronous.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="model">The model.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Post
    ///
    ///         {
    ///             "apiUrl": "https://ex-api-demo-yy.568win.com",
    ///             "companyKey": "49ED28A987624CA0A3F834C3B8A69AAB",
    ///             "agent": "STAG_GPI_CNY",
    ///             "startTime": "2022-09-22T17:00:00+08:00",
    ///             "endTime": "2022-09-22T17:30:00+08:00"
    ///         }
    /// </remarks>
    [HttpPost("sportsbook")]
    public ValueTask GameHistoryBySportsBookAsync(
        [FromServices] ICommandService<ManualWorkerCommand> service,
        GameRecordQueryViewModel model)
        => service.ExecuteAsync(new ManualWorkerCommand(
            ApiUrl: model.ApiUrl,
            CompanyKey: model.CompanyKey,
            Agent: model.Agent,
            Portfolio: "SportsBook",
            StartTime: model.StartTime.UtcDateTime,
            EndTime: model.EndTime.UtcDateTime));

    /// <summary>
    /// Games the history by virtual sports asynchronous.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="model">The model.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Post
    ///
    ///         {
    ///             "apiUrl": "https://ex-api-demo-yy.568win.com",
    ///             "companyKey": "49ED28A987624CA0A3F834C3B8A69AAB",
    ///             "agent": "STAG_GPI_CNY",
    ///             "startTime": "2022-09-22T16:30:00+08:00",
    ///             "endTime": "2022-09-22T17:00:00+08:00"
    ///         }
    /// </remarks>
    [HttpPost("virtualsports")]
    public ValueTask GameHistoryByVirtualSportsAsync(
        [FromServices] ICommandService<ManualWorkerCommand> service,
        GameRecordQueryViewModel model)
        => service.ExecuteAsync(new ManualWorkerCommand(
            ApiUrl: model.ApiUrl,
            CompanyKey: model.CompanyKey,
            Agent: model.Agent,
            Portfolio: "VirtualSports",
            StartTime: model.StartTime.UtcDateTime,
            EndTime: model.EndTime.UtcDateTime));
}
