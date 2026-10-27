using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Redbox.Core;

public static class Int32Extensions
{
	public static string FormatAsByteSize(this int fileSize)
	{
		StringBuilder stringBuilder = new StringBuilder(20);
		StrFormatByteSize(fileSize, stringBuilder, 20);
		return stringBuilder.ToString();
	}

	public static string FormatAsByteSize(this long fileSize)
	{
		StringBuilder stringBuilder = new StringBuilder(20);
		StrFormatByteSize(fileSize, stringBuilder, 20);
		return stringBuilder.ToString();
	}

	public static string FormatAsByteSize(this ulong fileSize)
	{
		StringBuilder stringBuilder = new StringBuilder(20);
		StrFormatByteSize(Convert.ToInt64(fileSize), stringBuilder, 20);
		return stringBuilder.ToString();
	}

	public static bool NotInRanges(this int number, Pair<int, int>[] ranges)
	{
		return !number.BetweenAnyRange(ranges);
	}

	public static bool BetweenAnyRange(this int theNumber, Pair<int, int>[] ranges)
	{
		bool flag = false;
		for (int i = 0; i < ranges.Length; i++)
		{
			flag = flag || theNumber.Between(ranges[i].First, ranges[i].Second);
		}
		return flag;
	}

	public static bool Between(this int theNumber, int min, int max)
	{
		if (theNumber >= min)
		{
			return theNumber <= max;
		}
		return false;
	}

	[DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
	private static extern long StrFormatByteSize(long fileSize, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);
}
