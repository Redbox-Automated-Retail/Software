using System.Collections.Generic;
using KioskClientService.API.Vistar;

namespace KioskClientService.API.Ads;

public static class DisplayAreaExtensions
{
	public static List<VistarDisplayArea> ToVistar(this List<DisplayArea> list)
	{
		List<VistarDisplayArea> result = new List<VistarDisplayArea>();
		list.ForEach(delegate(DisplayArea da)
		{
			result.Add(da.ToVistarDisplayArea());
		});
		return result;
	}
}
