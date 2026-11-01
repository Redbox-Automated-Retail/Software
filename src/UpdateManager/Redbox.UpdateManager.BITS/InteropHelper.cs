using System;

namespace Redbox.UpdateManager.BITS;

public static class InteropHelper
{
	internal static DateTime? ToUTCDateTime(this FILETIME filetime)
	{
		long low = filetime.dwLowDateTime;
		long high = filetime.dwHighDateTime;
		if (low == 0L && high == 0L)
		{
			return null;
		}
		return DateTime.FromFileTimeUtc((high << 32) + low);
	}
}
