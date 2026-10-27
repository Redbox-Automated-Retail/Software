namespace Redbox.HAL.Component.Model;

public interface IKioskConfiguration
{
	bool IsVmz { get; }

	int ReturnSlotBuffer { get; }
}
