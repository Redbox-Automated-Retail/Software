using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;

namespace Redbox.Core;

public class ShellShortcut : IDisposable
{
	[Flags]
	private enum SLR_FLAGS
	{
		SLR_NO_UI = 1,
		SLR_ANY_MATCH = 2,
		SLR_UPDATE = 4,
		SLR_NOUPDATE = 8,
		SLR_NOSEARCH = 0x10,
		SLR_NOTRACK = 0x20,
		SLR_NOLINKINFO = 0x40,
		SLR_INVOKE_MSI = 0x80
	}

	[Flags]
	private enum SLGP_FLAGS
	{
		SLGP_SHORTPATH = 1,
		SLGP_UNCPRIORITY = 2,
		SLGP_RAWPATH = 4
	}

	private struct WIN32_FIND_DATAA
	{
		private const int MAX_PATH = 260;

		public int dwFileAttributes;

		public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;

		public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;

		public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;

		public int nFileSizeHigh;

		public int nFileSizeLow;

		public int dwReserved0;

		public int dwReserved1;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string cFileName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
		public string cAlternateFileName;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct WIN32_FIND_DATAW
	{
		private const int MAX_PATH = 260;

		public int dwFileAttributes;

		public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;

		public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;

		public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;

		public int nFileSizeHigh;

		public int nFileSizeLow;

		public int dwReserved0;

		public int dwReserved1;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string cFileName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
		public string cAlternateFileName;
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("0000010B-0000-0000-C000-000000000046")]
	private interface IPersistFile
	{
		void GetClassID(out Guid pClassID);

		[PreserveSig]
		int IsDirty();

		void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, int dwMode);

		void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);

		void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

