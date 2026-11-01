using System;
using System.ComponentModel;

namespace Redbox.HAL.Management.Console;

public class SortableBindingList<T> : BindingList<T>
{
	protected override bool SupportsSortingCore => true;

	protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
	{
		int sign = ((direction == ListSortDirection.Ascending) ? 1 : (-1));
		if (prop.PropertyType.GetInterface("IComparable") != null)
		{
			for (int i = 1; i < base.Items.Count; i++)
			{
				bool sorted = true;
				for (int j = 1; j < base.Items.Count; j++)
				{
					IComparable previous = prop.GetValue(base.Items[j - 1]) as IComparable;
					IComparable current = prop.GetValue(base.Items[j]) as IComparable;
					if (previous.CompareTo(current) * sign > 0)
					{
						T swap = base.Items[j - 1];
						base.Items[j - 1] = base.Items[j];
						base.Items[j] = swap;
						sorted = false;
					}
				}
				if (sorted)
				{
					break;
				}
			}
			return;
		}
		for (int k = 1; k < base.Items.Count; k++)
		{
			bool sortedText = true;
			for (int l = 1; l < base.Items.Count; l++)
			{
				string strA = ((prop.GetValue(base.Items[l - 1]) != null) ? prop.GetValue(base.Items[l - 1]).ToString() : string.Empty);
				string strB = ((prop.GetValue(base.Items[l]) != null) ? prop.GetValue(base.Items[l]).ToString() : string.Empty);
				if (string.Compare(strA, strB) * sign > 0)
				{
					T swapText = base.Items[l - 1];
					base.Items[l - 1] = base.Items[l];
					base.Items[l] = swapText;
					sortedText = false;
				}
			}
			if (sortedText)
			{
				break;
			}
		}
	}
}
