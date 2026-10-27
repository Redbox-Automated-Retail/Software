using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Redbox.IPC.Framework.Remoting;

public class RemotingClientProxy : RealProxy
{
	private string Url { get; set; }

	private IMessageSink SinkChain { get; set; }

	public RemotingClientProxy(Type type, string url, int timeout)
		: base(type)
	{
		Url = url;
		using (IEnumerator<IChannel> enumerator = ChannelServices.RegisteredChannels.Where((IChannel channel) => channel.ChannelName == "ipc_tcp" && channel is IChannelSender).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				IChannel current = enumerator.Current;
				SinkChain = ((IChannelSender)current).CreateMessageSink(Url, timeout, out var _);
			}
		}
		if (SinkChain == null)
		{
			throw new ApplicationException("TcpChannel is not registered");
		}
	}

	public override IMessage Invoke(IMessage msg)
	{
		msg.Properties["__Uri"] = Url;
		return SinkChain.SyncProcessMessage(msg);
	}
}
