using System.Linq;

namespace Redbox.NetCore.Middleware.Extensions;

public static class StringExtensions
{
	public static string ServiceName(this string callerLocation)
	{
		if (!callerLocation.Contains("\\"))
		{
			return callerLocation.Split('/').LastOrDefault()?.TrimEnd('.', 'c', 's');
		}
		return callerLocation.Split("\\").LastOrDefault()?.TrimEnd('.', 'c', 's');
	}
}
