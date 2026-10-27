using System;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Pipes;

namespace HALUtilities;

internal sealed class BasicPipeServer : AbstractPipeSever
{
	private readonly byte[] AckMessage;

	protected override void OnStop()
	{
		using ClientPipeChannel clientPipeChannel = new ClientPipeChannel(PipeName, "basic shutdown");
		if (clientPipeChannel.Connect())
		{
			clientPipeChannel.Write(Encoding.ASCII.GetBytes("QUIT"));
			clientPipeChannel.Read();
		}
	}

	protected override void Process(BasePipeChannel channel)
	{
		LogHelper.Instance.Log("[BasicPipeServer] {0} Session connected.", DateTime.Now);
		channel.Write(Encoding.ASCII.GetBytes("Hello"));
		byte[] array = channel.Read();
		LogHelper.Instance.Log("[BasicPipeServer] Bytes received: {0}", array.Length);
		channel.Write(AckMessage);
		channel.Dispose();
	}

	internal BasicPipeServer(string name)
		: base(name)
	{
		AckMessage = Encoding.ASCII.GetBytes("OK");
	}
}
