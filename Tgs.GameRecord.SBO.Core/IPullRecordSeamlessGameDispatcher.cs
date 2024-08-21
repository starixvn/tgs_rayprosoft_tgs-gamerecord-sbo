using Tgs.GameRecord.SBO.Core.ApplicationServices;

namespace Tgs.GameRecord.SBO.Core;

public interface IPullRecordSeamlessGameDispatcher
{
	ValueTask PushCommandAsync(PullRecordSeamlessGameCommand command, CancellationToken cancellationToken = default);
}
