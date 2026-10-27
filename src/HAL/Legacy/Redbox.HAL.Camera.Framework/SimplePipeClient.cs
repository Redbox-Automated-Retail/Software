using System;
using System.Collections.Generic;
using Redbox.HAL.IPC.Framework;

namespace Redbox.HAL.Camera.Framework;

internal class SimplePipeClient : AbstractPipeClientSession
{
	internal static string SnapImage()
	{
		SimplePipeClient simplePipeClient = new SimplePipeClient("CAMERATUNER", "NONE");
		try
		{
			((AbstractPipeClientSession)simplePipeClient).Connect();
			((AbstractPipeClientSession)simplePipeClient).Write("SNAP");
			List<string> list = ((AbstractPipeClientSession)simplePipeClient).ConsumeMessages();
			if (list.Count == 0)
			{
				return "NONE";
			}
			string text = list[0];
			return text.TrimEnd();
		}
		finally
		{
			((IDisposable)simplePipeClient)?.Dispose();
		}
	}

	public List<string> ExecuteCommand(string command)
	{
		((AbstractPipeClientSession)this).Write(command);
		return ((AbstractPipeClientSession)this).ConsumeMessages();
	}

	~SimplePipeClient()
	{
		try
		{
			((AbstractPipeClientSession)this).DisposeInner(false);
		}
		finally
		{
			((object)this).Finalize();
		}
	}

	protected override List<string> ProcessRawMessage()
	{
		List<string> list = new List<string>();
		list.Add(((AbstractPipeClientSession)this).ReadBuilder.ToString());
		return list;
	}

	private SimplePipeClient(string pipeName, string id)
		: base(id, pipeName)
	{
	}
}
