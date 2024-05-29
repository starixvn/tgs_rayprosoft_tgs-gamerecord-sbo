using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.SBO.ViewModels;

public record SBOGameRecordViewModel
{
	[Required]
	[DataType(DataType.Url)]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	public string CompanyKey { get; init; } = default!;
	[Required]
	public string Agent { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	public DateTimeOffset StartTime { get; init; }
	[Required]
	[DataType(DataType.DateTime)]
	public DateTimeOffset EndTime { get; init; }
}
