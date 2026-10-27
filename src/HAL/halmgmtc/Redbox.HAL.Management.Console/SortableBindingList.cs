using System;
using System.ComponentModel;

namespace Redbox.HAL.Management.Console;

public class SortableBindingList<T> : BindingList<T>
{
	protected override bool SupportsSortingCore => true;

	protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
	{
		int num = ((direction == ListSortDirection.Ascending) ? 1 : (-1));
		if (prop.PropertyType.GetInterface("IComparable") != null)
		{
			for (int i = 1; i < base.Items.Count; i++)
			{
				bool flag = true;
				for (int j = 1; j < base.Items.Count; j++)
				{
					IComparable obj = prop.GetValue(base.Items[j - 1]) as IComparable;
					IComparable obj2 = prop.GetValue(base.Items[j]) as IComparable;
					if (obj.CompareTo(obj2) * num > 0)
					{
						T value = base.Items[j - 1];
						base.Items[j - 1] = base.Items[j];
						base.Items[j] = value;
						flag = false;
					}
				}
				if (flag)
				{
					break;
				}
			}
			return;
		}
		for (int k = 1; k < base.Items.Count; k++)
		{
			bool flag2 = true;
			for (int l = 1; l < base.Items.Count; l++)
			{
				string strA = ((prop.GetValue(base.Items[l - 1]) != null) ? prop.GetValue(base.Items[l - 1]).ToString() : string.Empty);
				string strB = ((prop.GetValue(base.Items[l]) != null) ? prop.GetValue(base.Items[l]).ToString() : string.Empty);
				if (string.Compare(strA, strB) * num > 0)
				{
					T value2 = base.Items[l - 1];
					base.Items[l - 1] = base.Items[l];
					base.Items[l] = value2;
					flag2 = false;
				}
			}
			if (flag2)
			{
				break;
			}
		}
	}
}
