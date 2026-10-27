using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace Redbox.IPC.Framework.Remoting;

public class RemotingClientSink : BaseChannelSinkWithProperties, IClientChannelSink, IChannelSinkBase
{
	public IClientChannelSink NextChannelSink { get; private set; }

	public RemotingClientSink(IClientChannelSink nextSink, object remoteChannelData)
	{
		NextChannelSink = nextSink;
		MethodInfo setMethod = NextChannelSink.GetType().GetProperty("Item").GetSetMethod();
		setMethod.Invoke(NextChannelSink, new object[2] { "timeout", remoteChannelData });
		IClientChannelSink nextChannelSink = NextChannelSink;
		object[] parameters = new string[2]
		{
			"connectiongroupname",
			remoteChannelData.ToString()
		};
		setMethod.Invoke(nextChannelSink, parameters);
	}

	public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
	{
		NextChannelSink.ProcessMessage(msg, requestHeaders, requestStream, out responseHeaders, out responseStream);
	}

	public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream)
	{
		NextChannelSink.AsyncProcessRequest(sinkStack, msg, headers, stream);
	}

	public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
	{
		NextChannelSink.AsyncProcessResponse(sinkStack, state, headers, stream);
	}

	public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
	{
		return null;
	}
}
