using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Redbox.Core;

public class LibraryModule : IDisposable
{
	private readonly IntPtr m_hModule;

	public IntPtr Handle => m_hModule;

	public LibraryModule(string fileName)
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

	[DllImport("Kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryW", SetLastError = true)]
	private static extern IntPtr LoadLibrary(string lpFileName);

	[DllImport("Kernel32.dll", SetLastError = true)]
	private static extern bool FreeLibrary(IntPtr hModule);

	[DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
}
