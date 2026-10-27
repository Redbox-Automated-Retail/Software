using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Redbox.Core;

namespace Redbox.IPC.Framework.Remoting;

public class RemotingClientSession : ClientSession
{
	private static readonly object m_lockObject = new object();

	protected override Stream Stream
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	private string Url { get; set; }

	public RemotingClientSession(IPCProtocol protocol, int timeout)
		: base(protocol)
	{
		base.Timeout = timeout;
		Url = $"tcp://{base.Protocol.Host}:{base.Protocol.Port}/IPCFramework/CommandService.rem";
	}

	public override bool Connect()
	{
		base.IsConnected = true;
		return true;
	}

	public override void ConnectThrowOnError()
	{
		base.IsConnected = true;
	}

	public override void Close()
	{
	}

	public override void Dispose()
	{
		if (base.OwningPool != null)
		{
			base.OwningPool.ReturnSession(this);
		}
		base.Dispose();
	}

	public override List<string> ExecuteCommand(string command)
	{
		if (IsRegistrationRequired())
		{
			RegisterChannel();
		}
		string text = ((IRemotingHost)new RemotingClientProxy(typeof(IRemotingHost), Url, base.Timeout).GetTransparentProxy()).ExecuteCommand(command);
		if (string.IsNullOrEmpty(text))
		{
			return new List<string>();
		}
		return new List<string>(text.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
	}

	protected internal override bool IsConnectionAvailable()
	{
		return true;
	}

	protected internal override void CustomClose()
	{
	}

	protected override int GetAvailableData()
	{
		throw new NotImplementedException();
	}

	protected override bool CanRead()
	{
		throw new NotImplementedException();
	}

	private void RegisterChannel()
	{
		lock (m_lockObject)
		{
			if (IsRegistrationRequired())
			{
				if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["CallRemotingConfigure"]))
				{
					RemotingConfiguration.Configure(null, base.Protocol.IsSecure);
				}
				if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CustomErrorsMode"]))
				{
					RemotingConfiguration.CustomErrorsMode = Enum<CustomErrorsModes>.ParseIgnoringCase(ConfigurationManager.AppSettings["CustomErrorsMode"], CustomErrorsModes.RemoteOnly);
				}
				LogHelper.Instance.Log("CustomErrors enabled = {0}, {1}", RemotingConfiguration.CustomErrorsEnabled(isLocalRequest: false), RemotingConfiguration.CustomErrorsMode);
				BinaryClientFormatterSinkProvider binaryClientFormatterSinkProvider = new BinaryClientFormatterSinkProvider();
				RemotingClientChannelSinkProvider next = new RemotingClientChannelSinkProvider(binaryClientFormatterSinkProvider.Next);
				binaryClientFormatterSinkProvider.Next = next;
				ChannelServices.RegisterChannel(new TcpClientChannel(GetProperties(), binaryClientFormatterSinkProvider), base.Protocol.IsSecure);
			}
		}
	}

	private static bool IsRegistrationRequired()
	{
		return ChannelServices.GetChannel("ipc_tcp") == null;
	}

	private ListDictionary GetProperties()
	{
		return new ListDictionary
		{
			{ "port", "0" },
			{ "timeout", base.Timeout },
			{ "name", "ipc_tcp" }
		};
	}
}
