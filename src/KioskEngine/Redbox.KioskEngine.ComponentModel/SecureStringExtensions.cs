using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Redbox.KioskEngine.ComponentModel;

public static class SecureStringExtensions
{
	public static string SecureStringToString(this SecureString value)
	{
		if (value == null)
		{
			return null;
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = Marshal.SecureStringToGlobalAllocUnicode(value);
			return Marshal.PtrToStringUni(intPtr);
		}
		finally
		{
			Marshal.ZeroFreeGlobalAllocUnicode(intPtr);
		}
	}
}
