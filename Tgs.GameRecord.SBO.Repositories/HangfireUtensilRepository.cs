using Tgs.GameRecord.SBO.Core;

using Tgs.GameRecord.SBO.Core.JobApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.SBO.Repositories;

internal class HangfireUtensilRepository(
	ICommandService<JobCheckCommand> jobCheckService,
	ICommandService<JobExecuteSeamlessGameCommand> jobExecuteSeamlessGameService,
	ICommandService<JobExecuteSportsBookCommand> jobExecuteSportsBookService,
	ICommandService<JobExecuteVirtualSportsCommand> jobExecuteVirtualSportsService,
	ICommandService<JobInitCommand> jobInitservice) : IHangfireUtensilRepository
{
	public Task JobCheckAsync()
		=> jobCheckService.ExecuteAsync(new()).AsTask();

	public Task JobExecuteSeamlessGameAsync(JobExecuteSeamlessGameCommand source)
		 => jobExecuteSeamlessGameService.ExecuteAsync(source).AsTask();

	public Task JobExecuteSportsBookAsync(JobExecuteSportsBookCommand source)
		=> jobExecuteSportsBookService.ExecuteAsync(source).AsTask();

	public Task JobExecuteVirtualSportsAsync(JobExecuteVirtualSportsCommand source)
		 => jobExecuteVirtualSportsService.ExecuteAsync(source).AsTask();

	public Task JobInitAsync()
		=> jobInitservice.ExecuteAsync(new()).AsTask();
}
