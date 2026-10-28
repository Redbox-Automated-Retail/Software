using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Redbox.HAL.Core;

public class DeviceHelper : IDisposable
{
	public enum DriverType : uint
	{
		SPDIT_NODRIVER,
		SPDIT_CLASSDRIVER,
		SPDIT_COMPATDRIVER
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct DriverInfoData
	{
		public int Size;

		public DriverType DriverType;

		public IntPtr Reserved;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string Description;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string MfgName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string ProviderName;

		public long DriverDate;

		public ulong DriverVersion;

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(DriverType.ToString());
			stringBuilder.AppendLine(Description);
			stringBuilder.AppendLine(MfgName);
			stringBuilder.AppendLine(ProviderName);
			stringBuilder.AppendLine(DateTime.FromFileTime(DriverDate).ToShortDateString());
			stringBuilder.AppendLine(GetVersion().ToString());
			return stringBuilder.ToString();
		}

		public Version GetVersion()
		{
			int revision = (int)(DriverVersion & 0xFFFF);
			int build = (int)((DriverVersion >> 16) & 0xFFFF);
			int minor = (int)((DriverVersion >> 32) & 0xFFFF);
			int major = (int)(DriverVersion >> 48);
			return new Version(major, minor, build, revision);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct SP_DEVINFO_DATA
	{
		internal int cbSize;

		internal Guid classGuid;

		internal uint devInst;

		internal IntPtr reserved;
	}

	internal struct SP_DEVICE_INTERFACE_DATA
	{
		internal int cbSize;

		internal Guid interfaceClassGuid;

		internal int flags;

		private UIntPtr reserved;
	}

	internal struct SP_PROPCHANGE_PARAMS
	{
		internal SP_CLASSINSTALL_HEADER ClassInstallHeader;

		internal int StateChange;

		internal int Scope;

		internal int HwProfile;
	}

	internal struct SP_CLASSINSTALL_HEADER
	{
		internal int cbSize;

		internal int InstallFunction;
	}

	private enum ProcessDeviceInfoResult
	{
		None,
		Continue,
		Error,
		Success
	}

	private delegate ProcessDeviceInfoResult ProcessDeviceInfo(IntPtr devInfo, ref SP_DEVINFO_DATA did);

	private const int CR_SUCCESS = 0;

	private const int CM_PROB_DISABLED = 22;

	private const int DN_HAS_PROBLEM = 1024;

	private const int DN_STARTED = 8;

	private const int SPDIT_NODRIVER = 0;

	private const int SPDIT_CLASSDRIVER = 1;

	private const int SPDIT_COMPATDRIVER = 2;

	private const int ERROR_INSUFFICIENT_BUFFER = 122;

	private const int DIGCF_ALLCLASSES = 4;

	private const int DIGCF_PRESENT = 2;

	private const int INVALID_HANDLE_VALUE = -1;

	private const int MAX_DEV_LEN = 1000;

	private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;

	private const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;

	private const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;

	private const int DBT_DEVTYP_DEVICEINTERFACE = 5;

	private const int DBT_DEVNODES_CHANGED = 7;

	private const int WM_DEVICECHANGE = 537;

	private const int DIF_PROPERTYCHANGE = 18;

	private const int DICS_FLAG_GLOBAL = 1;

	private const int DICS_FLAG_CONFIGSPECIFIC = 2;

	private const int DICS_ENABLE = 1;

	private const int DICS_DISABLE = 2;

	private const int SPDRP_DEVICEDESC = 0;

	private const int SPDRP_HARDWAREID = 1;

	private const int SPDRP_COMPATIBLEIDS = 2;

	private const int SPDRP_UNUSED0 = 3;

	private const int SPDRP_SERVICE = 4;

	private const int SPDRP_UNUSED1 = 5;

	private const int SPDRP_UNUSED2 = 6;

	private const int SPDRP_CLASS = 7;

	private const int SPDRP_CLASSGUID = 8;

	private const int SPDRP_DRIVER = 9;

	private const int SPDRP_CONFIGFLAGS = 10;

	private const int SPDRP_MFG = 11;

	private const int SPDRP_FRIENDLYNAME = 12;

	private const int SPDRP_LOCATION_INFORMATION = 13;

	private const int SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 14;

	private const int SPDRP_CAPABILITIES = 15;

	private const int SPDRP_UI_NUMBER = 16;

	private const int SPDRP_UPPERFILTERS = 17;

	private const int SPDRP_LOWERFILTERS = 18;

	private const int SPDRP_BUSTYPEGUID = 19;

	private const int SPDRP_LEGACYBUSTYPE = 20;

	private const int SPDRP_BUSNUMBER = 21;

	private const int SPDRP_ENUMERATOR_NAME = 22;

	private const int SPDRP_SECURITY = 23;

	private const int SPDRP_SECURITY_SDS = 24;

	private const int SPDRP_DEVTYPE = 25;

	private const int SPDRP_EXCLUSIVE = 26;

	private const int SPDRP_CHARACTERISTICS = 27;

	private const int SPDRP_ADDRESS = 28;

	private const int SPDRP_UI_NUMBER_DESC_FORMAT = 29;

	private const int SPDRP_DEVICE_POWER_DATA = 30;

	private const int SPDRP_REMOVAL_POLICY = 31;

	private const int SPDRP_REMOVAL_POLICY_HW_DEFAULT = 32;

	private const int SPDRP_REMOVAL_POLICY_OVERRIDE = 33;

	private const int SPDRP_INSTALL_STATE = 34;

	private const int SPDRP_LOCATION_PATHS = 35;

	private const int SPDRP_BASE_CONTAINERID = 36;

	private int ResetPauseTime => 1500;

	private bool Debug { get; set; }

	public void Dispose()
	{
	}

	public bool ChangeByHWID(DeviceDescriptor descriptor, DeviceState state)
	{
		return SetDeviceStateInner(descriptor, state);
	}

	public bool ChangeByHWID(DeviceDescriptor descriptor, Guid deviceClass, DeviceState state)
	{
		StringBuilder hwidBuffer = MakeBuffer();
		return EnumerateDevices(deviceClass, 2u, delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			if (!SetupDiGetDeviceRegistryProperty(devInfo, ref did, 1u, 0u, hwidBuffer, 1000u, IntPtr.Zero))
			{
				return ProcessDeviceInfoResult.Continue;
			}
			if (!hwidBuffer.ToString().ToLower().Contains(descriptor.ToString()))
			{
				return ProcessDeviceInfoResult.Continue;
			}
			if (Debug)
			{
				LogHelper.Instance.Log("attempt to change state of {0} to {1}", hwidBuffer.ToString(), state.ToString());
			}
			return (!ChangeState(devInfo, ref did, state)) ? ProcessDeviceInfoResult.Error : ProcessDeviceInfoResult.Success;
		});
	}

