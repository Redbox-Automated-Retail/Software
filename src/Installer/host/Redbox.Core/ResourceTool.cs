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
		byte[] data = null;
		IntPtr resourceInfo = FindResource(module.Handle, name.ToUpper(), group.ToUpper());
		if (resourceInfo == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		int size = SizeofResource(module.Handle, resourceInfo);
		if (size > 0)
		{
			IntPtr resourceData = LoadResource(module.Handle, resourceInfo);
			if (resourceData == IntPtr.Zero)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			IntPtr lockedData = LockResource(resourceData);
			if (lockedData != IntPtr.Zero)
			{
				data = new byte[size];
				Marshal.Copy(lockedData, data, 0, size);
			}
		}
		return data;
	}

	public static int ForEachResourceDo(LibraryModule module, string group, Action<string> action)
	{
		return EnumResourceNames(module.Handle, group.ToUpper(), delegate(IntPtr hModule, string pType, string pName, IntPtr param)
		{
			action(pName);
			return true;
		}, IntPtr.Zero);
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "FindResourceW", SetLastError = true)]
	private static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResource);

	[DllImport("kernel32.dll")]
	private static extern IntPtr LockResource(IntPtr hResData);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern int SizeofResource(IntPtr hModule, IntPtr hResInfo);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int EnumResourceNames(IntPtr hModule, string pType, EnumResNameProc callback, IntPtr param);
}
