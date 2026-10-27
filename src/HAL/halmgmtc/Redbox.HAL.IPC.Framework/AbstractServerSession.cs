using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

public abstract class AbstractServerSession : ISession, IMessageSink
{
	protected readonly IIpcServiceHost ServiceHost;

	protected readonly StringBuilder ReadBuilder = new StringBuilder();

	protected readonly string Identifier;

	private readonly string Welcome;

	private readonly List<string> MessageSinkQueue = new List<string>();

	private readonly object m_syncLock = new object();

	public IDictionary<string, object> Properties { get; private set; }

	public bool LogDetailedMessages { get; private set; }

	protected abstract IIPCChannel Channel { get; }

	public event EventHandler Disconnect;

	public void Start()
	{
		LogHelper.Instance.Log("[AbstractServerSession-{0}] Start", Identifier);
		RedboxChannelDecorator.Write(Channel, Welcome);
		CommLoop();
	}

	public bool Send(string message)
	{
		string text = $"[MSG] {message}";
		if (LogDetailedMessages)
		{
			LogHelper.Instance.Log(text);
		}
		lock (m_syncLock)
		{
			MessageSinkQueue.Add(text);
		}
		return true;
	}

	protected AbstractServerSession(IIpcServiceHost host, string id)
		: this(host, id, LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
	{
	}

	protected AbstractServerSession(IIpcServiceHost host, string id, bool logDetails)
	{
		ServiceHost = host;
		LogDetailedMessages = logDetails;
		Properties = new Dictionary<string, object>();
		Identifier = id;
		Welcome = $"Welcome! {host.HostInfo.Product}, Version {host.HostInfo.Version}, {host.HostInfo.Copyright}";
	}

	protected abstract bool OnSessionEnd();

	protected void CommLoop()
	{
		while (true)
		{
			using ServerResponse serverResponse = new ServerResponse();
			Channel.Read(serverResponse);
			if (!serverResponse.IsComplete)
			{
				QuitSession(sendBye: false);
				break;
			}
			if (LogDetailedMessages)
			{
				LogHelper.Instance.Log("[AbstractServerSession-{0}] Received command = '{1}'", Identifier, serverResponse.Command);
			}
			if (serverResponse.Command.StartsWith("quit", StringComparison.CurrentCultureIgnoreCase))
			{
				QuitSession(sendBye: true);
				break;
			}
			CommandResult commandResult = CommandService.Instance.Execute(this, serverResponse.Command);
			ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
			FlushSinkQueue(commandResult.Messages);
			string text = commandResult.ToString();
			if (LogDetailedMessages)
			{
				LogHelper.Instance.Log("[AbstractServerSession - {0}]  Response = {1}", Identifier, text);
			}
			RedboxChannelDecorator.Write(Channel, text);
		}
	}

	protected bool QuitSession(bool sendBye)
	{
		if (sendBye)
		{
			RedboxChannelDecorator.Write(Channel, "Goodbye!");
		}
		if (Disconnect != null)
		{
			Disconnect(this, EventArgs.Empty);
		}
		bool result = false;
		try
		{
			OnSessionEnd();
			if (LogDetailedMessages)
			{
				LogHelper.Instance.Log("[AbstractServerSession - {0}] Session end.", Identifier);
			}
			result = true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Caught exception in QuitSession", e);
		}
		try
		{
			ServiceHost.Unregister(this);
			return result;
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("QuitSession: unable to unregister session.", e2);
			return false;
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
}
