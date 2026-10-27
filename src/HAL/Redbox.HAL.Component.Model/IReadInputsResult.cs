using System;

namespace Redbox.HAL.Component.Model;

public interface IReadInputsResult<T>
{
	ErrorCodes Error { get; }

	bool Success { get; }

	int InputCount { get; }

	void Log();

	void Log(LogEntryType type);

	bool IsInputActive(T input);

	bool IsInState(T input, InputState state);

	void Foreach(Action<T> action);
}
