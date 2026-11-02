using System.Collections.Generic;

namespace RBA_SDK_Simulator;

/// <summary>Customer actions (swipe, insert, tap, ...) waiting to be performed on the simulated terminal.</summary>
public interface ICommandQueue
{
	void Enqueue(SimulatorCommand command);

	/// <summary>Removes and returns the first queued command that <paramref name="canRun" /> accepts.</summary>
	bool TryTake(System.Predicate<SimulatorCommand> canRun, out SimulatorCommand? command);

	/// <summary>Waits until a command is enqueued or the timeout elapses.</summary>
	bool WaitForCommand(int millisecondsTimeout);

	IReadOnlyList<SimulatorCommand> Pending { get; }

	void Clear();
}
