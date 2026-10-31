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

	[DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
	private static extern long StrFormatByteSize(long fileSize, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);
}
