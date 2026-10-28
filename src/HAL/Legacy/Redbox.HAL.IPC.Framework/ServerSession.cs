using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

public abstract class ServerSession : ISession, IMessageSink
{
	public enum SessionType
	{
		NamedPipe,
		TcpSocket
	}

	private StringBuilder m_readBuilder;

	private ParameterDictionary m_context;

	private List<string> m_messageSinkQueue;

	private readonly object m_syncLock = new object();

	public ParameterDictionary Context
	{
		get
		{
			if (m_context == null)
			{
				m_context = new ParameterDictionary();
			}
			return m_context;
		}
	}

	public bool LogDetailedMessages { get; set; }

	protected abstract int ReadBufferSize { get; }

	protected Stream Stream { get; set; }

	protected byte[] ReadBuffer { get; set; }

	private bool IsSecure
	{
		get
		{
			if (ServiceHost.EncryptionProtocol != SslProtocols.None)
			{
				return ServiceHost.Certificate != null;
			}
			return false;
		}
	}

	private IPCServiceHost ServiceHost { get; set; }

	private List<string> MessageSinkQueue
	{
		get
		{
			if (m_messageSinkQueue == null)
			{
				m_messageSinkQueue = new List<string>();
			}
			return m_messageSinkQueue;
		}
	}

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

	public event EventHandler Disconnect;

	public ServerSession(IPCServiceHost serviceHost, int bufferSize)
	{
		ServiceHost = serviceHost;
		ReadBuffer = new byte[bufferSize];
		LogDetailedMessages = false;
	}

	public void Start()
	{
		Write($"Welcome! {ServiceHost.Name}, Version {ServiceHost.Version}, {ServiceHost.Copyright}");
		Read();
	}

	public bool Send(string message)
	{
		string text = $"[MSG] {message}";
		if (LogDetailedMessages)
		{
			LogHelper.Instance.Log(text);
		}
		EnqueueSinkMessage(text);
		return true;
	}

	public bool CloseClients()
	{
		return CloseClientsInternal();
	}

	public bool CloseStreams()
	{
		return CloseStreamsInternal();
	}

	protected abstract bool CloseClientsInternal();

	protected abstract bool CloseStreamsInternal();

	protected void NegotiateSecurityLayer(Stream stream)
	{
		if (!IsSecure)
		{
			Stream = stream;
			return;
		}
		SslStream sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
		sslStream.ReadTimeout = 30000;
		sslStream.WriteTimeout = 30000;
		SslStream sslStream2 = sslStream;
		sslStream2.AuthenticateAsServer(ServiceHost.Certificate, clientCertificateRequired: false, SslProtocols.Tls, checkCertificateRevocation: true);
		Stream = sslStream2;
	}

	private void RaiseDisconnect()
	{
		if (Disconnect != null)
		{
			Disconnect(this, EventArgs.Empty);
		}
	}

	private void Read()
	{
		try
		{
			Stream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, EndReadCallback, null);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("ServerSession.Read caught an exception", e);
			QuitSession(null);
		}
	}

	private void Write(string s)
	{
		if (!Stream.CanWrite)
		{
			return;
		}
		try
		{
			if (!s.EndsWith(Environment.NewLine))
			{
				s = $"{s}{Environment.NewLine}";
			}
			Stream.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);
			Interlocked.Add(ref Statistics.Instance.NumberOfBytesSent, s.Length);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("There was an unhandled exception writing to the client session.", e);
			QuitSession(null);
		}
	}

	private void EndReadCallback(IAsyncResult result)
	{
		try
		{
			int num = Stream.EndRead(result);
			if (num == 0)
			{
				return;
			}
			Interlocked.Add(ref Statistics.Instance.NumberOfBytesReceived, num);
			string value = Encoding.ASCII.GetString(ReadBuffer, 0, num);
			ReadBuilder.Append(value);
			if (!HasMoreBufferLines())
			{
				ThreadPool.QueueUserWorkItem(delegate
				{
					ProcessReadBuilder();
				});
			}
			else
			{
				Read();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("ClientSession.Execute has caught an unhandled exception.", e);
			QuitSession(null);
		}
	}

	private void ProcessReadBuilder()
	{
		while (true)
		{
			string nextBufferLine = GetNextBufferLine();
			if (nextBufferLine == string.Empty && HasMoreBufferLines())
			{
				Read();
				return;
			}
			if (nextBufferLine.StartsWith("quit", StringComparison.CurrentCultureIgnoreCase))
			{
				break;
			}
			CommandResult commandResult = CommandService.Instance.Execute(this, nextBufferLine);
			ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
			FlushSinkQueue(commandResult.Messages);
			string text = commandResult.ToString();
			Write(text);
			if (LogDetailedMessages)
			{
				LogHelper.Instance.Log(text, LogEntryType.Debug);
			}
		}
		QuitSession("Goodbye!");
	}

	private bool QuitSession(string msg)
	{
		if (msg != null)
		{
			if (LogDetailedMessages)
			{
				LogHelper.Instance.Log(msg, LogEntryType.Debug);
			}
			Write(msg);
		}
		RaiseDisconnect();
		bool result = false;
		try
		{
			CloseClients();
			CloseStreams();
			result = true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Caught exception in QuitSession", e);
		}
		try
		{
			ServiceHost.UnregisterSession(this);
			return result;
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("QuitSession: unable to unregister session.", e2);
			return false;
		}
	}

	private void EnqueueSinkMessage(string message)
	{
		lock (m_syncLock)
		{
			MessageSinkQueue.Add(message);
		}
	}

	private void FlushSinkQueue(ICollection<string> messages)
	{
		lock (m_syncLock)
		{
			while (MessageSinkQueue.Count > 0)
			{
				string item = MessageSinkQueue[0];
				MessageSinkQueue.RemoveAt(0);
				messages.Add(item);
			}
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

	private bool HasMoreBufferLines()
	{
		return ReadBuilder.ToString().IndexOf(Environment.NewLine) == -1;
	}

	private void ResetReadBuilder()
	{
		if (ReadBuilder.Length != 0)
		{
			ReadBuilder.Length = 0;
		}
	}
}
