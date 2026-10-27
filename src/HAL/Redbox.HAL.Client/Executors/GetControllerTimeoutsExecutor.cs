using System;
using System.IO;

namespace Redbox.HAL.Client.Executors;

public sealed class GetControllerTimeoutsExecutor : JobExecutor
{
	protected override string JobName => "get-controller-timeouts";

	protected override string Label => "Tester Counters";

	public void Log(StreamWriter log)
	{
		log.WriteLine("Counter sample on {0} {1}", DateTime.Now.ToShortTimeString(), DateTime.Now.ToShortDateString());
		foreach (ProgramResult result in base.Results)
		{
			log.WriteLine(" {0}: {1}", result.Code, result.Message);
		}
	}

	public GetControllerTimeoutsExecutor(HardwareService service)
		: base(service)
	{
	}
}
