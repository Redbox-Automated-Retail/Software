namespace Redbox.HAL.Component.Model;

public interface IPowerEventObserver
{
	void OnSuspend();

	void OnResume();
}
