namespace Redbox.HAL.Component.Model;

public interface IControlResponse
{
	bool Success { get; }

	bool TimedOut { get; }

	bool CommError { get; }

	string Diagnostic { get; }
}
