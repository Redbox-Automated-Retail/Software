namespace Redbox.HAL.Component.Model;

public interface IPowerCycleDevice
{
	bool Configured { get; }

	ErrorCodes CutPower();

	ErrorCodes SupplyPower();

	ErrorCodes CyclePower();
}
