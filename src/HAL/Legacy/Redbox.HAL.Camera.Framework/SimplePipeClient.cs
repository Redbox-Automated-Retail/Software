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
			simplePipeClient.Connect();
			simplePipeClient.Write("SNAP");
			List<string> list = simplePipeClient.ConsumeMessages();
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
		Write(command);
		return ConsumeMessages();
	}

	~SimplePipeClient()
	{
		DisposeInner(false);
	}

	protected override List<string> ProcessRawMessage()
	{
		List<string> list = new List<string>();
		list.Add(ReadBuilder.ToString());
		return list;
	}

	private SimplePipeClient(string pipeName, string id)
		: base(id, pipeName)
	{
	}
}
