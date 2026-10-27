using System.Runtime.CompilerServices;
using Redbox.HAL.Core;

namespace Redbox.HAL.Camera.Framework;

public sealed class CameraInfo
{
	private readonly DeviceHelper Helper;

	[CompilerGenerated]
	private DeviceStatus _003CStatus_003Ek__BackingField;

	public DeviceStatus Status
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CStatus_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CStatus_003Ek__BackingField = value;
		}
	}

	public KnownCameras Revision { get; private set; }

	public DeviceDescriptor Descriptor
	{
		get
		{
			if (Revision != KnownCameras.None)
			{
				return CameraDescriptors.FromKnownCamera(Revision);
			}
			return null;
		}
	}

	public DriverDescriptor DriverDescriptor
	{
		get
		{
			if (Revision != KnownCameras.None)
			{
				return CameraDescriptors.ForKnownCamera(Revision);
			}
			return null;
		}
	}

	public bool FindActiveCamera()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		DeviceDescriptor val = CameraDescriptors.FromKnownCamera(KnownCameras.FourthGen);
		DeviceStatus val2 = Helper.FindDeviceStatus(DeviceClassGuids.Image, val);
		if ((int)val2 != 0)
		{
			Revision = KnownCameras.FourthGen;
			Status = val2;
			return true;
		}
		val = CameraDescriptors.FromKnownCamera(KnownCameras.ThirdGen);
		val2 = Helper.FindDeviceStatus(DeviceClassGuids.Image, val);
		if ((int)val2 != 0)
		{
			Revision = KnownCameras.ThirdGen;
			Status = val2;
			return true;
		}
		return false;
	}

	public bool ResetDriver()
	{
		if (!SetCameraStateInner((DeviceState)2))
		{
			return false;
		}
		PerfFunctions.Wait(1500);
		bool result = SetCameraStateInner((DeviceState)1);
		PerfFunctions.SpinWait(1500);
		return result;
	}

	public bool SetCameraState(DeviceState state)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return SetCameraStateInner(state);
	}

	public bool CorrectDriverRunning()
	{
		if (DriverDescriptor == null || Descriptor == null)
		{
			return false;
		}
		return Helper.MatchDriver(Descriptor, DriverDescriptor, DeviceClassGuids.Image);
	}

	public CameraInfo(DeviceHelper helper)
	{
		Status = (DeviceStatus)0;
		Revision = KnownCameras.None;
		Helper = helper;
	}

	private bool SetCameraStateInner(DeviceState state)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Helper.ChangeByHWID(Descriptor, DeviceClassGuids.Image, state);
	}
}
