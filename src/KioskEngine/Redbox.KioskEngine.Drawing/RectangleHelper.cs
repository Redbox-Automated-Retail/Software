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
			int byLeft = lhs.Left.CompareTo(rhs.Left);
			int byTop = lhs.Top.CompareTo(rhs.Top);
			return (byLeft <= byTop) ? byLeft : byTop;
		});
		Rectangle rectangle = rectangles[0];
		for (int i = 1; i < rectangles.Count; i++)
		{
			Rectangle next = rectangles[i];
			if (rectangle.IntersectsWith(next))
			{
				rectangle = Rectangle.Union(rectangle, next);
				continue;
			}
			list.Add(rectangle);
			rectangle = next;
		}
		list.Add(rectangle);
		return list;
	}
}
