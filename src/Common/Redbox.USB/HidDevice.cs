using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Redbox.USB;

public class HidDevice
{
	internal struct OVERLAPPED
	{
		public int Internal;

		public int InternalHigh;

		public int Offset;

		public int OffsetHigh;

		public int hEvent;
	}

	internal struct COMMTIMEOUTS
	{
		public int ReadIntervalTimeout;

		public int ReadTotalTimeoutMultiplier;

		public int ReadTotalTimeoutConstant;

		public int WriteTotalTimeoutMultiplier;

		public int WriteTotalTimeoutConstant;
	}

	internal struct HIDP_CAPS
	{
		public ushort Usage;

		public ushort UsagePage;

		public ushort InputReportByteLength;

		public ushort OutputReportByteLength;

		public ushort FeatureReportByteLength;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
		public ushort[] Reserved;

		public ushort NumberLinkCollectionNodes;

		public ushort NumberInputButtonCaps;

		public ushort NumberInputValueCaps;

		public ushort NumberInputDataIndices;

		public ushort NumberOutputButtonCaps;

		public ushort NumberOutputValueCaps;

		public ushort NumberOutputDataIndices;

		public ushort NumberFeatureButtonCaps;

		public ushort NumberFeatureValueCaps;

		public ushort NumberFeatureDataIndices;
	}

	public int Index;

	public int Handle;

	public ushort Usage;

	public ushort UsagePage;

	public string DevicePath;

	public ushort NumberInputValueCaps;

	public ushort NumberInputButtonCaps;

	public ushort NumberOutputValueCaps;

	public ushort InputReportByteLength;

	public ushort OutputReportByteLength;

	public ushort NumberOutputButtonCaps;

	public ushort NumberFeatureValueCaps;

	public ushort NumberInputDataIndices;

	public ushort FeatureReportByteLength;

	public ushort NumberOutputDataIndices;

	public ushort NumberFeatureButtonCaps;

	public ushort NumberFeatureDataIndices;

	public ushort NumberLinkCollectionNodes;

	internal const uint GENERIC_READ = 2147483648u;

	internal const uint GENERIC_WRITE = 1073741824u;

	internal const uint FILE_SHARE_READ = 1u;

	internal const uint FILE_SHARE_WRITE = 2u;

	internal const int OPEN_EXISTING = 3;

	internal const int EV_RXFLAG = 2;

	internal const int INVALID_HANDLE_VALUE = -1;

	internal const int ERROR_INVALID_HANDLE = 6;

	internal const int FILE_FLAG_OVERLAPPED = 1073741824;

	private FileStream m_fileStream;

	public HidDevice(int index, string devicePath)
	{
		Index = index;
		DevicePath = devicePath;
	}

	public FileStream OpenStream()
	{
		if (Handle == 0 || Handle == -1)
		{
			_CreateFile();
			GetCapabilities();
		}
		if (m_fileStream == null)
		{
			m_fileStream = new FileStream(new SafeFileHandle(new IntPtr(Handle), ownsHandle: false), FileAccess.Read);
		}
		return m_fileStream;
	}

