using System.Collections.Generic;

namespace Redbox.Rental.Model;

public class InventoryActionFile
{
	public string CompletedDate { get; set; }

	public int FillQty { get; set; }

	public List<RebalancesIn> RebalancesIn { get; set; }
}
