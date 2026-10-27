using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

public abstract class ClientSession : IIpcClientSession, IDisposable
{
	private int? m_timeout;

	private StringBuilder m_readBuilder;

	private readonly AutoResetEvent m_resetEvent = new AutoResetEvent(initialState: false);

	public int Timeout
	{
		get
		{
			return m_timeout ?? 30000;
		}
		set
		{
			m_timeout = value;
		}
	}

	public bool IsConnected { get; internal set; }

	public IIpcProtocol Protocol { get; private set; }

	public bool IsDisposed { get; private set; }

	protected abstract Stream Stream { get; }

	protected byte[] ReadBuffer { get; set; }

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

	public virtual void Dispose()
	{
		Close();
		m_resetEvent.Close();
		IsDisposed = true;
		if (Disposed != null)
		{
			Disposed(this, EventArgs.Empty);
		}
	}

	public void ConnectThrowOnError()
	{
		OnConnectThrowOnError();
	}

	public void Close()
	{
		if (!IsConnected)
		{
			return;
		}
		try
		{
			ExecuteCommand("quit");
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

	public bool IsStatusOK(List<string> messages)
	{
		bool result = false;
		if (messages.Count > 0)
		{
			result = messages[messages.Count - 1].StartsWith("203 Command");
		}
		return result;
	}

	public List<string> ExecuteCommand(string command)
	{
		Write(command);
		return ConsumeMessages();
	}

	protected abstract void OnConnectThrowOnError();

	protected List<string> ConsumeMessages()
	{
		if (ReadBuilder.Length > 0)
		{
			ReadBuilder.Length = 0;
		}
		List<string> list = new List<string>();
		Read(list);
		if (!m_resetEvent.WaitOne(Timeout))
		{
			list.Clear();
			ProtocolHelper.FormatErrors(new ErrorList { Error.NewError("J888", $"Timeout threshold {Timeout} exceeded.", "Reissue the command when the service is not as busy.") }, list);
			list.Add("545 Command failed.");
		}
		return list;
	}

	protected ClientSession(IPCProtocol protocol)
	{
		Protocol = protocol;
	}

	protected internal abstract bool IsConnectionAvailable();

	protected internal abstract void CustomClose();

	protected abstract int GetAvailableData();

	protected abstract bool CanRead();

	private void Read(List<string> messages)
	{
		try
		{
			Stream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, EndReadCallback, messages);
		}
		catch (IOException)
		{
			LogHelper.Instance.Log("ClientSession.Read caught an I/O exception; maybe server is down?");
			SetEvent();
		}
	}

	private void Write(string s)
	{
		try
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
		catch (IOException)
		{
			LogHelper.Instance.Log("ClientSession.Write caught an I/O exception; maybe server is down?");
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
		return ReadBuilder.ToString().IndexOf(Environment.NewLine) == -1;
	}

	private void EndReadCallback(IAsyncResult result)
	{
		try
		{
			int num = Stream.EndRead(result);
			if (num == 0)
			{
				SetEvent();
				return;
			}
			List<string> messages = (List<string>)result.AsyncState;
			string text = Encoding.ASCII.GetString(ReadBuffer, 0, num);
			ReadBuilder.Append(text);
			if (text.IndexOf(Environment.NewLine) == -1)
			{
				Read(messages);
			}
			else if (!ProcessReadBuilder(messages))
			{
				Read(messages);
			}
			else
			{
				SetEvent();
			}
		}
		catch (Exception)
		{
			LogHelper.Instance.Log("ClientSession.EndRead caught an exception; maybe server is down?");
			SetEvent();
		}
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

	private bool SetEvent()
	{
		try
		{
			m_resetEvent.Set();
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Caught an exception setting the event.", e);
			return false;
		}
	}
}
