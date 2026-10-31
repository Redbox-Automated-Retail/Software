using System;
using System.Runtime.InteropServices;

namespace Redbox.Core;

public static class SelfRegistrationTool
{
	[return: MarshalAs(UnmanagedType.Error)]
	private delegate int DllRegisterDelegate();

	[return: MarshalAs(UnmanagedType.Error)]
	private delegate int DllUnregisterDelegate();

	public static void Register(string fileName)
	{
		using LibraryModule libraryModule = new LibraryModule(fileName);
		IntPtr procAddress = libraryModule.GetProcAddress("DllRegisterServer");
		if (!(procAddress == IntPtr.Zero))
		{
			Marshal.GetDelegateForFunctionPointer(procAddress, typeof(DllRegisterDelegate))?.DynamicInvoke(null);
		}
	}

	public static void Unregister(string fileName)
	{
		using LibraryModule libraryModule = new LibraryModule(fileName);
		IntPtr procAddress = libraryModule.GetProcAddress("DllUnregisterServer");
		if (!(procAddress == IntPtr.Zero))
		{
			Marshal.GetDelegateForFunctionPointer(procAddress, typeof(DllUnregisterDelegate))?.DynamicInvoke(null);
		}
	}

	public unsafe static bool IsModuleSelfRegistering(string fileName)
	{
		try
		{
			int fileVersionInfoSize = GetFileVersionInfoSize(fileName, out var handle);
			if (fileVersionInfoSize == 0)
			{
				return false;
			}
			byte[] array = new byte[fileVersionInfoSize];
			if (!GetFileVersionInfo(fileName, handle, fileVersionInfoSize, array))
			{
				return false;
			}
			if (!VerQueryValue(array, "\\VarFileInfo\\Translation", out short* pValue, out uint len))
			{
				return false;
			}
			string pSubBlock = "\\StringFileInfo\\" + pValue->ToString("X4") + pValue[1].ToString("X4") + "\\OLESelfRegister";
			string pValue2;
			return VerQueryValue(array, pSubBlock, out pValue2, out len);
		}
		catch (Exception)
		{
			return false;
		}
	}

	[DllImport("version.dll")]
	private static extern bool GetFileVersionInfo(string sFileName, int handle, int size, byte[] infoBuffer);

	[DllImport("version.dll")]
	private static extern int GetFileVersionInfoSize(string sFileName, out int handle);

	[DllImport("version.dll")]
	private static extern bool VerQueryValue(byte[] pBlock, string pSubBlock, out string pValue, out uint len);

	[DllImport("version.dll")]
	private unsafe static extern bool VerQueryValue(byte[] pBlock, string pSubBlock, out short* pValue, out uint len);
}
