using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.SBO.ViewModels;

public record SBOGameRecordViewModel
{
	[Required]
	[DataType(DataType.Url)]
	[DefaultValue(typeof(Uri), "https://ex-api-yy.xxttgg.com")]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "A54D5BCEEE204E4EB799E6BB4E617937")]
	public string CompanyKey { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "PROD_SBO")]
	public string Agent { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2024-05-20T00:00:00Z")]
	public DateTimeOffset StartTime { get; init; }
	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2024-05-21T00:00:00Z")]
	public DateTimeOffset EndTime { get; init; }
}
