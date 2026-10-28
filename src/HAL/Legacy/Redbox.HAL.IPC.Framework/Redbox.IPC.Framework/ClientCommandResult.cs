using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

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
}
