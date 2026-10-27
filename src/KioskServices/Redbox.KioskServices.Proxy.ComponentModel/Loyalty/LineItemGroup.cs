using System.Collections.Generic;

namespace Redbox.KioskServices.Proxy.ComponentModel.Loyalty;

public class LineItemGroup
{
	public LineItemGroupType GroupType { get; set; }

	public List<LineItem> Items { get; set; }

	public Totals Totals { get; set; }

	public LineItemGroup()
	{
		Items = new List<LineItem>();
	}
}
