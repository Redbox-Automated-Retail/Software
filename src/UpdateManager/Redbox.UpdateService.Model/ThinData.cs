using System;

namespace Redbox.UpdateService.Model;

public class ThinData
{
	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Barcode { get; set; } = null!;

	public long TitleId { get; set; }

	public byte ThinReason { get; set; }

	public long TotalRentalCount { get; set; }

	public bool IsFraud { get; set; }

	public DateTime Thin_Date { get; set; }

	public DateTime LastUpdate { get; set; }

	public long KioskID { get; set; }
}
