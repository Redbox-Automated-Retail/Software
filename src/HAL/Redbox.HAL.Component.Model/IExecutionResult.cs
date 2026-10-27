using System;

namespace Redbox.HAL.Component.Model;

public interface IExecutionResult
{
	ErrorList Errors { get; }

	TimeSpan? ExecutionTime { get; }
}
