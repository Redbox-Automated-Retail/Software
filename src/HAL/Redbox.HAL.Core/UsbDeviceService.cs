using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core.Descriptors;

namespace Redbox.HAL.Core;

public sealed class UsbDeviceService : IUsbDeviceService
{
	private enum DriverType : uint
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
			return new Version((int)(DriverVersion >> 48), minor, build, revision);
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

	private enum DeviceDescriptorMatchOption
	{
		VidPid,
		Vendor,
		Product
	}

	private enum ProcessDeviceInfoResult
	{
		None,
		Continue,
		Error,
		Success
	}

	private delegate ProcessDeviceInfoResult ProcessDeviceInfo(IntPtr devInfo, ref SP_DEVINFO_DATA did);

	private readonly DriverDescriptor Driver_Elo = new DriverDescriptor(new Version("5.5.1.4"), "Elo Touch Solutions");

	private readonly DriverDescriptor Driver_GeneralTouch_HID1 = new DriverDescriptor(new Version("4.2.2.1"), "General Touch Technology Co.,Ltd.");

	private readonly DriverDescriptor Driver_GeneralTouch_HID2 = new DriverDescriptor(new Version("2.10.1781.0"), "General Touch Technology Co., Ltd.");

	private readonly DriverDescriptor Driver_TouchBase = new DriverDescriptor(new Version("4.0.2.0"), "Touch-Base Ltd");

	private readonly List<ITouchscreenDescriptor> TouchScreenDescriptors = new List<ITouchscreenDescriptor>();

	private readonly List<IDeviceDescriptor> CreditCardReaders = new List<IDeviceDescriptor>();

	private readonly IDriverDescriptor Gen4DriverDescriptor = new DriverDescriptor(new Version("2.1.0.0"), "AVEO");

	private readonly IDriverDescriptor Gen3DriverDescriptor = new DriverDescriptor(new Version("5.7.19104.104"), "Sonix");

	private readonly List<IDeviceDescriptor> Cameras = new List<IDeviceDescriptor>();

	private readonly bool Debug;

	private const int CR_SUCCESS = 0;

	private const int CM_PROB_FAILED_START = 10;

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

	public IDeviceDescriptor FindActiveCamera(bool matchDriver)
	{
		IDeviceDescriptor deviceDescriptor = Cameras.Find((IDeviceDescriptor each) => each.Locate());
		if (deviceDescriptor == null)
		{
			return null;
		}
		LogHelper.Instance.Log("[UsbDeviceService] FindActiveCamera: HWID found {0}", deviceDescriptor.ToString());
		if (!matchDriver)
		{
			return deviceDescriptor;
		}
		if (!deviceDescriptor.MatchDriver())
		{
			LogHelper.Instance.Log("[{0}] unable to match camera driver.", GetType().Name);
			return null;
		}
		return deviceDescriptor;
	}

	public IDeviceDescriptor FromQueryString(string hwid)
	{
		return GenericDeviceDescriptor.Create(hwid, DeviceClass.None);
	}

