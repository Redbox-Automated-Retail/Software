using System;
using System.Runtime.InteropServices;

namespace TaskSchedulerInterop;

public class CoTaskMem
{
	public static string LPWStrToString(IntPtr lpwstr)
	{
		string result = Marshal.PtrToStringUni(lpwstr)!; // the COM getters always hand back an allocated string
		Marshal.FreeCoTaskMem(lpwstr);
		return result;
	}
}