		void GetCurFile(out IntPtr ppszFileName);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("000214EE-0000-0000-C000-000000000046")]
	private interface IShellLinkA
	{
		void GetPath([Out][MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile, int cchMaxPath, out WIN32_FIND_DATAA pfd, SLGP_FLAGS fFlags);

		void GetIDList(out IntPtr ppidl);

		void SetIDList(IntPtr pidl);

		void GetDescription([Out][MarshalAs(UnmanagedType.LPStr)] StringBuilder pszName, int cchMaxName);

		void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);

		void GetWorkingDirectory([Out][MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir, int cchMaxPath);

		void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);

		void GetArguments([Out][MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs, int cchMaxPath);

		void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);

		void GetHotkey(out short pwHotkey);

		void SetHotkey(short wHotkey);

		void GetShowCmd(out int piShowCmd);

		void SetShowCmd(int iShowCmd);

		void GetIconLocation([Out][MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

		void SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath, int iIcon);

		void SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel, int dwReserved);

		void Resolve(IntPtr hwnd, SLR_FLAGS fFlags);

		void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
	}

	[ComImport]
	[Guid("000214F9-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IShellLinkW
	{
		void GetPath([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out WIN32_FIND_DATAW pfd, SLGP_FLAGS fFlags);

		void GetIDList(out IntPtr ppidl);

		void SetIDList(IntPtr pidl);

		void GetDescription([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);

		void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

		void GetWorkingDirectory([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);

		void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

		void GetArguments([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);

		void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

		void GetHotkey(out short pwHotkey);

		void SetHotkey(short wHotkey);

		void GetShowCmd(out int piShowCmd);

		void SetShowCmd(int iShowCmd);

		void GetIconLocation([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

		void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

		void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);

		void Resolve(IntPtr hwnd, SLR_FLAGS fFlags);

		void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
	}

	[ComImport]
	[Guid("00021401-0000-0000-C000-000000000046")]
	private class ShellLink
	{
	}

	private const int INFOTIPSIZE = 1024;

	private const int MAX_PATH = 260;

	private const int SW_SHOWNORMAL = 1;

	private const int SW_SHOWMINIMIZED = 2;

	private const int SW_SHOWMAXIMIZED = 3;

	private const int SW_SHOWMINNOACTIVE = 7;

	private IShellLinkW m_link;

	private readonly string m_path;

	public string Arguments
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			m_link.GetArguments(stringBuilder, stringBuilder.Capacity);
			return stringBuilder.ToString();
		}
		set
		{
			m_link.SetArguments(value);
		}
	}

	public string Description
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			m_link.GetDescription(stringBuilder, stringBuilder.Capacity);
			return stringBuilder.ToString();
		}
		set
		{
			m_link.SetDescription(value);
		}
	}

	public string WorkingDirectory
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(260);
			m_link.GetWorkingDirectory(stringBuilder, stringBuilder.Capacity);
			return stringBuilder.ToString();
		}
		set
		{
			m_link.SetWorkingDirectory(value);
		}
	}

	public string Path
	{
		get
		{
			WIN32_FIND_DATAW pfd = default(WIN32_FIND_DATAW);
			StringBuilder stringBuilder = new StringBuilder(260);
			m_link.GetPath(stringBuilder, stringBuilder.Capacity, out pfd, SLGP_FLAGS.SLGP_UNCPRIORITY);
			return stringBuilder.ToString();
		}
		set
		{
			m_link.SetPath(value);
		}
	}

	public string IconPath
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(260);
			m_link.GetIconLocation(stringBuilder, stringBuilder.Capacity, out var _);
			return stringBuilder.ToString();
		}
		set
		{
			m_link.SetIconLocation(value, IconIndex);
		}
	}

	public int IconIndex
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(260);
			m_link.GetIconLocation(stringBuilder, stringBuilder.Capacity, out var piIcon);
			return piIcon;
		}
		set
		{
			m_link.SetIconLocation(IconPath, value);
		}
	}

	public Icon Icon
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(260);
			m_link.GetIconLocation(stringBuilder, stringBuilder.Capacity, out var piIcon);
			IntPtr hINSTANCE = Marshal.GetHINSTANCE(GetType().Module);
			IntPtr intPtr = ExtractIcon(hINSTANCE, stringBuilder.ToString(), piIcon);
			if (intPtr == IntPtr.Zero)
			{
				return null;
			}
			Icon icon = Icon.FromHandle(intPtr);
			Icon result = (Icon)icon.Clone();
			icon.Dispose();
			DestroyIcon(intPtr);
			return result;
		}
	}

	public ProcessWindowStyle WindowStyle
	{
		get
		{
			m_link.GetShowCmd(out var piShowCmd);
			switch (piShowCmd)
			{
			case 2:
			case 7:
				return ProcessWindowStyle.Minimized;
			case 3:
				return ProcessWindowStyle.Maximized;
			default:
				return ProcessWindowStyle.Normal;
			}
		}
		set
		{
			int showCmd = value switch
			{
				ProcessWindowStyle.Normal => 1, 
				ProcessWindowStyle.Minimized => 7, 
				ProcessWindowStyle.Maximized => 3, 
				_ => throw new ArgumentException("Unsupported ProcessWindowStyle value."), 
			};
			m_link.SetShowCmd(showCmd);
		}
	}

	public Keys Hotkey
	{
		get
		{
			m_link.GetHotkey(out var pwHotkey);
			return (Keys)(((pwHotkey & 0xFF00) << 8) | (pwHotkey & 0xFF));
		}
		set
		{
			if ((value & Keys.Modifiers) == 0)
			{
				throw new ArgumentException("Hotkey must include a modifier key.");
			}
			short hotkey = (short)(((int)(value & Keys.Modifiers) >> 8) | (int)(value & Keys.KeyCode));
			m_link.SetHotkey(hotkey);
		}
	}

	public object Instance => m_link;

	public ShellShortcut(string linkPath)
	{
		m_path = linkPath;
		m_link = (IShellLinkW)new ShellLink();
		if (File.Exists(linkPath))
		{
			IPersistFile persistFile = (IPersistFile)m_link;
			persistFile.Load(linkPath, 0);
		}
	}

	public void Save()
	{
		IPersistFile persistFile = (IPersistFile)m_link;
		persistFile.Save(m_path, fRemember: true);
	}

	public void Dispose()
	{
		if (m_link != null)
		{
			Marshal.ReleaseComObject(m_link);
			m_link = null;
		}
	}

	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

	[DllImport("user32.dll")]
	private static extern bool DestroyIcon(IntPtr hIcon);
}
