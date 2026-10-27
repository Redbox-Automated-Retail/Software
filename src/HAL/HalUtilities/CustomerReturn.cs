using System;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class CustomerReturn : JobExecutor
{
	protected override string JobName => "return";

	internal static void RunExecutor(object o)
	{
		RunExecutor(o as HardwareService);
	}

	internal static void RunExecutor(HardwareService service)
	{
		new ConsoleLogger(enableDebug: true);
		using CustomerReturn customerReturn = new CustomerReturn(service);
		customerReturn.AddSink(delegate(HardwareJob job, DateTime eventTime, string eventMessage)
		{
			LogHelper.Instance.Log("<EventReceived> Job = {0} Msg = {1}", job.ID, eventMessage);
		});
		customerReturn.Run();
	}

	internal CustomerReturn(HardwareService s)
		: base(s)
	{
	}
}
