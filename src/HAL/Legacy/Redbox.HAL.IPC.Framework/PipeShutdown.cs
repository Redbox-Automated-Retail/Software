using System.Collections.Generic;

namespace Redbox.HAL.IPC.Framework;

public class PipeShutdown : AbstractPipeClientSession
{
	public static string ShutdownPipe(string pipeName)
	{
		using PipeShutdown pipeShutdown = new PipeShutdown(pipeName, "NONE");
		pipeShutdown.Connect();
		pipeShutdown.Write("QUIT");
		return "OK";
	}

	public List<string> ExecuteCommand(string command)
	{
		Write(command);
		return ConsumeMessages();
	}

	~PipeShutdown()
	{
		DisposeInner(disposing: false);
	}

	protected override List<string> ProcessRawMessage()
	{
		List<string> list = new List<string>();
		list.Add(base.ReadBuilder.ToString());
		return list;
	}

	private PipeShutdown(string pipeName, string id)
		: base(id, pipeName)
	{
	}
}
