using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Inventory;

public class _KioskTitlesCount
{
	public Dictionary<long, int> TitlesCount = new Dictionary<long, int>();

	public long KioskId { get; set; }
}
