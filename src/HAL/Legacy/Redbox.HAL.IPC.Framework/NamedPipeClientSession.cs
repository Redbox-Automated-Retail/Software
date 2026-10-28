using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

public sealed class NamedPipeClientSession : AbstractPipeClientSession
{
	private static long m_counter;

	public IPCProtocol Protocol { get; set; }

	public event Action<string> ServerEvent;

	public static NamedPipeClientSession MakeSession(IPCProtocol protocol)
	{
		string id = $"NamedPipeClientSession-{Interlocked.Increment(ref m_counter)}";
		return new NamedPipeClientSession("HALService7001", id);
	}

	public bool IsStatusOK(List<string> messages)
	{
		bool result = false;
		if (messages.Count > 0)
		{
			string text = messages[messages.Count - 1];
			result = text.StartsWith("203 Command");
		}
		return result;
	}

	public List<string> ExecuteCommand(string command)
	{
		Write(command);
		return ConsumeMessages();
	}

	~NamedPipeClientSession()
	{
		DisposeInner(disposing: false);
	}

	protected override List<string> ProcessRawMessage()
	{
		List<string> list = new List<string>();
		if (ProcessReadBuilder(list))
		{
			return list;
		}
		list.Clear();
		return list;
	}

	private string GetNextBufferLine()
	{
		string text = base.ReadBuilder.ToString();
		int num = text.IndexOf(Environment.NewLine);
		if (num != -1)
		{
			string text2 = text.Substring(0, num + Environment.NewLine.Length);
			base.ReadBuilder.Remove(0, num + Environment.NewLine.Length);
			return text2.Trim();
		}
		return string.Empty;
	}

	private bool ProcessReadBuilder(ICollection<string> messages)
	{
		string nextBufferLine;
		do
		{
			nextBufferLine = GetNextBufferLine();
			if (nextBufferLine == string.Empty && BufferHasMoreLines())
			{
				return false;
			}
			if (nextBufferLine.StartsWith("[MSG]"))
			{
				if (ServerEvent != null)
				{
					int num = nextBufferLine.IndexOf("]");
					if (num != -1)
					{
						ServerEvent(nextBufferLine.Substring(num + 1).Trim());
					}
				}
			}
			else
			{
				messages.Add(nextBufferLine);
			}
		}
		while (!nextBufferLine.StartsWith("Welcome!") && !nextBufferLine.StartsWith("Goodbye!") && !nextBufferLine.StartsWith("545 Command") && !nextBufferLine.StartsWith("203 Command"));
		return true;
	}

	private NamedPipeClientSession(string pipeName, string id)
		: base(id, pipeName)
	{
	}
}