	public bool SetDeviceState(IDeviceDescriptor descriptor, DeviceState state)
	{
		MakeBuffer();
		int lastError = 0;
		int failures = 0;
		EnumerateDevices(descriptor.SetupClass.Guid, SearchFlags(descriptor), delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			string text = QueryHardwareId(devInfo, ref did, out lastError);
			if (text == null)
			{
				return ProcessDeviceInfoResult.Continue;
			}
			IDeviceDescriptor deviceDescriptor = FromQueryString(text);
			if (deviceDescriptor != null && Match(deviceDescriptor, descriptor))
			{
				bool flag = ChangeState(devInfo, ref did, state);
				LogHelper.Instance.Log("[{0}] Found HWID {1}; change state to {2} returned {3}", GetType().Name, text, state.ToString(), flag ? "OK" : "FAIL");
				if (!flag)
				{
					int num = failures + 1;
					failures = num;
				}
			}
			return ProcessDeviceInfoResult.Continue;
		});
		return failures == 0;
	}

	public bool ChangeByHWID(IDeviceDescriptor descriptor, DeviceState state)
	{
		int error = 0;
		return EnumerateDevices(descriptor.SetupClass.Guid, SearchFlags(descriptor), delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			string text = QueryHardwareId(devInfo, ref did, out error);
			if (text == null)
			{
				return ProcessDeviceInfoResult.Continue;
			}
			IDeviceDescriptor deviceDescriptor = FromQueryString(text);
			ProcessDeviceInfoResult result = ProcessDeviceInfoResult.Continue;
			if (deviceDescriptor != null && Match(deviceDescriptor, descriptor))
			{
				if (Debug)
				{
					LogHelper.Instance.Log("[{0}] Change state of {1} to {2}", GetType().Name, text, state.ToString());
				}
				result = (ChangeState(devInfo, ref did, state) ? ProcessDeviceInfoResult.Success : ProcessDeviceInfoResult.Error);
			}
			return result;
		});
	}

	public DeviceStatus FindDeviceStatus(IDeviceDescriptor deviceInfo)
	{
		DeviceStatus rv = DeviceStatus.None;
		int lastError;
		EnumerateDevices(deviceInfo.SetupClass.Guid, SearchFlags(deviceInfo), delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			string text = QueryHardwareId(devInfo, ref did, out lastError);
			if (text == null)
			{
				return ProcessDeviceInfoResult.Continue;
			}
			IDeviceDescriptor deviceDescriptor = FromQueryString(text);
			LogHelper.Instance.Log("[USBService] Find device status hwid = {0}", text);
			rv = DeviceStatus.None;
			if (deviceDescriptor != null && Match(deviceDescriptor, deviceInfo))
			{
				rv |= DeviceStatus.Found;
				if (CM_Get_DevNode_Status(out var status, out var probNum, did.devInst, 0) == 0)
				{
					LogHelper.Instance.Log("[USBService] CM_Get_DevNode {0} ( {1} ) status = {2} problem = {3}", deviceInfo.ToString(), deviceInfo.Friendlyname, status, probNum);
					if ((status & 0x400) != 0)
					{
						if (22 == probNum)
						{
							rv |= DeviceStatus.Disabled;
						}
						else if (10 == probNum)
						{
							rv |= DeviceStatus.NotStarted;
						}
					}
					else if ((status & 8) != 0)
					{
						rv |= DeviceStatus.Enabled;
					}
					return ProcessDeviceInfoResult.Success;
				}
			}
			return ProcessDeviceInfoResult.Continue;
		});
		return rv;
	}

	public bool MatchDriverByVendor(IDeviceDescriptor desc, IDriverDescriptor driverInfo)
	{
		int lastError;
		return EnumerateDevices(desc.SetupClass.Guid, SearchFlags(desc), delegate(IntPtr hDevInfo, ref SP_DEVINFO_DATA did)
		{
			string text = QueryHardwareId(hDevInfo, ref did, out lastError);
			if (text == null)
			{
				return ProcessDeviceInfoResult.Continue;
			}
			ProcessDeviceInfoResult result = ProcessDeviceInfoResult.Continue;
			if (!string.IsNullOrEmpty(text) && text.Equals(desc.Vendor, StringComparison.CurrentCultureIgnoreCase))
			{
				result = ((!MatchDriverInner(hDevInfo, ref did, driverInfo, text.ToString())) ? ProcessDeviceInfoResult.Continue : ProcessDeviceInfoResult.Success);
			}
			return result;
		});
	}

	public bool MatchDriver(IDeviceDescriptor descriptor, IDriverDescriptor driverInfo)
	{
		int lastError;
		return EnumerateDevices(descriptor.SetupClass.Guid, 2u, delegate(IntPtr hDevInfo, ref SP_DEVINFO_DATA did)
		{
			string text = QueryHardwareId(hDevInfo, ref did, out lastError);
			if (text == null)
			{
				return ProcessDeviceInfoResult.Continue;
			}
			LogHelper.Instance.Log("[MatchDriver] Processing HWID {0}", text.ToString());
			IDeviceDescriptor deviceDescriptor = FromQueryString(text.ToString());
			if (deviceDescriptor == null || !Match(deviceDescriptor, descriptor))
			{
				return ProcessDeviceInfoResult.Continue;
			}
			LogHelper.Instance.Log("[MatchDriver] HWID found {0}", text.ToString());
			return (!MatchDriverInner(hDevInfo, ref did, driverInfo, text.ToString())) ? ProcessDeviceInfoResult.Continue : ProcessDeviceInfoResult.Success;
		});
	}

	public IUsbDeviceSearchResult FindDevice(IDeviceDescriptor descriptor)
	{
		int lastError = 0;
		UsbDeviceSearchResult result = new UsbDeviceSearchResult();
		if (Debug)
		{
			LogHelper.Instance.Log("[FindDevice] Search device class {0}", descriptor.SetupClass.Class.ToString());
		}
		EnumerateDevices(descriptor.SetupClass.Guid, SearchFlags(descriptor), delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			string text = QueryHardwareId(devInfo, ref did, out lastError);
			if (text != null)
			{
				LogHelper.Instance.Log("  [FindDeviceWithVid] found hwid = {0}", text);
				IDeviceDescriptor deviceDescriptor = FromQueryString(text);
				if (deviceDescriptor != null && Match(deviceDescriptor, descriptor))
				{
					result.Matches.Add(deviceDescriptor);
				}
			}
			return ProcessDeviceInfoResult.Continue;
		});
		return result;
	}

	public IUsbDeviceSearchResult FindVendorDevices(string _vendor)
	{
		int lastError = 0;
		UsbDeviceSearchResult result = new UsbDeviceSearchResult();
		if (Debug)
		{
			LogHelper.Instance.Log("[FindVendorDevices] Search for devices from vendor {0}", _vendor);
		}
		EnumerateDevices(Guid.Empty, 6u, delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			string text = QueryHardwareId(devInfo, ref did, out lastError);
			if (Debug && text != null)
			{
				LogHelper.Instance.Log("  [FindVendorDevices] Examine hwid = {0} for match", text);
			}
			IDeviceDescriptor deviceDescriptor = FromQueryString(text);
			if (deviceDescriptor != null && _vendor.Equals(deviceDescriptor.Vendor, StringComparison.CurrentCultureIgnoreCase))
			{
				result.Matches.Add(deviceDescriptor);
			}
			return ProcessDeviceInfoResult.Continue;
		});
		return result;
	}

	public void EnumDevices(Action<string, string> onDeviceFound)
	{
		int lastError = 0;
		StringBuilder clazzNameBuffer = MakeBuffer();
		EnumerateDevices(Guid.Empty, 6u, delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			if (!SetupDiGetDeviceRegistryProperty(devInfo, ref did, 7u, 0u, clazzNameBuffer, 1000u, IntPtr.Zero))
			{
				lastError = Marshal.GetLastWin32Error();
				LogHelper.Instance.Log("[{0}] SetupDiGetDeviceRegistryProperty returned error {1}", GetType().Name, lastError);
				return ProcessDeviceInfoResult.Error;
			}
			string text = QueryHardwareId(devInfo, ref did, out lastError);
			if (!string.IsNullOrEmpty(text))
			{
				onDeviceFound(clazzNameBuffer.ToString(), text);
			}
			return ProcessDeviceInfoResult.Continue;
		});
	}

	public List<IDeviceDescriptor> FindDevices(DeviceClass clazz)
	{
		int lastError = 0;
		List<IDeviceDescriptor> result = new List<IDeviceDescriptor>();
		if (clazz == DeviceClass.None)
		{
			return result;
		}
		StringBuilder clazzNameBuffer = MakeBuffer();
		Guid deviceClass = Guid.Empty;
		IDeviceSetupClass deviceSetupClass = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>().Get(clazz);
		if (deviceSetupClass != null)
		{
			deviceClass = deviceSetupClass.Guid;
		}
		EnumerateDevices(deviceClass, 2u, delegate(IntPtr devInfo, ref SP_DEVINFO_DATA did)
		{
			if (!SetupDiGetDeviceRegistryProperty(devInfo, ref did, 7u, 0u, clazzNameBuffer, 1000u, IntPtr.Zero))
			{
				lastError = Marshal.GetLastWin32Error();
				return ProcessDeviceInfoResult.Error;
			}
			DeviceClass deviceClass2 = Enum<DeviceClass>.ParseIgnoringCase(clazzNameBuffer.ToString(), DeviceClass.None);
			if (Debug)
			{
				LogHelper.Instance.Log("[{0}] Requested search = {1}, found clazz = {2}", GetType().Name, clazz.ToString(), clazzNameBuffer.ToString());
			}
			if (deviceClass2 == clazz)
			{
				string text = QueryHardwareId(devInfo, ref did, out lastError);
				if (!string.IsNullOrEmpty(text))
				{
					if (Debug)
					{
						LogHelper.Instance.Log("[{0}] Requested search = {1}, found clazz = {2} HWID = {3}", GetType().Name, clazz.ToString(), clazzNameBuffer.ToString(), text);
					}
					GenericDeviceDescriptor genericDeviceDescriptor = GenericDeviceDescriptor.Create(text, deviceClass2);
					if (genericDeviceDescriptor != null)
					{
						result.Add(genericDeviceDescriptor);
					}
				}
			}
			return ProcessDeviceInfoResult.Continue;
		});
		return result;
	}

	public ITouchscreenDescriptor FindTouchScreen(bool matchDriver)
	{
		foreach (ITouchscreenDescriptor touchScreenDescriptor in TouchScreenDescriptors)
		{
			LogHelper.Instance.Log("[UsbDeviceService] Searching for device {0}", touchScreenDescriptor.ToString());
			if (touchScreenDescriptor.Locate())
			{
				LogHelper.Instance.Log("[UsbDeviceService] Found Touchscreen device {0}.", touchScreenDescriptor.ToString());
				if (!matchDriver)
				{
					return touchScreenDescriptor;
				}
				if (touchScreenDescriptor.MatchDriver())
				{
					LogHelper.Instance.Log("[UsbDeviceService] Touch screen driver matched.");
					return touchScreenDescriptor;
				}
			}
		}
		LogHelper.Instance.Log("[UsbDeviceService] Could not match TS driver.");
		return null;
	}

	public ITouchscreenDescriptor FindTouchScreen()
	{
		return FindTouchScreen(matchDriver: false);
	}

	public IQueryUsbDeviceResult FindCCR()
	{
		return OnLocate(CreditCardReaders);
	}

	public IQueryUsbDeviceResult FindCamera()
	{
		return OnLocate(Cameras);
	}

	public UsbDeviceService(bool debug)
	{
		Debug = debug;
		DriverDescriptor desc = new DriverDescriptor(new Version("7.13.14.0"), "3M");
		TouchScreenDescriptors.Add(new MicrotouchDescriptor(this, DeviceClass.HIDClass, desc));
		TouchScreenDescriptors.Add(new MicrotouchDescriptor(this, DeviceClass.Mouse, desc));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("04e7", "0020", "Elo", Driver_Elo, this, DeviceClass.Mouse));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("04e7", "0042", "Elo_2", Driver_Elo, this, DeviceClass.Mouse));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("0dfc", "0001", "General Touch", Driver_GeneralTouch_HID2, this, DeviceClass.HIDClass));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("0dfc", "0001", "General Touch", Driver_GeneralTouch_HID2, this, DeviceClass.HIDClass));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("0dfc", "0001", "General Touch", Driver_GeneralTouch_HID1, this, DeviceClass.Mouse));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("14c8", "0003", "Zytronic", Driver_TouchBase, this, DeviceClass.Mouse));
		IDeviceSetupClassFactory service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
		CreditCardReaders.Add(new IdTechRev1(this, service));
		CreditCardReaders.Add(new IdTechRev2(this, service));
		Cameras.Add(new Gen5DeviceDescriptor(this));
		Cameras.Add(new LegacyDeviceDescriptor("1871", "0d01", "4th Gen (Color)", Gen4DriverDescriptor, this));
		Cameras.Add(new LegacyDeviceDescriptor("1871", "0f01", "4th Gen", Gen4DriverDescriptor, this));
		Cameras.Add(new LegacyDeviceDescriptor("0c45", "627b", "3rd Gen", Gen3DriverDescriptor, this));
	}

	public UsbDeviceService()
		: this(debug: false)
	{
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

	private uint SearchFlags(IDeviceDescriptor desc)
	{
		uint num = 2u;
		if (desc.SetupClass.Class == DeviceClass.None)
		{
			num |= 4;
		}
		return num;
	}

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
				LogHelper.Instance.Log("SetupDiCallClassInstaller returned false ( Win32 error = {0} )", Marshal.GetLastWin32Error());
				return false;
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private QueryUsbDeviceResult OnLocate(List<IDeviceDescriptor> descriptors)
	{
		foreach (IDeviceDescriptor descriptor in descriptors)
		{
			DeviceStatus deviceStatus = FindDeviceStatus(descriptor);
			if ((deviceStatus & DeviceStatus.Found) != DeviceStatus.None)
			{
				return new QueryUsbDeviceResult(descriptor)
				{
					Status = deviceStatus
				};
			}
		}
		return new QueryUsbDeviceResult(null)
		{
			Status = DeviceStatus.None
		};
	}

	private bool MatchDriverInner(IntPtr hDevInfo, ref SP_DEVINFO_DATA did, IDriverDescriptor driverInfo, string hwid)
	{
		if (driverInfo != null && SetupDiBuildDriverInfoList(hDevInfo, ref did, DriverType.SPDIT_COMPATDRIVER))
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
				if (driverInfoData.GetVersion() != driverInfo.DriverVersion)
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
					LogHelper.Instance.Log("** driver provider {0} didn't match {1}", driverInfoData.ProviderName, driverInfo.Provider);
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
			LogHelper.Instance.Log("[UsbDeviceService] Unable to enumerate device tree.");
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
				LogHelper.Instance.Log("[UsbDeviceService] Clearing the device info list caught an exception.", e);
			}
		}
	}

	private StringBuilder MakeBuffer()
	{
		return new StringBuilder("")
		{
			Capacity = 1000
		};
	}

	private bool Match(IDeviceDescriptor left, IDeviceDescriptor right)
	{
		return Match(left, right, DeviceDescriptorMatchOption.VidPid);
	}

	private bool Match(IDeviceDescriptor left, IDeviceDescriptor right, DeviceDescriptorMatchOption option)
	{
		if (DeviceDescriptorMatchOption.Product == option)
		{
			return left.Product.ToLower().Equals(right.Product.ToLower(), StringComparison.CurrentCultureIgnoreCase);
		}
		if (DeviceDescriptorMatchOption.Vendor == option)
		{
			return left.Vendor.ToLower().Equals(right.Vendor.ToLower(), StringComparison.CurrentCultureIgnoreCase);
		}
		if (left.Product.ToLower().Equals(right.Product.ToLower(), StringComparison.CurrentCultureIgnoreCase))
		{
			return left.Vendor.ToLower().Equals(right.Vendor.ToLower(), StringComparison.CurrentCultureIgnoreCase);
		}
		return false;
	}
}
