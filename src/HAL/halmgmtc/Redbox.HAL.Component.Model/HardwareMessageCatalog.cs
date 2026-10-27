namespace Redbox.HAL.Component.Model;

public static class HardwareMessageCatalog
{
	public static string MachineFull()
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		return $"The machine is full; there is {service.GetMachineEmptyCount()} empty slot(s).";
	}

	public static string LookupFailure(string id)
	{
		return $"LOOKUP of item {id} returned NOT FOUND.";
	}
}
