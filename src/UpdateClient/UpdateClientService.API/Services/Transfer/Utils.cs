using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace UpdateClientService.API.Services.Transfer;

internal static class Utils
{
	private static readonly BitsVersion version;

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
		StringBuilder stringBuilder2 = new StringBuilder(255);
		if (NativeMethods.ConvertStringSidToSidW(SID, ref SID2) && NativeMethods.LookupAccountSidW(string.Empty, SID2, stringBuilder, ref cbName, stringBuilder2, ref cbDomainName, ref psUse))
		{
			return stringBuilder2.ToString() + "\\" + stringBuilder.ToString();
		}
		return string.Empty;
	}

	internal static FILETIME DateTime2FileTime(DateTime dateTime)
	{
		long num = 0L;
		if (dateTime != DateTime.MinValue)
		{
			num = dateTime.ToFileTime();
		}
		return new FILETIME
		{
			dwLowDateTime = (uint)(num & 0xFFFFFFFFu),
			dwHighDateTime = (uint)(num >> 32)
		};
	}

	internal static DateTime FileTime2DateTime(FILETIME fileTime)
	{
		if (fileTime.dwHighDateTime == 0 && fileTime.dwLowDateTime == 0)
		{
			return DateTime.MinValue;
		}
		long fileTime2 = (long)(((ulong)fileTime.dwHighDateTime << 32) + fileTime.dwLowDateTime);
		return DateTime.FromFileTime(fileTime2);
	}

	private static BitsVersion GetBitsVersion()
	{
		try
		{
			string sFileName = Path.Combine(Environment.SystemDirectory, "qmgr.dll");
			int fileVersionInfoSize = NativeMethods.GetFileVersionInfoSize(sFileName, out var handle);
			if (fileVersionInfoSize == 0)
			{
				return BitsVersion.Bits0_0;
			}
			byte[] array = new byte[fileVersionInfoSize];
			if (!NativeMethods.GetFileVersionInfo(sFileName, handle, fileVersionInfoSize, array))
			{
				return BitsVersion.Bits0_0;
			}
			IntPtr pValue = IntPtr.Zero;
			if (!NativeMethods.VerQueryValue(array, "\\VarFileInfo\\Translation", out pValue, out uint len))
			{
				return BitsVersion.Bits0_0;
			}
			int num = Marshal.ReadInt16(pValue);
			int num2 = Marshal.ReadInt16((IntPtr)((int)pValue + 2));
			string pSubBlock = $"\\StringFileInfo\\{num:X4}{num2:X4}\\ProductVersion";
			if (!NativeMethods.VerQueryValue(array, pSubBlock, out string pValue2, out len))
			{
				return BitsVersion.Bits0_0;
			}
			string[] array2 = pValue2.Split('.');
			if (array2 == null || array2.Length < 2)
			{
				return BitsVersion.Bits0_0;
			}
			int num3 = int.Parse(array2[0]);
			int num4 = int.Parse(array2[1]);
			return num3 switch
			{
				6 => num4 switch
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
