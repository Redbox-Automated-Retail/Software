namespace Redbox.Rental.Model.EngineApplication;

public interface IEngineApplicationService
{
	int ProcessId { get; }

	PerformShutdownResponse PerformShutdown();
}
