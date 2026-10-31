using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002ELibraryModule : IDisposable
{
	private readonly IntPtr m_hModule;

	public IntPtr Handle => m_hModule;

	public Redbox_002EMacros1309_002ELibraryModule(string fileName)
	{
		m_hModule = LoadLibrary(fileName);
		if (m_hModule == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}

	public IntPtr GetProcAddress(string procName)
	{
		return GetProcAddress(m_hModule, procName);
	}

	public void Dispose()
	{
		if (m_hModule != IntPtr.Zero)
		{
			FreeLibrary(m_hModule);
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryW", SetLastError = true)]
	private static extern IntPtr LoadLibrary(string lpFileName);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool FreeLibrary(IntPtr hModule);

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
}
