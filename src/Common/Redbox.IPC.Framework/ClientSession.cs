using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.IPC.Framework.NamedPipes;
using Redbox.IPC.Framework.Remoting;
using Redbox.IPC.Framework.Sockets;

namespace Redbox.IPC.Framework;

public abstract class ClientSession : IDisposable
{
	private StringBuilder m_readBuilder;

	private readonly AutoResetEvent m_resetEvent = new AutoResetEvent(initialState: false);

	public int Timeout { get; set; }

	public bool IsDisposed { get; protected set; }

	public string HostName { get; protected set; }

	public bool IsConnected { get; internal set; }

	public IPCProtocol Protocol { get; protected set; }

	protected abstract Stream Stream { get; }

	protected byte[] ReadBuffer { get; set; }

	protected internal ClientSessionPool OwningPool { get; set; }

	private StringBuilder ReadBuilder
	{
		get
		{
			if (m_readBuilder == null)
			{
				m_readBuilder = new StringBuilder();
			}
			return m_readBuilder;
		}
	}

	public event EventHandler Disposed;

	public event Action<string> ServerEvent;

	public static ClientSession GetClientSession(IPCProtocol protocol, int? timeout)
	{
		switch (protocol.Channel)
		{
		case ChannelType.NamedPipe:
			return GetNamedPipesClientSession(protocol);
		case ChannelType.Socket:
			return GetTcpClientSession(protocol, timeout);
		case ChannelType.Remoting:
			return new RemotingClientSession(protocol, timeout ?? 5000);
		default:
			LogHelper.Instance.Log($"Unrecognized channel type {protocol.Channel}; please fix your protocol string.", LogEntryType.Error);
			return null;
		}
	}

	public abstract bool Connect();

	public abstract void ConnectThrowOnError();

	public virtual void Close()
	{
		if (!IsConnected)
		{
			return;
		}
		try
		{
			if (Protocol.Channel == ChannelType.Socket || Protocol.Channel == ChannelType.NamedPipe)
			{
				ExecuteCommand("quit");
			}
		}
		catch (IOException)
		{
		}
		finally
		{
			IsConnected = false;
			CustomClose();
		}
	}

	public virtual void Dispose()
	{
		if (!IsDisposed)
		{
			IsDisposed = true;
			Close();
			try
			{
				m_resetEvent.Close();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in Dispose closing the m_resetEvent.", e);
			}
			if (Disposed != null)
			{
				Disposed(this, EventArgs.Empty);
			}
		}
	}

	public bool IsStatusOk(List<string> messages)
	{
		bool result = false;
		if (messages.Count > 0)
		{
			result = messages[messages.Count - 1].StartsWith("203 Command");
		}
		return result;
	}

	public virtual List<string> ExecuteCommand(string command)
	{
		Write(command);
		return ConsumeMessages();
	}

	protected ClientSession(IPCProtocol protocol)
	{
		Protocol = protocol;
	}

	protected List<string> ConsumeMessages()
	{
		List<string> list = new List<string>();
		try
		{
			ResetReadBuilder();
			Read(list);
			if (!m_resetEvent.WaitOne(Timeout))
			{
				list.Clear();
				ProtocolHelper.FormatErrors(new ErrorList { Error.NewError("J888", "Timeout threshold exceeded.", "Reissue the command when the service is not as busy.") }, list);
				list.Add("545 Command failed.");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ClientSession.ConsumeMessages.", e);
			throw;
		}
		return list;
	}

	protected internal abstract bool IsConnectionAvailable();

	protected internal abstract void CustomClose();

	protected abstract int GetAvailableData();

	protected abstract bool CanRead();

	internal void ProcessServerEvent(string line)
	{
		if (ServerEvent != null)
		{
			int num = line.IndexOf("]");
			if (num != -1)
			{
				ServerEvent(line.Substring(num + 1).Trim());
			}
		}
	}

	private static ClientSession GetTcpClientSession(IPCProtocol protocol, int? timeout)
	{
		return new TcpClientSession(protocol, timeout);
	}

	private static ClientSession GetNamedPipesClientSession(IPCProtocol protocol)
	{
		return new NamedPipeClientSession(protocol);
	}

	private void Read(List<string> messages)
	{
		Stream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, EndReadCallback, messages);
	}

	private void Write(string s)
	{
		if (Stream.CanWrite)
		{
			if (!s.EndsWith(Environment.NewLine))
			{
				s = $"{s}{Environment.NewLine}";
			}
			Stream.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);
		}
	}

	private void ResetReadBuilder()
	{
		if (ReadBuilder.Length != 0)
		{
			ReadBuilder.Length = 0;
		}
	}

	private string GetNextBufferLine()
	{
		string text = ReadBuilder.ToString();
		int num = text.IndexOf(Environment.NewLine);
		if (num != -1)
		{
			string text2 = text.Substring(0, num + Environment.NewLine.Length);
			ReadBuilder.Remove(0, num + Environment.NewLine.Length);
			return text2.Trim();
		}
		return string.Empty;
	}

	private bool BufferHasMoreLines()
	{
		return ReadBuilder.ToString().IndexOf(Environment.NewLine) != -1;
	}

	private void EndReadCallback(IAsyncResult result)
	{
		try
		{
			List<string> messages = (List<string>)result.AsyncState;
			int num = Stream.EndRead(result);
			if (num == 0)
			{
				SafeSetEvent();
				return;
			}
			string text = Encoding.ASCII.GetString(ReadBuffer, 0, num);
			ReadBuilder.Append(text);
			if (text.IndexOf(Environment.NewLine) == -1 && text.IndexOf("\n") == -1)
			{
				Read(messages);
			}
			else if (!ProcessReadBuilder(messages))
			{
				Read(messages);
			}
			else
			{
				SafeSetEvent();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ClientSession.EndReadCallback", e);
			SafeSetEvent();
		}
	}

	private bool ProcessReadBuilder(ICollection<string> messages)
	{
		string nextBufferLine;
		do
		{
			nextBufferLine = GetNextBufferLine();
			if (nextBufferLine == null || (nextBufferLine == string.Empty && !BufferHasMoreLines()))
			{
				return false;
			}
			if (nextBufferLine.StartsWith("[MSG]"))
			{
				ProcessServerEvent(nextBufferLine);
			}
			else
			{
				messages.Add(nextBufferLine);
			}
		}
		while (!nextBufferLine.StartsWith("Welcome!") && !nextBufferLine.StartsWith("Goodbye!") && !nextBufferLine.StartsWith("545 Command") && !nextBufferLine.StartsWith("203 Command"));
		return true;
	}

	private void SafeSetEvent()
	{
		try
		{
			m_resetEvent.Set();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ClientSession.SafeSetEvent.", e);
		}
	}
}
