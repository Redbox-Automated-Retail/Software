using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Client;

public abstract class ClientSession : IIpcClientSession, IDisposable
{
	protected readonly bool LogDetailed;

	private int? m_timeout;

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

	public bool IsConnected { get; private set; }

	public string Identifier { get; private set; }

	public IIpcProtocol Protocol { get; protected set; }

	public bool IsDisposed { get; private set; }

	protected abstract IIPCChannel Channel { get; }

	internal bool SupportsEvents => ServerEvent != null;

	public event Action<string> ServerEvent;

	public void Dispose()
	{
		if (IsDisposed)
		{
			return;
		}
		if (LogDetailed)
		{
			LogHelper.Instance.Log("[ClientSession] Disposing id {0}", Identifier);
		}
		IsDisposed = true;
		try
		{
			ExecuteCommand("quit");
			Channel.Dispose();
		}
		catch (IOException)
		{
		}
		finally
		{
			OnDispose();
		}
	}

	public void ConnectThrowOnError()
	{
		IsConnected = OnConnect();
		if (!IsConnected)
		{
			throw new TimeoutException($"Failed to connect on {Protocol.ToString()}");
		}
		ConsumeMessages();
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
		if (LogDetailed)
		{
			LogHelper.Instance.Log("[ClientSession-{0}] executing {1}", Identifier, command);
		}
		RedboxChannelDecorator.Write(Channel, command);
		MessageList messageList = ConsumeMessages();
		if (LogDetailed)
		{
			foreach (string item in messageList)
			{
				LogHelper.Instance.Log("[ClientSession-{0}]  Response msg {1}", Identifier, item);
			}
		}
		return messageList;
	}

	protected virtual void OnDispose()
	{
	}

	protected abstract bool OnConnect();

	protected MessageList ConsumeMessages()
	{
		ClientResponse clientResponse = new ClientResponse(this, LogDetailed);
		Channel.Read(clientResponse);
		if (!clientResponse.IsComplete)
		{
			clientResponse.Clear();
			ProtocolHelper.FormatErrors(new ErrorList { Error.NewError("J888", $"Timeout threshold {Timeout} exceeded.", "Reissue the command when the service is not as busy.") }, clientResponse.Messages);
			clientResponse.Messages.Add("545 Command failed.");
		}
		return clientResponse.Messages;
	}

	protected ClientSession(IIpcProtocol protocol, string id, bool logDetails)
	{
		Protocol = protocol;
		Identifier = id;
		LogDetailed = logDetails;
	}

	protected ClientSession(IIpcProtocol protocol, string id)
		: this(protocol, id, LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
	{
	}

	internal void OnServerEvent(string line)
	{
		if (ServerEvent != null)
		{
			ServerEvent(line);
		}
	}
}
