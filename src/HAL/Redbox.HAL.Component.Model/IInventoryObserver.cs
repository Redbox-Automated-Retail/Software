namespace Redbox.HAL.Component.Model;

public interface IInventoryObserver
{
	void OnInventoryInitialize();

	void OnInventoryChange();

	void OnInventoryRebuild();
}
