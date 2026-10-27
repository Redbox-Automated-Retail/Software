using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework.Pipes;

namespace HALUtilities;

internal abstract class AbstractPipeSever
{
	protected readonly string PipeName;

	protected volatile bool IsAlive;

	protected abstract void OnStop();

	protected abstract void Process(BasePipeChannel channel);

	protected virtual void Run()
	{
		LogHelper.Instance.Log("Abstract[PipeServer] {0} Server running on {1}", DateTime.Now, PipeName);
		IsAlive = true;
		using (NamedPipeServer namedPipeServer = NamedPipeServer.Create(PipeName))
		{
			while (IsAlive)
			{
				BasePipeChannel channel = namedPipeServer.WaitForClientConnect();
				if (channel != null)
				{
					ThreadPool.QueueUserWorkItem(delegate
					{
						Process(channel);
					});
				}
			}
		}
		LogHelper.Instance.Log("[AbstractPipeServer] {0} Exiting.", DateTime.Now);
	}

	protected AbstractPipeSever(string pipeName)
	{
		PipeName = pipeName;
	}

	internal static void RunServer(string cmdLineArg, PipeServers serverType, bool debug)
	{
		new ConsoleLogger(debug);
		string optionVal = CommandLineOption.GetOptionVal(cmdLineArg, "HAL7001");
		AbstractPipeSever abstractPipeSever = null;
		switch (serverType)
		{
		case PipeServers.Basic:
			abstractPipeSever = new BasicPipeServer(optionVal);
			break;
		case PipeServers.Message:
			abstractPipeSever = new PipeMessageServer(optionVal);
			break;
		}
		if (abstractPipeSever != null)
		{
			Thread thread = new Thread(abstractPipeSever.Run);
			thread.Start();
			Console.WriteLine("Press any key to halt.");
			Console.ReadLine();
			abstractPipeSever.Stop();
			thread.Join(5000);
			Console.WriteLine("Program finished.");
		}
		Environment.Exit(0);
	}

	private void Stop()
	{
		IsAlive = false;
		OnStop();
	}
}
