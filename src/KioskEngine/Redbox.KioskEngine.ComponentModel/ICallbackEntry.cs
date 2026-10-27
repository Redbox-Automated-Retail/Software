namespace Redbox.KioskEngine.ComponentModel;

public interface ICallbackEntry
{
	string Name { get; }

	void Invoke();
}
