using System;

namespace Redbox.UpdateManager.BITS;

public static class InteropHelper
{
	internal static DateTime? ToUTCDateTime(this FILETIME filetime)
	{
		long num = filetime.dwLowDateTime;
		long num2 = filetime.dwHighDateTime;
		if (num == 0L && num2 == 0L)
		{
			return null;
		}
		return DateTime.FromFileTimeUtc((num2 << 32) + num);
	}
}
