namespace Redbox.HAL.Component.Model;

public interface IExecutionEngineChangeObserver
{
	void OnEngineChange(EngineModes newMode);

	void OnShutdown();
}
