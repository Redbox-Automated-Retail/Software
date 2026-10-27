using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using Redbox.Core;

namespace Redbox.IPC.Framework;

public abstract class ServerSession : ISession, IMessageSink
{
	private Action<string, string> _paramAction;

	private Action<ISession> _beforeCommandAction;

	private List<string> _filters = new List<string>();

	private bool _enableFilters;

	private StringBuilder m_readBuilder;

	private ParameterDictionary m_context;

	private List<string> m_messageSinkQueue;

	private IDictionary<string, object> m_properties;

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

	public IDictionary<string, object> Properties
	{
		get
		{
			if (m_properties == null)
			{
				m_properties = new Dictionary<string, object>();
			}
			return m_properties;
		}
	}

	public List<string> Filters => _filters;

	public bool EnableFilters
	{
		get
		{
			return _enableFilters;
		}
		set
		{
			_enableFilters = value;
		}
	}

	protected byte[] ReadBuffer { get; set; }

	protected Stream Stream { get; set; }

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

	public event EventHandler Disconnect;

	public void Start()
	{
		Write($"Welcome! {ServiceHost.Name}, Version {ServiceHost.Version}, {ServiceHost.Copyright}");
		Read();
	}

	public void SetFilters(List<string> filters)
	{
		_filters.Clear();
		filters.ForEach(_filters.Add);
	}

	public void SetParamAction(Action<string, string> paramAction)
	{
		_paramAction = paramAction;
	}

	public void SetBeforeCommandAction(Action<ISession> beforeCommandAction)
	{
		_beforeCommandAction = beforeCommandAction;
	}

	public bool Send(string message)
	{
		string message2 = $"[MSG] {message}";
		LogHelper.Instance.Log(message2, LogEntryType.Info);
		EnqueueSinkMessage(message2);
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

	public bool IsConnected()
	{
		return IsConnectedInternal();
	}

	protected ServerSession(IPCServiceHost serviceHost)
	{
		ServiceHost = serviceHost;
	}

	protected void NegotiateSecurityLayer(Stream stream)
	{
		if (!IsSecure)
		{
			Stream = stream;
			return;
		}
		SslStream sslStream = new SslStream(stream, leaveInnerStreamOpen: false)
		{
			ReadTimeout = 30000,
			WriteTimeout = 30000
		};
		sslStream.AuthenticateAsServer(ServiceHost.Certificate, clientCertificateRequired: false, SslProtocols.Tls, checkCertificateRevocation: true);
		Stream = sslStream;
	}

	protected abstract bool IsConnectedInternal();

	protected abstract bool CloseStreamsInternal();

	protected abstract bool CloseClientsInternal();

	protected abstract string GetRemoteHostIP();

	private void Read()
	{
		try
		{
			ReadBuffer = BufferPool.Instance.Checkout();
			Stream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, EndReadCallback, null);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in Read.", e);
			InternalClose();
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
			LogHelper.Instance.Log("An unhandled exception was raised in Write.", e);
			InternalClose();
		}
	}

	private void EndReadCallback(IAsyncResult result)
	{
		try
		{
			int num = Stream.EndRead(result);
			if (num == 0)
			{
				InternalClose();
				return;
			}
			Interlocked.Add(ref Statistics.Instance.NumberOfBytesReceived, num);
			ReadBuilder.Append(Encoding.ASCII.GetString(ReadBuffer, 0, num));
			if (ServiceHost.WorkerPool.QueueWorkItem(delegate
			{
				ProcessReadBuilder();
			}))
			{
				return;
			}
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					using ExecutionTimer timer = new ExecutionTimer();
					string serverBusyResult = CommandService.Instance.GetServerBusyResult(timer, ServiceHost.WorkerPool.MaximumThreads);
					LogHelper.Instance.Log(serverBusyResult);
					Write(serverBusyResult);
				}
				catch (Exception e2)
				{
					LogHelper.Instance.Log("An unhandled exception was raised in ProcessReadBuilder, throttling client call.", e2);
				}
				finally
				{
					LogHelper.Instance.Log("Call Read from ProcessReadBuilder 5.", LogEntryType.Debug);
					Read();
				}
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EndReadCallback.", e);
			InternalClose();
		}
		finally
		{
			BufferPool.Instance.Checkin(ReadBuffer);
		}
	}

	private void ProcessReadBuilder()
	{
		string nextBufferLine;
		try
		{
			nextBufferLine = GetNextBufferLine();
			if (string.IsNullOrEmpty(nextBufferLine) && NoMoreBufferLines())
			{
				LogHelper.Instance.Log("Call Read from ProcessReadBuilder 1.", LogEntryType.Debug);
				Read();
				return;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ProcessReadBuilder near GetNextBufferLine.", e);
			LogHelper.Instance.Log("Call Read from ProcessReadBuilder 2.", LogEntryType.Debug);
			Read();
			return;
		}
		if (string.IsNullOrEmpty(nextBufferLine))
		{
			LogHelper.Instance.Log("Call Read from ProcessReadBuilder 3.", LogEntryType.Debug);
			Read();
			return;
		}
		if (nextBufferLine.StartsWith("quit", StringComparison.CurrentCultureIgnoreCase))
		{
			LogHelper.Instance.Log(nextBufferLine, LogEntryType.Debug);
			Write("Goodbye!");
			InternalClose();
			return;
		}
		try
		{
			string originIP = string.Empty;
			try
			{
				originIP = GetRemoteHostIP();
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("Error accessing remote host ip.", e2);
			}
			if (_beforeCommandAction != null)
			{
				_beforeCommandAction(this);
			}
			CommandResult commandResult = CommandService.Instance.Execute(this, originIP, nextBufferLine, Filters, EnableFilters, _paramAction);
			if (commandResult != null)
			{
				ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
				FlushSinkQueue(commandResult.Messages);
				string s = commandResult.ToString();
				Write(s);
			}
		}
		catch (Exception e3)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ProcessReadBuilder thread pool work item, executing command.", e3);
		}
		finally
		{
			LogHelper.Instance.Log("Call Read from ProcessReadBuilder 4.", LogEntryType.Debug);
			Read();
		}
	}

	private void InternalClose()
	{
		try
		{
			if (Disconnect != null)
			{
				Disconnect(this, EventArgs.Empty);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in Disconnect event handler.", e);
		}
		try
		{
			CloseClients();
			CloseStreams();
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in TcpServerSession.InternalClose.", e2);
		}
		finally
		{
			BufferPool.Instance.Checkin(ReadBuffer);
			if (ServiceHost != null)
			{
				ServiceHost.UnregisterSession(this);
			}
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

	private bool NoMoreBufferLines()
	{
		return ReadBuilder.ToString().IndexOf(Environment.NewLine) == -1;
	}

	private void ResetReadBuilder()
	{
		ReadBuilder.Length = 0;
	}
}