	public void CloseStream()
	{
		if (m_fileStream != null)
		{
			m_fileStream.Close();
		}
		if (Handle != 0)
		{
			_CloseHandle();
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern int CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile);

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern int CloseHandle(int hObject);

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern bool ReadFile(int hFile, byte[] lpBuffer, int nNumberOfBytesToRead, ref int lpNumberOfBytesRead, ref OVERLAPPED lpOverlapped);

	[DllImport("kernel32.dll")]
	internal static extern bool GetOverlappedResult(IntPtr hFile, [In] ref OVERLAPPED lpOverlapped, out uint lpNumberOfBytesTransferred, bool bWait);

	[DllImport("kernel32.dll")]
	internal static extern bool HasOverlappedIoCompleted([In] ref OVERLAPPED lpOverlapped);

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern bool GetCommTimeouts(int hFile, ref COMMTIMEOUTS lpCommTimeouts);

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern bool SetCommTimeouts(int hFile, ref COMMTIMEOUTS lpCommTimeouts);

	[DllImport("hid.dll", SetLastError = true)]
	internal static extern int HidP_GetCaps(int pPHIDP_PREPARSED_DATA, ref HIDP_CAPS myPHIDP_CAPS);

	[DllImport("hid.dll", SetLastError = true)]
	internal static extern int HidD_GetPreparsedData(int hObject, ref int pPHIDP_PREPARSED_DATA);

	[DllImport("hid.dll", SetLastError = true)]
	internal static extern int HidD_FreePreparsedData(int pPHIDP_PREPARSED_DATA);

	internal bool _CreateFile()
	{
		Handle = CreateFile(DevicePath, 3221225472u, 3u, 0u, 3u, 1073741824u, 0u);
		if (Handle == -1)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		return true;
	}

	internal void GetCapabilities()
	{
		int pPHIDP_PREPARSED_DATA = -1;
		if (_GetPreparsedData(Handle, ref pPHIDP_PREPARSED_DATA) != 0)
		{
			HIDP_CAPS hIDP_CAPS = _GetCaps(pPHIDP_PREPARSED_DATA);
			Usage = hIDP_CAPS.Usage;
			UsagePage = hIDP_CAPS.UsagePage;
			InputReportByteLength = hIDP_CAPS.InputReportByteLength;
			OutputReportByteLength = hIDP_CAPS.OutputReportByteLength;
			FeatureReportByteLength = hIDP_CAPS.FeatureReportByteLength;
			NumberLinkCollectionNodes = hIDP_CAPS.NumberLinkCollectionNodes;
			NumberInputButtonCaps = hIDP_CAPS.NumberInputButtonCaps;
			NumberInputValueCaps = hIDP_CAPS.NumberInputValueCaps;
			NumberInputDataIndices = hIDP_CAPS.NumberInputDataIndices;
			NumberOutputButtonCaps = hIDP_CAPS.NumberOutputButtonCaps;
			NumberOutputValueCaps = hIDP_CAPS.NumberOutputValueCaps;
			NumberOutputDataIndices = hIDP_CAPS.NumberOutputDataIndices;
			NumberFeatureButtonCaps = hIDP_CAPS.NumberFeatureButtonCaps;
			NumberFeatureValueCaps = hIDP_CAPS.NumberFeatureValueCaps;
			NumberFeatureDataIndices = hIDP_CAPS.NumberFeatureDataIndices;
			_FreePreparsedData(pPHIDP_PREPARSED_DATA);
		}
	}

	internal int _CloseHandle()
	{
		if (Handle == 0)
		{
			return 0;
		}
		Handle = 0;
		return CloseHandle(Handle);
	}

	internal byte[] _ReadFile(int inputReportByteLength, ref OVERLAPPED ovl)
	{
		int lpNumberOfBytesRead = 0;
		byte[] array = new byte[inputReportByteLength];
		if (!ReadFile(Handle, array, inputReportByteLength, ref lpNumberOfBytesRead, ref ovl))
		{
			return null;
		}
		byte[] array2 = new byte[lpNumberOfBytesRead];
		Array.Copy(array, array2, lpNumberOfBytesRead);
		return array2;
	}

	internal static int _GetPreparsedData(int hObject, ref int pPHIDP_PREPARSED_DATA)
	{
		return HidD_GetPreparsedData(hObject, ref pPHIDP_PREPARSED_DATA);
	}

	internal static int _FreePreparsedData(int pPHIDP_PREPARSED_DATA)
	{
		return HidD_FreePreparsedData(pPHIDP_PREPARSED_DATA);
	}

	internal static HIDP_CAPS _GetCaps(int pPreparsedData)
	{
		HIDP_CAPS myPHIDP_CAPS = default(HIDP_CAPS);
		if (HidP_GetCaps(pPreparsedData, ref myPHIDP_CAPS) == 0)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		return myPHIDP_CAPS;
	}
}
