using System.Collections.Generic;
using System.Drawing;

namespace Redbox.KioskEngine.Drawing;

public static class RectangleHelper
{
	public static List<Rectangle> Merged(this IEnumerable<Rectangle> rects)
	{
		List<Rectangle> list = new List<Rectangle>();
		List<Rectangle> rectangles = new List<Rectangle>(rects);
		if (rectangles.Count == 0)
		{
			return list;
		}
		rectangles.Sort(delegate(Rectangle lhs, Rectangle rhs)
		{
			int num2 = lhs.Left.CompareTo(rhs.Left);
			int num3 = lhs.Top.CompareTo(rhs.Top);
			return (num2 <= num3) ? num2 : num3;
		});
		Rectangle rectangle = rectangles[0];
		for (int num = 1; num < rectangles.Count; num++)
		{
			Rectangle rectangle2 = rectangles[num];
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
