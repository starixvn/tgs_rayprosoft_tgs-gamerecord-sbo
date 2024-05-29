using Tgs.GameRecord.SBO.Core.JobApplicationServices;

namespace Tgs.GameRecord.SBO.Core;

public interface IHangfireUtensil
{
	public Task JobCheckAsync();

	public Task JobExecuteSeamlessGameAsync(JobExecuteSeamlessGameCommand source);

	public Task JobExecuteSportsBookAsync(JobExecuteSportsBookCommand source);

	public Task JobExecuteVirtualSportsAsync(JobExecuteVirtualSportsCommand source);

	public Task JobInitAsync();
}
