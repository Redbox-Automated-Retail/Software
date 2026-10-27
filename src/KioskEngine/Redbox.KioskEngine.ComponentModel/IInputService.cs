namespace Redbox.KioskEngine.ComponentModel;

public interface IInputService
{
	void Reset();

	void PerformSingleClick();

	void PerformDoubleClick();

	void ClearIdleHandlers();

	void SimulateKeyStrokes(string input);

	void RegisterIdleHandler(string name, IdleEventHandler handler);

	void RemoveIdleHandler(string name);

	void ClearActivatedHandlers();

	void RegisterActivatedHandler(string name, ActivatedEventHandler handler);

	void RemoveActivatedHandler(string name);

	void RegisterMouseClickHandler(string name, MouseClickHandler handler);

	void RemoveMouseClickHandler(string name);

	void RegisterKeyPressHandler(string name, KeyPressHandler handler);

	void RemoveKeyPressHandler(string name);

	void RegisterMouseDoubleClickHandler(string name, MouseClickHandler handler);

	void RemoveMouseDoubleClickHandler(string name);

	void ClearKeyPressHandlers();

	void ClearMouseClickHandlers();

	void ClearMouseDoubleClickHandlers();
}
