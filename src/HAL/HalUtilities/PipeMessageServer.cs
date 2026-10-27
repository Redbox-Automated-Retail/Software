using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Pipes;

namespace HALUtilities;

internal sealed class PipeMessageServer : AbstractPipeSever
{
	private readonly IIPCMessageFactory Factory = new IPCMessageFactory();

	protected override void OnStop()
	{
		IIPCMessage msg = Factory.Create(MessageType.Quit, MessageSeverity.High, string.Empty);
		ClientPipeChannel clientPipeChannel = new ClientPipeChannel(PipeName, "MessageServerQuit");
		if (clientPipeChannel.Connect())
		{
			Factory.Write(msg, clientPipeChannel);
			IIPCMessage iIPCMessage = Factory.Read(clientPipeChannel);
			if (iIPCMessage.Type != MessageType.Ack)
			{
				Console.WriteLine("Client: didn't get an ACK (received = {0})", iIPCMessage.Type.ToString());
			}
		}
	}

	protected override void Process(BasePipeChannel channel)
	{
		IIPCMessage iIPCMessage = Factory.Read(channel);
		LogHelper.Instance.Log("[PipeMessageServer] {0} Message received.", DateTime.Now);
		LogHelper.Instance.Log(iIPCMessage.ToString());
		IIPCMessage msg = Factory.CreateAck(iIPCMessage.UID);
		Factory.Write(msg, channel);
		channel.Dispose();
	}

	internal PipeMessageServer(string name)
		: base(name)
	{
	}
}
