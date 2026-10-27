using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Redbox.USB;

public class UsbNotification
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
	internal class DeviceBroadcastInterface
	{
		public int Size;

		public int DeviceType;

		public int Reserved;

		public Guid ClassGuid;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string Name;
	}

	internal const int DEVTYP_DEVICEINTERFACE = 5;

	internal const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;

	internal const int WM_DEVICECHANGE = 537;

	internal const int DEVICE_ARRIVAL = 32768;

	internal const int DEVICE_REMOVECOMPLETE = 32772;

	public static NotificationEventType HandleMessage(ref Message m)
	{
		if (m.Msg == 537)
		{
			switch (m.WParam.ToInt32())
			{
			case 32768:
				return NotificationEventType.DeviceArrived;
			case 32772:
				return NotificationEventType.DeviceRemoved;
			}
		}
		return NotificationEventType.None;
	}

	public static IntPtr RegisterForUsbEvents(IntPtr hWnd)
	{
		Guid lpHidGuid = default(Guid);
		HidDeviceManager.HidD_GetHidGuid(ref lpHidGuid);
		DeviceBroadcastInterface deviceBroadcastInterface = new DeviceBroadcastInterface();
		deviceBroadcastInterface.Size = Marshal.SizeOf(deviceBroadcastInterface);
		deviceBroadcastInterface.ClassGuid = lpHidGuid;
		deviceBroadcastInterface.DeviceType = 5;
		deviceBroadcastInterface.Reserved = 0;
		return RegisterDeviceNotification(hWnd, deviceBroadcastInterface, 0u);
	}

	public static bool UnregisterForUsbEvents(IntPtr hHandle)
	{
		return UnregisterDeviceNotification(hHandle);
	}

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern IntPtr RegisterDeviceNotification(IntPtr hwnd, DeviceBroadcastInterface oInterface, uint nFlags);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool UnregisterDeviceNotification(IntPtr hHandle);
}
