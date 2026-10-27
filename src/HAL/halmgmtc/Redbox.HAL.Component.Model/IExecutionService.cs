namespace Redbox.HAL.Component.Model;

public interface IExecutionService
{
	void AddObserver(IExecutionEngineChangeObserver ob);

	void RemoveObserver(IExecutionEngineChangeObserver ob);

	void ChangeMode(EngineModes newMode);

	void Initialize(ErrorList errors);

	void StartScheduler();

	void Shutdown();

	void Suspend();

	void Restart();
}
