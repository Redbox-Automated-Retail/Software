namespace Redbox.HAL.Component.Model;

public interface IControlSystemObserver
{
	void OnSystemInitialize(ErrorCodes initError);

	void OnSystemShutdown();
}
