using System.Collections.Generic;
using System.Drawing;

namespace Redbox.KioskEngine.Drawing;

public static class RectangleHelper
{
	public static List<Rectangle> Merged(this IEnumerable<Rectangle> rects)
	{
		List<Rectangle> list = new List<Rectangle>();
		List<Rectangle> list2 = new List<Rectangle>(rects);
		if (list2.Count == 0)
		{
			return list;
		}
		list2.Sort(delegate(Rectangle lhs, Rectangle rhs)
		{
			int num2 = lhs.Left.CompareTo(rhs.Left);
			int num3 = lhs.Top.CompareTo(rhs.Top);
			return (num2 <= num3) ? num2 : num3;
		});
		Rectangle rectangle = list2[0];
		for (int num = 1; num < list2.Count; num++)
		{
			Rectangle rectangle2 = list2[num];
			if (rectangle.IntersectsWith(rectangle2))
			{
				rectangle = Rectangle.Union(rectangle, rectangle2);
				continue;
			}
			list.Add(rectangle);
			rectangle = rectangle2;
		}
		list.Add(rectangle);
		return list;
	}
}
