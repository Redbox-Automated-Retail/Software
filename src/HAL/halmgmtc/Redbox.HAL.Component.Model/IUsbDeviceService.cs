using System;
using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IUsbDeviceService
{
	IDeviceDescriptor FindActiveCamera(bool matchDriver);

	IDeviceDescriptor FromQueryString(string s);

	bool SetDeviceState(IDeviceDescriptor descriptor, DeviceState state);

	bool ChangeByHWID(IDeviceDescriptor descriptor, DeviceState state);

	DeviceStatus FindDeviceStatus(IDeviceDescriptor deviceInfo);

	bool MatchDriverByVendor(IDeviceDescriptor desc, IDriverDescriptor driverInfo);

	bool MatchDriver(IDeviceDescriptor descriptor, IDriverDescriptor driverInfo);

	IUsbDeviceSearchResult FindDeviceByVendor(IDeviceDescriptor desc);

	IUsbDeviceSearchResult FindDevice(IDeviceDescriptor descriptor);

	IUsbDeviceSearchResult FindVendorDevices(string _vendor);

	void EnumDevices(Action<string, string> a);

	List<IDeviceDescriptor> FindDevices(DeviceClass clazz);

	ITouchscreenDescriptor FindTouchScreen(bool matchDriver);

	ITouchscreenDescriptor FindTouchScreen();

	IDeviceDescriptor FindUps();
}
