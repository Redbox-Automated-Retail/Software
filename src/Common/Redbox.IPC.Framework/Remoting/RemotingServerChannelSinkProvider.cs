using System.Runtime.Remoting.Channels;

namespace Redbox.IPC.Framework.Remoting;

public class RemotingServerChannelSinkProvider : IServerChannelSinkProvider
{
	public IServerChannelSinkProvider Next { get; set; }

	private RemotingServiceHost RemotingHost { get; set; }

	public RemotingServerChannelSinkProvider(RemotingServiceHost host, IServerFormatterSinkProvider next)
	{
		Next = next;
		RemotingHost = host;
	}

	public void GetChannelData(IChannelDataStore channelData)
	{
	}

	public IServerChannelSink CreateSink(IChannelReceiver channel)
	{
		IServerChannelSink nextSink = Next.CreateSink(channel);
		return new RemotingServerSink(RemotingHost, nextSink);
	}
}
