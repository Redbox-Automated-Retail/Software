using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "dump-hardware-statistics")]
internal sealed class DumpHardwareStatisticsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		HardwareCorrectionStatistic hardwareCorrectionStatistic = Enum<HardwareCorrectionStatistic>.ParseIgnoringCase(base.Context.PopTop<string>(), HardwareCorrectionStatistic.None);
		if (hardwareCorrectionStatistic == HardwareCorrectionStatistic.None)
		{
			return;
		}
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string path = Path.Combine(ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().CreateSubpath("Service"), $"{hardwareCorrectionStatistic.ToString()}CorrectionStats.txt");
		service.SafeDelete(path);
		IHardwareCorrectionStatisticService service2 = ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>();
		StreamWriter writer = new StreamWriter(path);
		try
		{
			List<IHardwareCorrectionStatistic> stats = service2.GetStats(hardwareCorrectionStatistic);
			stats.Sort((IHardwareCorrectionStatistic x, IHardwareCorrectionStatistic y) => x.CorrectionTime.CompareTo(y.CorrectionTime));
			using (new DisposeableList<IHardwareCorrectionStatistic>(stats))
			{
				writer.WriteLine("{0} Kiosk:{1}", DateTime.Now, service.KioskId);
				stats.ForEach(delegate(IHardwareCorrectionStatistic s)
				{
					writer.WriteLine("{0},{1},{2}", s.CorrectionTime.ToString(), s.ProgramName, s.CorrectionOk ? "SUCCESS" : "FAILURE");
				});
			}
		}
		finally
		{
			if (writer != null)
			{
				((IDisposable)writer).Dispose();
			}
		}
	}

	internal DumpHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
