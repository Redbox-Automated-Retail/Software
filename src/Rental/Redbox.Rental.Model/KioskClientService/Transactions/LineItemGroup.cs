using System.Collections.Generic;
using System.Text;

namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class LineItemGroup
{
	public LineItemGroupType GroupType { get; set; }

	public List<LineItem> Items { get; set; } = new List<LineItem>();

	public Totals Totals { get; set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"GroupType: {GroupType}, ");
		if (Items != null)
		{
			stringBuilder.Append("Items: [(" + string.Join("), (", Items) + ")], ");
		}
		stringBuilder.Append($"Totals: ({Totals})");
		return stringBuilder.ToString();
	}
}
