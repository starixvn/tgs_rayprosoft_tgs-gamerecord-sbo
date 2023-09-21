using Tgs.GameRecord.SBO.Core.JobApplicationServices;

namespace Tgs.GameRecord.SBO.Core;

public interface IHangfireUtensil
{
    public Task JobCheckAsync();

    public Task JobExecuteAsync(JobExecuteCommand source);

    public Task JobInitAsync();
}
