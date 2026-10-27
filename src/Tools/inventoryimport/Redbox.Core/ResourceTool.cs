using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Redbox.Core;

public static class ResourceTool
{
	internal delegate bool EnumResNameProc(IntPtr hModule, string pType, string pName, IntPtr param);

	public static byte[] GetResource(LibraryModule module, string group, string name)
	{
		if (group == null || name == null)
		{
			return null;
		}
		byte[] array = null;
		IntPtr intPtr = FindResource(module.Handle, name.ToUpper(), group.ToUpper());
		if (intPtr == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		int num = SizeofResource(module.Handle, intPtr);
		if (num > 0)
		{
			IntPtr intPtr2 = LoadResource(module.Handle, intPtr);
			if (intPtr2 == IntPtr.Zero)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			IntPtr intPtr3 = LockResource(intPtr2);
			if (intPtr3 != IntPtr.Zero)
			{
				array = new byte[num];
				Marshal.Copy(intPtr3, array, 0, num);
			}
		}
		return array;
	}

	public static int ForEachResourceDo(LibraryModule module, string group, Action<string> action)
	{
		return EnumResourceNames(module.Handle, group.ToUpper(), delegate(IntPtr hModule, string pType, string pName, IntPtr param)
		{
			action(pName);
			return true;
		}, IntPtr.Zero);
	}

	[DllImport("Kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "FindResourceW", SetLastError = true)]
	private static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);

	[DllImport("Kernel32.dll", SetLastError = true)]
	private static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResource);

	[DllImport("Kernel32.dll")]
	private static extern IntPtr LockResource(IntPtr hResData);

	[DllImport("Kernel32.dll", SetLastError = true)]
	private static extern int SizeofResource(IntPtr hModule, IntPtr hResInfo);

	[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int EnumResourceNames(IntPtr hModule, string pType, EnumResNameProc callback, IntPtr param);
}
