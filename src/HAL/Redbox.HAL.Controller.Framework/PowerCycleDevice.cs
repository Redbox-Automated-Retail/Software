using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal abstract class PowerCycleDevice : IPowerCycleDevice
{
	protected int CyclePause;

	public bool Configured { get; protected set; }

	protected internal abstract PowerCycleDevices Device { get; }

	public ErrorCodes CutPower()
	{
		return OnPowerCut();
	}

	public ErrorCodes SupplyPower()
	{
		return OnPowerSupply();
	}

	public ErrorCodes CyclePower()
	{
		ErrorCodes errorCodes = CutPower();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		Thread.Sleep(CyclePause);
		return SupplyPower();
	}

	protected abstract ErrorCodes OnPowerCut();

	protected abstract ErrorCodes OnPowerSupply();
}
