using Redbox.HAL.Component.Model;

namespace Redbox.DirectShow;

public static class ExposureFixer
{
	private class CameraProperties
	{
		internal int Value { get; private set; }

		internal CameraControlFlags Flags { get; private set; }

		internal CameraProperties(int f, CameraControlFlags ccf)
		{
			Value = f;
			Flags = ccf;
		}
	}

	public static void ResetCameraProperties()
	{
		LogHelper.Instance.Log("Checking camera properties...");
		IDeviceDescriptor deviceDescriptor = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindActiveCamera(matchDriver: false);
		if (deviceDescriptor == null)
		{
			LogHelper.Instance.Log("Unable to find an active camera.");
			return;
		}
		LogHelper.Instance.Log("Found camera device {0} ( {1} )", deviceDescriptor.ToString(), deviceDescriptor.Friendlyname);
		if (!deviceDescriptor.Friendlyname.Equals("4th Gen"))
		{
			return;
		}
		FilterInfoCollection filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
		if (filterInfoCollection == null || filterInfoCollection.Count == 0)
		{
			LogHelper.Instance.Log("Unable to find any devices.");
			return;
		}
		PlayerDevice playerDevice = new PlayerDevice(filterInfoCollection[0].MonikerString, 500, debug: false);
		if (playerDevice == null)
		{
			LogHelper.Instance.Log("Found no devices.");
			return;
		}
		CameraProperties properties = GetProperties(playerDevice);
		if (properties != null)
		{
			if (CameraControlFlags.Auto != properties.Flags)
			{
				LogHelper.Instance.Log("Device is not set to auto.");
			}
			else if (!playerDevice.SetCameraProperty(CameraControlProperty.Exposure, properties.Value, CameraControlFlags.Manual))
			{
				LogHelper.Instance.Log(" !!Failed to set camera property to manual!!");
			}
			else
			{
				GetProperties(playerDevice);
			}
		}
	}

	private static CameraProperties GetProperties(PlayerDevice device)
	{
		int value = 0;
		if (!device.GetCameraProperty(CameraControlProperty.Exposure, out value, out var controlFlags))
		{
			LogHelper.Instance.Log("Unable to retrieve camera properties.");
			return null;
		}
		LogHelper.Instance.Log(" GetProperties: Value = {0} Flags = {1}", value, controlFlags);
		return new CameraProperties(value, controlFlags);
	}
}
