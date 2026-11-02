using System;

namespace Redbox.UpdateService.Client;

public class TransferStatistic
{
	public long ID { get; set; }

	public decimal Value { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string FileName { get; set; } = null!;

	public Identifier Store { get; set; } = null!;

	public DateTime CreatedOn { get; set; }

	public Identifier Revision { get; set; } = null!;

	public Identifier Repository { get; set; } = null!;

	public TransferStatisticType Type { get; set; }
}
