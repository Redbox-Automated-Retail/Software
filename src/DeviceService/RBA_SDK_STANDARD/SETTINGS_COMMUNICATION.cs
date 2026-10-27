using RBA_SDK_ComponentModel;

namespace RBA_SDK;

public struct SETTINGS_COMMUNICATION
{
	public uint interface_id;

	public SETTINGS_ETHERNET ip_config;

	public SETTINGS_RS232 rs232_config;

	public SETTINGS_USB_HID usbhid_config;

	public SETTINGS_BLUETOOTH bt_config;
}