	public bool ChangeWithDriverMatch(DeviceDescriptor descriptor, Guid deviceClass, DeviceState state)
	{
		StringBuilder hwidBuffer = MakeBuffer();
		return EnumerateDevices(deviceClass, 2u, delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			if (!SetupDiGetDeviceRegistryProperty(devInfo, ref did, 1u, 0u, hwidBuffer, 1000u, IntPtr.Zero))
			{
				return ProcessDeviceInfoResult.Continue;
			}
			if (!hwidBuffer.ToString().ToLower().Contains(descriptor.ToString()))
			{
				return ProcessDeviceInfoResult.Continue;
			}
			if (!MatchDriverInner(devInfo, ref did, descriptor.DriverInfo, hwidBuffer.ToString()))
			{
				return ProcessDeviceInfoResult.Continue;
			}
			return (!ChangeState(devInfo, ref did, state)) ? ProcessDeviceInfoResult.Error : ProcessDeviceInfoResult.Success;
		});
	}

	public bool ResetDriver(DeviceDescriptor descriptor)
	{
		if (!SetDeviceStateInner(descriptor, DeviceState.Disable))
		{
			return false;
		}
		if (!SetDeviceStateInner(descriptor, DeviceState.Enable))
		{
			return false;
		}
		return SetDeviceStateInner(descriptor, DeviceState.Enable);
	}

