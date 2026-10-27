using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.IPC.Framework;

public class ClientCommandResult
{
	private ErrorList m_errors;

	private List<string> m_commandMessages;

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

	public List<string> CommandMessages
	{
		get
		{
			if (m_commandMessages == null)
			{
				m_commandMessages = new List<string>();
			}
			return m_commandMessages;
		}
	}

	public bool Success { get; set; }

	public string CommandText { get; internal set; }

	public string StatusMessage { get; internal set; }

	public TimeSpan ExecutionTime { get; internal set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string commandMessage in CommandMessages)
		{
			stringBuilder.AppendLine(commandMessage);
		}
		foreach (Error error in Errors)
		{
			stringBuilder.AppendLine(error.ToString());
			stringBuilder.AppendFormat("Details: {0}\n\n", error.Details);
		}
		if (Success)
		{
			stringBuilder.Append("203 Command Success. (Execution Time:" + Math.Truncate(ExecutionTime.TotalMilliseconds) + ")");
		}
		else if (!string.IsNullOrEmpty(StatusMessage))
		{
			stringBuilder.Append(StatusMessage);
		}
		else
		{
			stringBuilder.Append("545 Command failed. (Execution Time:" + Math.Truncate(ExecutionTime.TotalMilliseconds) + ")");
		}
		stringBuilder.AppendLine();
		return stringBuilder.ToString();
	}
}
