using System;

namespace Redbox.Rental.Model;

public class InventoryDataJsonFile
{
	public DateTime? LastCompletedDate { get; set; }

	public DateTime? LastRunThinJobDate { get; set; }

	public DateTime? LastMerchOrderDate { get; set; }

	public DateTime? LastMerchOrdersReceivedDate { get; set; }
}
