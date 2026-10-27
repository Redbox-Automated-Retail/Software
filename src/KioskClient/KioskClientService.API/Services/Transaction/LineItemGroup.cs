using System.Collections.Generic;

namespace KioskClientService.API.Services.Transaction;

public class LineItemGroup
{
	public LineItemGroupType GroupType { get; set; }

	public List<LineItem> Items { get; set; }

	public Totals Totals { get; set; }
}
