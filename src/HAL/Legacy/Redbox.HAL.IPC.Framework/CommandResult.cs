using System;
using System.Text;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

public class CommandResult
{
	private ErrorList m_errors;

	private MessageList m_messages;

	public bool Success { get; internal set; }

	public TimeSpan ExecutionTime { get; internal set; }

	public string ExtendedErrorMessage { get; internal set; }

	public MessageList Messages
	{
		get
		{
			if (m_messages == null)
			{
				m_messages = new MessageList();
			}
			return m_messages;
		}
	}

	public ErrorList Errors
	{
		get
		{
			if (m_errors == null)
			{
				m_errors = new ErrorList();
			}
			return m_errors;
		}
	}

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
		stringBuilder.AppendLine(string.Concat(" (Execution Time = ", ExecutionTime, ")"));
		return stringBuilder.ToString();
	}
}
