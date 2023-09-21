using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;
using System.Text;
using System.Text.Json;

namespace Tgs.GameRecord.SBO.Core.JobApplicationServices;

internal class JobExecuteCommandService : ICommandService<JobExecuteCommand>
{
    private readonly ILogger<JobExecuteCommandService> _logger;
    private readonly IMessageSender _messageSender;

    public JobExecuteCommandService(
        ILogger<JobExecuteCommandService> logger,
        IMessageSender messageSender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            command.ApiUrl,
            command.CompanyKey,
            command.Agent,
            command.Portfolio
        };

        _logger.LogDebug("LogOn:{logOn} Parameters:{parameters}", DateTime.Now.ToString("u"), parameters);

        return _messageSender
            .PublishAsync(
            subject: "gp.gamerecord.schedule.sbotf",
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(parameters)),
            cancellationToken: cancellationToken);
    }
}
