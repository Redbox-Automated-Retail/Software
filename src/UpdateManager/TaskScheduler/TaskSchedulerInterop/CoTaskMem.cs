using System;
using System.Runtime.InteropServices;

namespace TaskSchedulerInterop;

public class CoTaskMem
{
	public static string LPWStrToString(IntPtr lpwstr)
	{
		string result = Marshal.PtrToStringUni(lpwstr);
		Marshal.FreeCoTaskMem(lpwstr);
		return result;
	}
}
