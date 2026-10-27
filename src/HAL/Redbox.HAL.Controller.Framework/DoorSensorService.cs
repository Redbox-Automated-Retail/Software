using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DoorSensorService : IConfigurationObserver, IDoorSensorService, IMoveVeto
{
	private bool m_softwareOverride;

	private bool m_enabled = true;

	public bool SensorsEnabled
	{
		get
		{
			if (!m_enabled)
			{
				return false;
			}
			return !SoftwareOverride;
		}
	}

	public bool SoftwareOverride
	{
		get
		{
			return m_softwareOverride;
		}
		set
		{
			m_softwareOverride = value;
			ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap().SetValue("SensorSoftwareOverride", m_softwareOverride);
		}
	}

	public ErrorCodes CanMove()
	{
		DoorSensorResult doorSensorResult = Query();
		if (doorSensorResult == DoorSensorResult.Ok)
		{
			return ErrorCodes.Success;
		}
		LogHelper.Instance.Log($"Door sensor query returned {doorSensorResult.ToString()}", LogEntryType.Error);
		return ErrorCodes.DoorOpen;
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[DoorSensorService] Configuration load.");
		m_enabled = ControllerConfiguration.Instance.IsVMZMachine;
		if (!m_enabled)
		{
			LogHelper.Instance.Log("The door sensors are not configured.");
		}
		else if (SoftwareOverride)
		{
			LogHelper.Instance.Log("** WARNING **: Door sensors are configured, however a software override in place.");
			LogHelper.Instance.Log("** WARNING **: Kiosk is operating without door sensors.");
		}
		else
		{
			LogHelper.Instance.Log("Door sensors are configured.");
		}
	}

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
	}

	public DoorSensorResult Query()
	{
		if (!m_enabled || SoftwareOverride)
		{
			return DoorSensorResult.Ok;
		}
		return RawQuery();
	}

	public DoorSensorResult QueryStateForDisplay()
	{
		if (!m_enabled)
		{
			return DoorSensorResult.Ok;
		}
		if (!m_softwareOverride)
		{
			return RawQuery();
		}
		return DoorSensorResult.SoftwareOverride;
	}

	internal DoorSensorService()
	{
		IPersistentMap map = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
		m_softwareOverride = map.GetValue("SensorSoftwareOverride", def: false);
		ServiceLocator.Instance.GetService<IMotionControlService>().AddVeto(this);
		ControllerConfiguration.Instance.AddObserver(this);
	}

	private DoorSensorResult RawQuery()
	{
		IReadInputsResult<AuxInputs> readInputsResult = ServiceLocator.Instance.GetService<IControlSystem>().ReadAuxInputs();
		if (!readInputsResult.Success)
		{
			return DoorSensorResult.AuxReadError;
		}
		if (!readInputsResult.IsInputActive(AuxInputs.QlmDown))
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "[DoorSensorService] read inputs shows door not closed");
			readInputsResult.Log(LogEntryType.Error);
			return DoorSensorResult.FrontDoor;
		}
		return DoorSensorResult.Ok;
	}
}