	public bool ResetByName(string deviceName)
	{
		int resetPauseTime = ResetPauseTime;
		ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
		if (!SetDeviceStateInner(DeviceClass.Ports, DeviceState.Disable, deviceName, MatchType.DeviceDescription))
		{
			return false;
		}
		manualResetEvent.WaitOne(resetPauseTime);
		if (!SetDeviceStateInner(DeviceClass.USB, DeviceState.Disable, deviceName, MatchType.DeviceDescription))
		{
			return false;
		}
		manualResetEvent.WaitOne(resetPauseTime);
		if (!SetDeviceStateInner(DeviceClass.USB, DeviceState.Enable, deviceName, MatchType.DeviceDescription))
		{
			return false;
		}
		manualResetEvent.WaitOne(resetPauseTime);
		if (!SetDeviceStateInner(DeviceClass.Ports, DeviceState.Enable, deviceName, MatchType.DeviceDescription))
		{
			return false;
		}
		return true;
	}

	public DeviceStatus FindDeviceStatus(Guid deviceClazz, DeviceDescriptor deviceInfo)
	{
		StringBuilder hwid = MakeBuffer();
		DeviceStatus rv = DeviceStatus.None;
		EnumerateDevices(deviceClazz, 2u, delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			if (!SetupDiGetDeviceRegistryProperty(devInfo, ref did, 1u, 0u, hwid, 1000u, IntPtr.Zero))
			{
				return ProcessDeviceInfoResult.Continue;
			}
			if (!hwid.ToString().ToLower().Contains(deviceInfo.ToString()))
			{
				return ProcessDeviceInfoResult.Continue;
			}
			rv = DeviceStatus.Unknown;
			if (CM_Get_DevNode_Status(out var status, out var probNum, did.devInst, 0) == 0)
			{
				if ((status & 0x400) != 0 && 22 == probNum)
				{
					rv = DeviceStatus.Disabled;
				}
				else if ((status & 8) != 0)
				{
					rv = DeviceStatus.Enabled;
				}
				return ProcessDeviceInfoResult.Success;
			}
			return ProcessDeviceInfoResult.Continue;
		});
		return rv;
	}

	public bool MatchDriver(DeviceDescriptor descriptor, DriverDescriptor driverInfo, Guid type)
	{
		StringBuilder hwid = MakeBuffer();
		return EnumerateDevices(type, 2u, delegate(IntPtr hDevInfo, ref SP_DEVINFO_DATA did)
		{
			if (!SetupDiGetDeviceRegistryProperty(hDevInfo, ref did, 1u, 0u, hwid, 1000u, IntPtr.Zero))
			{
				return ProcessDeviceInfoResult.Continue;
			}
			if (!hwid.ToString().ToLower().Contains(descriptor.ToString()))
			{
				return ProcessDeviceInfoResult.Continue;
			}
			return (!MatchDriverInner(hDevInfo, ref did, driverInfo, hwid.ToString())) ? ProcessDeviceInfoResult.Continue : ProcessDeviceInfoResult.Success;
		});
	}

	public void FindDeviceWithPid(DeviceClass clazz, string vid, string pid, MatchType matcher, List<string> errors, List<string> devMatches)
	{
		int lastError = 0;
		char[] separator = new char[1] { '&' };
		StringBuilder clazzNameBuffer = MakeBuffer();
		EnumerateDevices(Guid.Empty, 6u, delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			if (!SetupDiGetDeviceRegistryProperty(devInfo, ref did, 7u, 0u, clazzNameBuffer, 1000u, IntPtr.Zero))
			{
				lastError = Marshal.GetLastWin32Error();
				errors.Add($"SetupDiGetDeviceRegisProperty returned {lastError} searching for class.");
				return ProcessDeviceInfoResult.Error;
			}
			if (clazz.ToString() != clazzNameBuffer.ToString())
			{
				return ProcessDeviceInfoResult.Continue;
			}
			string text = QueryHardwareId(devInfo, ref did, out lastError);
			if (!text.ToLower().StartsWith("vid"))
			{
				int num = text.IndexOf("vid");
				if (num == -1)
				{
					errors.Add("Invalid string.");
					return ProcessDeviceInfoResult.Error;
				}
				text = text.Substring(num).ToLower();
			}
			string[] array = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length < 2)
			{
				LogHelper.Instance.Log("The HWID '{0}' is malformed.", text);
				return ProcessDeviceInfoResult.Continue;
			}
			string text2 = array[0].Substring(4);
			string s = array[1].Substring(4);
			if (text2 == vid)
			{
				int num2 = int.Parse(s);
				int num3 = int.Parse(pid);
				switch (matcher)
				{
				case MatchType.GTEPid:
					if (num2 >= num3)
					{
						devMatches.Add(text);
					}
					break;
				case MatchType.ExactPID:
					if (num2 == num3)
					{
						devMatches.Add(text);
					}
					break;
				}
			}
			return ProcessDeviceInfoResult.Continue;
		});
	}

	public void EnumDevices(DeviceClass clazz, TextWriter writer)
	{
		int lastError = 0;
		Dictionary<string, List<string>> deviceMap = new Dictionary<string, List<string>>();
		string deviceClass = clazz.ToString();
		StringBuilder clazzNameBuffer = MakeBuffer();
		EnumerateDevices(Guid.Empty, 6u, delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			if (!SetupDiGetDeviceRegistryProperty(devInfo, ref did, 7u, 0u, clazzNameBuffer, 1000u, IntPtr.Zero))
			{
				lastError = Marshal.GetLastWin32Error();
				return ProcessDeviceInfoResult.Error;
			}
			if (clazz == DeviceClass.None)
			{
				List<string> list2 = null;
				list2 = (deviceMap.ContainsKey(clazzNameBuffer.ToString()) ? deviceMap[clazzNameBuffer.ToString()] : (deviceMap[clazzNameBuffer.ToString()] = new List<string>()));
				list2.Add(QueryHardwareId(devInfo, ref did, out lastError));
			}
			else if (deviceClass == clazzNameBuffer.ToString())
			{
				string arg = QueryHardwareId(devInfo, ref did, out lastError);
				writer.WriteLine("Class {0} hwid {1}", clazzNameBuffer.ToString(), arg);
			}
			return ProcessDeviceInfoResult.Continue;
		});
		if (clazz != DeviceClass.None)
		{
			return;
		}
		foreach (string key in deviceMap.Keys)
		{
			List<string> list = deviceMap[key];
			writer.WriteLine("Devices for class {0}", key);
			foreach (string item in list)
			{
				writer.WriteLine("  HWID: {0}", item);
			}
		}
	}

	public DeviceHelper()
	{
		Debug = false;
	}

	public DeviceHelper(bool debug)
	{
		Debug = debug;
	}

	[DllImport("setupapi.dll", SetLastError = true)]
	internal static extern IntPtr SetupDiGetClassDevs(ref Guid gClass, uint iEnumerator, IntPtr hParent, uint nFlags);

	[DllImport("setupapi.dll", SetLastError = true)]
	private static extern int CM_Get_DevNode_Status(out uint status, out uint probNum, uint devInst, int flags);

	[DllImport("setupapi.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetupDiBuildDriverInfoList(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, DriverType driverType);

	[DllImport("setupapi.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetupDiEnumDriverInfo(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, DriverType driverType, uint memberIndex, ref DriverInfoData driverInfoData);

	[DllImport("setupapi.dll", SetLastError = true)]
	private static extern int SetupDiDestroyDeviceInfoList(IntPtr lpInfoSet);

	[DllImport("setupapi.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, uint memberIndex, ref SP_DEVINFO_DATA data);

	[DllImport("setupapi.dll", SetLastError = true)]
	private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr lpInfoSet, ref SP_DEVINFO_DATA infoData, uint Property, uint PropertyRegDataType, StringBuilder PropertyBuffer, uint PropertyBufferSize, IntPtr RequiredSize);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool SetupDiSetClassInstallParams(IntPtr dis, ref SP_DEVINFO_DATA did, ref SP_PROPCHANGE_PARAMS spp, int sz);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool SetupDiCallClassInstaller(uint func, IntPtr dis, ref SP_DEVINFO_DATA did);

	private string QueryDeviceDescription(IntPtr devInfo, ref SP_DEVINFO_DATA deviceInfoData, out int lastError)
	{
		lastError = 0;
		StringBuilder stringBuilder = MakeBuffer();
		if (!SetupDiGetDeviceRegistryProperty(devInfo, ref deviceInfoData, 0u, 0u, stringBuilder, 1000u, IntPtr.Zero))
		{
			lastError = Marshal.GetLastWin32Error();
			return null;
		}
		return stringBuilder.ToString().ToLower();
	}

	private string QueryHardwareId(IntPtr devInfo, ref SP_DEVINFO_DATA did, out int lastError)
	{
		lastError = 0;
		StringBuilder stringBuilder = MakeBuffer();
		if (!SetupDiGetDeviceRegistryProperty(devInfo, ref did, 1u, 0u, stringBuilder, 1000u, IntPtr.Zero))
		{
			lastError = Marshal.GetLastWin32Error();
			return null;
		}
		return stringBuilder.ToString().ToLower();
	}

	private bool SetDeviceStateInner(DeviceClass devClass, DeviceState devState, string queryData, MatchType match)
	{
		if (devState == DeviceState.None)
		{
			return false;
		}
		StringBuilder buffer = MakeBuffer();
		int lastError;
		return EnumerateDevices(Guid.Empty, 6u, delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			if (!SetupDiGetDeviceRegistryProperty(devInfo, ref did, 7u, 0u, buffer, 1000u, IntPtr.Zero))
			{
				lastError = Marshal.GetLastWin32Error();
				return ProcessDeviceInfoResult.Continue;
			}
			if (devClass.ToString() != buffer.ToString())
			{
				return ProcessDeviceInfoResult.Continue;
			}
			string text = ((match == MatchType.HardwareId) ? QueryHardwareId(devInfo, ref did, out lastError) : QueryDeviceDescription(devInfo, ref did, out lastError));
			if (text.Equals(queryData, StringComparison.CurrentCultureIgnoreCase))
			{
				bool flag = ChangeState(devInfo, ref did, devState);
				LogHelper.Instance.Log($"Change state on class {devClass.ToString()} ({devState.ToString()}) returned {flag}");
				if (!flag)
				{
					return ProcessDeviceInfoResult.Error;
				}
				return ProcessDeviceInfoResult.Success;
			}
			return ProcessDeviceInfoResult.Continue;
		});
	}

	private bool SetDeviceStateInner(DeviceDescriptor descriptor, DeviceState state)
	{
		StringBuilder clazzNameBuffer = MakeBuffer();
		int lastError = 0;
		int failures = 0;
		EnumerateDevices(Guid.Empty, 6u, delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			if (!SetupDiGetDeviceRegistryProperty(devInfo, ref did, 7u, 0u, clazzNameBuffer, 1000u, IntPtr.Zero))
			{
				lastError = Marshal.GetLastWin32Error();
				return ProcessDeviceInfoResult.Continue;
			}
			string text = QueryHardwareId(devInfo, ref did, out lastError);
			if (text == null || !text.Contains(descriptor.ToString()))
			{
				return ProcessDeviceInfoResult.Continue;
			}
			LogHelper.Instance.Log("Found HWID {0} on class {1}", text, clazzNameBuffer);
			bool flag = ChangeState(devInfo, ref did, state);
			LogHelper.Instance.Log("Change state {0} returned {1}", state.ToString(), flag ? "OK" : "FAIL");
			if (!flag)
			{
				failures++;
			}
			return ProcessDeviceInfoResult.Continue;
		});
		return 0 == failures;
	}

	private bool ChangeState(IntPtr hDevInfo, ref SP_DEVINFO_DATA devInfoData, DeviceState devState)
	{
		try
		{
			SP_PROPCHANGE_PARAMS spp = new SP_PROPCHANGE_PARAMS
			{
				ClassInstallHeader = default(SP_CLASSINSTALL_HEADER)
			};
			if (DeviceState.Enable == devState)
			{
				spp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(SP_CLASSINSTALL_HEADER));
				spp.ClassInstallHeader.InstallFunction = 18;
				spp.StateChange = 1;
				spp.Scope = 1;
				spp.HwProfile = 0;
				if (SetupDiSetClassInstallParams(hDevInfo, ref devInfoData, ref spp, Marshal.SizeOf(typeof(SP_PROPCHANGE_PARAMS))))
				{
					SetupDiCallClassInstaller(18u, hDevInfo, ref devInfoData);
				}
				spp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(SP_CLASSINSTALL_HEADER));
				spp.ClassInstallHeader.InstallFunction = 18;
				spp.StateChange = 1;
				spp.Scope = 2;
				spp.HwProfile = 0;
			}
			else
			{
				spp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(SP_CLASSINSTALL_HEADER));
				spp.ClassInstallHeader.InstallFunction = 18;
				spp.StateChange = 2;
				spp.Scope = 2;
				spp.HwProfile = 0;
			}
			if (!SetupDiSetClassInstallParams(hDevInfo, ref devInfoData, ref spp, Marshal.SizeOf(typeof(SP_PROPCHANGE_PARAMS))))
			{
				LogHelper.Instance.Log("SetupDiSetClassInstallParams returned false.");
				return false;
			}
			if (!SetupDiCallClassInstaller(18u, hDevInfo, ref devInfoData))
			{
				LogHelper.Instance.Log("SetupDiCallClassInstaller returned false.");
				return false;
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private bool MatchDriverInner(IntPtr hDevInfo, ref SP_DEVINFO_DATA did, DriverDescriptor driverInfo, string hwid)
	{
		if (driverInfo == null)
		{
			return false;
		}
		if (SetupDiBuildDriverInfoList(hDevInfo, ref did, DriverType.SPDIT_COMPATDRIVER))
		{
			DriverInfoData driverInfoData = default(DriverInfoData);
			driverInfoData.Size = Marshal.SizeOf(driverInfoData);
			for (uint num = 0u; SetupDiEnumDriverInfo(hDevInfo, ref did, DriverType.SPDIT_COMPATDRIVER, num, ref driverInfoData); num++)
			{
				if (Debug)
				{
					LogHelper.Instance.Log("Driver data:", LogEntryType.Info);
					LogHelper.Instance.Log(driverInfoData.ToString(), LogEntryType.Info);
				}
				if (driverInfoData.GetVersion().Major != driverInfo.DriverVersion.Major)
				{
					if (Debug)
					{
						LogHelper.Instance.Log("** driver version {0} didn't match {1}", driverInfoData.GetVersion().ToString(), driverInfo.DriverVersion.ToString());
					}
					continue;
				}
				if (driverInfoData.ProviderName.Equals(driverInfo.Provider, StringComparison.CurrentCultureIgnoreCase))
				{
					if (Debug)
					{
						LogHelper.Instance.Log("Found HWID {0}; driver info:", hwid);
						LogHelper.Instance.Log("{0}", driverInfoData.ToString());
					}
					return true;
				}
				if (Debug)
				{
					LogHelper.Instance.Log("** driver mfr {0} didn't match {1}", driverInfoData.ProviderName, driverInfo.Provider);
				}
			}
		}
		return false;
	}

	private bool EnumerateDevices(Guid deviceClass, uint searchFlags, ProcessDeviceInfo callback)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = SetupDiGetClassDevs(ref deviceClass, 0u, IntPtr.Zero, searchFlags);
			if (intPtr.ToInt32() == -1)
			{
				return false;
			}
			SP_DEVINFO_DATA data = default(SP_DEVINFO_DATA);
			data.cbSize = Marshal.SizeOf(data);
			for (uint num = 0u; SetupDiEnumDeviceInfo(intPtr, num, ref data); num++)
			{
				ProcessDeviceInfoResult processDeviceInfoResult = callback(intPtr, ref data);
				if (ProcessDeviceInfoResult.Error == processDeviceInfoResult)
				{
					return false;
				}
				if (ProcessDeviceInfoResult.Success == processDeviceInfoResult)
				{
					return true;
				}
			}
			return false;
		}
		catch (Exception)
		{
			LogHelper.Instance.Log("Unable to enumerate device tree.");
			return false;
		}
		finally
		{
			try
			{
				if (intPtr != IntPtr.Zero && intPtr.ToInt32() != -1)
				{
					SetupDiDestroyDeviceInfoList(intPtr);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Clearing the device info list caught an exception.", e);
			}
		}
	}

	private StringBuilder MakeBuffer()
	{
		StringBuilder stringBuilder = new StringBuilder("");
		stringBuilder.Capacity = 1000;
		return stringBuilder;
	}
}
