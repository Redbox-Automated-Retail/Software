using System;

namespace Redbox.UpdateService.Model;

public class ThinData
{
	public string Barcode { get; set; }

	public long TitleId { get; set; }

	public byte ThinReason { get; set; }

	public long TotalRentalCount { get; set; }

	public bool IsFraud { get; set; }

	public DateTime Thin_Date { get; set; }

	public DateTime LastUpdate { get; set; }

	public long KioskID { get; set; }
}
