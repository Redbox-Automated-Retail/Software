using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Redbox.UpdateManager.BITS;

public static class Utils
{
	private static BitsVersion version;

	internal static BitsVersion BITSVersion => version;

	static Utils()
	{
		version = GetBitsVersion();
	}

	internal static string GetName(string SID)
	{
		long cbName = 255L;
		long cbDomainName = 255L;
		IntPtr SID2 = new IntPtr(0);
		int psUse = 0;
		StringBuilder stringBuilder = new StringBuilder(255);
		StringBuilder builder = new StringBuilder(255);
		if (NativeMethods.ConvertStringSidToSidW(SID, ref SID2) && NativeMethods.LookupAccountSidW(string.Empty, SID2, stringBuilder, ref cbName, builder, ref cbDomainName, ref psUse))
		{
			return builder.ToString() + "\\" + stringBuilder.ToString();
		}
		return string.Empty;
	}

	internal static FILETIME DateTime2FileTime(DateTime dateTime)
	{
		long fileTime = 0L;
		if (dateTime != DateTime.MinValue)
		{
			fileTime = dateTime.ToFileTime();
		}
		return new FILETIME
		{
			dwLowDateTime = (uint)(fileTime & 0xFFFFFFFFu),
			dwHighDateTime = (uint)(fileTime >> 32)
		};
	}

	internal static DateTime FileTime2DateTime(FILETIME fileTime)
	{
		if (fileTime.dwHighDateTime == 0 && fileTime.dwLowDateTime == 0)
		{
			return DateTime.MinValue;
		}
		return DateTime.FromFileTime((long)(((ulong)fileTime.dwHighDateTime << 32) + fileTime.dwLowDateTime));
	}

	private static BitsVersion GetBitsVersion()
	{
		try
		{
			string sFileName = Path.Combine(System.Environment.SystemDirectory, "qmgr.dll");
			int handle = 0;
			int fileVersionInfoSize = NativeMethods.GetFileVersionInfoSize(sFileName, out handle);
			if (fileVersionInfoSize == 0)
			{
				return BitsVersion.Bits0_0;
			}
			byte[] buffer = new byte[fileVersionInfoSize];
			if (!NativeMethods.GetFileVersionInfo(sFileName, handle, fileVersionInfoSize, buffer))
			{
				return BitsVersion.Bits0_0;
			}
			IntPtr pValue = IntPtr.Zero;
			uint len = 0u;
			if (!NativeMethods.VerQueryValue(buffer, "\\VarFileInfo\\Translation", out pValue, out len))
			{
				return BitsVersion.Bits0_0;
			}
			int language = Marshal.ReadInt16(pValue);
			int codePage = Marshal.ReadInt16((IntPtr)((int)pValue + 2));
			string pSubBlock = $"\\StringFileInfo\\{language:X4}{codePage:X4}\\ProductVersion";
			if (!NativeMethods.VerQueryValue(buffer, pSubBlock, out string productVersion, out len))
			{
				return BitsVersion.Bits0_0;
			}
			string[] parts = productVersion.Split('.');
			if (parts == null || parts.Length < 2)
			{
				return BitsVersion.Bits0_0;
			}
			int major = int.Parse(parts[0]);
			int minor = int.Parse(parts[1]);
			return major switch
			{
				6 => minor switch
				{
					0 => BitsVersion.Bits1_0, 
					2 => BitsVersion.Bits1_2, 
					5 => BitsVersion.Bits1_5, 
					6 => BitsVersion.Bits2_0, 
					7 => BitsVersion.Bits2_5, 
					_ => BitsVersion.Bits0_0, 
				}, 
				7 => BitsVersion.Bits3_0, 
				_ => BitsVersion.Bits0_0, 
			};
		}
		catch
		{
			return BitsVersion.Bits0_0;
		}
	}
}
