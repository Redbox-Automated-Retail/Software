using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

public sealed class CommandResult
{
	public bool Success { get; internal set; }

	public TimeSpan ExecutionTime { get; internal set; }

	public string ExtendedErrorMessage { get; internal set; }

	public MessageList Messages { get; private set; }

	public ErrorList Errors { get; private set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string message in Messages)
		{
			stringBuilder.AppendLine(message);
		}
		if (Success)
		{
			stringBuilder.Append("203 Command completed successfully.");
		}
		else
		{
			stringBuilder.Append("545 Command failed.");
			if (ExtendedErrorMessage != null)
			{
				stringBuilder.AppendFormat(" {0}", ExtendedErrorMessage);
			}
		}
		stringBuilder.AppendLine(" (Execution Time = " + ExecutionTime.ToString() + ")");
		return stringBuilder.ToString();
	}

	internal CommandResult()
	{
		Messages = new MessageList();
		Errors = new ErrorList();
	}
}
