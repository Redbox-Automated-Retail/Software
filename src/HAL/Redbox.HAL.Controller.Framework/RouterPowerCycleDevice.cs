using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

[PowerCycleDevice(Device = PowerCycleDevices.Router)]
internal sealed class RouterPowerCycleDevice : PowerCycleDevice, IConfigurationObserver
{
	protected internal override PowerCycleDevices Device => PowerCycleDevices.Router;

	public void NotifyConfigurationLoaded()
	{
		OnConfigurationUpdate();
	}

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
		OnConfigurationUpdate();
	}

	protected override ErrorCodes OnPowerCut()
	{
		return ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.PowerAux21).Error;
	}

	protected override ErrorCodes OnPowerSupply()
	{
		return ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.DisableAux21).Error;
	}

	internal RouterPowerCycleDevice()
	{
		ControllerConfiguration.Instance.AddObserver(this);
	}

	private void OnConfigurationUpdate()
	{
		LogHelper.Instance.Log("[RouterPowerCycleDevice] Configuration update");
		CyclePause = ControllerConfiguration.Instance.RouterPowerCyclePause;
		base.Configured = CyclePause > 0;
	}
}
