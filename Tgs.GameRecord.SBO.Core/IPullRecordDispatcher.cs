using Tgs.GameRecord.SBO.Core.ApplicationServices;

namespace Tgs.GameRecord.SBO.Core;

public interface IPullRecordDispatcher
{
	ValueTask PushCommandAsync(PullRecordCommand command, CancellationToken cancellationToken = default);
}
