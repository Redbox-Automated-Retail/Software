using System;
using System.Text;

namespace Redbox.IPC.Framework;

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
		return ToString(includeReturnResult: true);
	}

	public string ToString(bool includeReturnResult)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (includeReturnResult)
		{
			foreach (string message in Messages)
			{
				stringBuilder.AppendLine(message);
			}
		}
		if (Success)
		{
			stringBuilder.Append("203 Command Success.");
		}
		else
		{
			stringBuilder.Append("545 Command failed.");
			if (ExtendedErrorMessage != null)
			{
				stringBuilder.AppendFormat(" {0}", ExtendedErrorMessage);
			}
		}
		stringBuilder.AppendLine(" (Execution Time:" + Math.Truncate(ExecutionTime.TotalMilliseconds) + ")");
		return stringBuilder.ToString();
	}
}
