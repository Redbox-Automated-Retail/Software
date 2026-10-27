using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

public sealed class ExecutionResult : IExecutionResult
{
	public ErrorList Errors { get; private set; }

	public TimeSpan? ExecutionTime { get; set; }

	internal bool RestartAtCurrentIP { get; set; }

	internal Instruction SwitchToContext { get; set; }

	internal void Reset()
	{
		Errors.Clear();
		ExecutionTime = null;
		SwitchToContext = null;
		RestartAtCurrentIP = false;
	}

	internal void UpdateExecutionTime(TimeSpan timeSpan)
	{
		if (!ExecutionTime.HasValue)
		{
			ExecutionTime = timeSpan;
		}
		else
		{
			ExecutionTime += timeSpan;
		}
	}

	internal void AddMissingOperandError(string details)
	{
		AddError(ExecutionErrors.MissingOperand, details);
	}

	internal void AddInvalidOperandError(string details)
	{
		AddError(ExecutionErrors.InvalidOperand, details);
	}

	internal void AddError(ExecutionErrors error, string details)
	{
		Error item = Error.NewError(ExecutionErrorsTable.ErrorCodeTable[error], ExecutionErrorsTable.ErrorTable[error], details);
		Errors.Add(item);
	}

	public ExecutionResult()
	{
		Errors = new ErrorList();
	}
}
