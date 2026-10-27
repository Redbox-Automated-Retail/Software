using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class KioskConfigurationService : IKioskConfiguration
{
	public bool IsVmz => ControllerConfiguration.Instance.IsVMZMachine;

	public int ReturnSlotBuffer => ControllerConfiguration.Instance.ReturnSlotBuffer;
}
