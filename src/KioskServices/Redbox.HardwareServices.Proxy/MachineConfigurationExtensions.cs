using Redbox.HAL.Client.Executors;

namespace Redbox.HardwareServices.Proxy;

public static class MachineConfigurationExtensions
{
	public static bool IsNotValid(this MachineConfiguration mc)
	{
		if (mc.MaxDeck == 0)
		{
			return true;
		}
		if (mc.MaxSlot == 0)
		{
			return true;
		}
		if (mc.ExcludedSlots == null)
		{
			return true;
		}
		return false;
	}
}
