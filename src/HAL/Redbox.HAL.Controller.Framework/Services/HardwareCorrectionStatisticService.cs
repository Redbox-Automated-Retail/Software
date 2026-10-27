using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework.Services;

internal sealed class HardwareCorrectionStatisticService : IHardwareCorrectionStatisticService
{
	private readonly ITableTypeFactory TypeFactory;

	private readonly IDataTableService DataTableService;

	private readonly IDataTable<IHardwareCorrectionStatistic> StatsTable;

	public bool Insert(HardwareCorrectionEventArgs args, IExecutionContext context)
	{
		return Insert(args.Statistic, context, args.CorrectionOk);
	}

	public bool Insert(HardwareCorrectionStatistic type, IExecutionContext context, bool correctionOk)
	{
		return Insert(type, context, correctionOk, DateTime.Now);
	}

	public bool Insert(HardwareCorrectionStatistic type, IExecutionContext context, bool correctionOk, DateTime ts)
	{
		if (!ControllerConfiguration.Instance.TrackHardwareCorrections)
		{
			return true;
		}
		IHardwareCorrectionStatistic p = TypeFactory.NewStatistic(type, context.ProgramName, correctionOk, ts);
		return StatsTable.Insert(p);
	}

	public bool RemoveAll()
	{
		try
		{
			StatsTable.DeleteAll();
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[HardwareCorrectionStatisticService] Failed to delete all correction entries.", e);
			return false;
		}
	}

	public bool RemoveAll(HardwareCorrectionStatistic stat)
	{
		try
		{
			List<IHardwareCorrectionStatistic> stats = GetStats(stat);
			using (new DisposeableList<IHardwareCorrectionStatistic>(stats))
			{
				return StatsTable.Delete(stats);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[HardwareCorrectionStatisticService] Failed to delete HardwareCorrection info.", e);
			return false;
		}
	}

	public List<IHardwareCorrectionStatistic> GetStats()
	{
		return StatsTable.LoadEntries();
	}

	public List<IHardwareCorrectionStatistic> GetStats(HardwareCorrectionStatistic key)
	{
		List<IHardwareCorrectionStatistic> list = StatsTable.LoadEntries();
		using (new DisposeableList<IHardwareCorrectionStatistic>(list))
		{
			return list.FindAll((IHardwareCorrectionStatistic each) => each.Statistic == key);
		}
	}

	internal HardwareCorrectionStatisticService(IDataTableService dts)
	{
		DataTableService = dts;
		TypeFactory = ServiceLocator.Instance.GetService<ITableTypeFactory>();
		StatsTable = DataTableService.GetTable<IHardwareCorrectionStatistic>();
		if (!StatsTable.Exists)
		{
			bool flag = StatsTable.Create();
			LogHelper.Instance.Log("[HardwareCorrectionStatisticsService] Stats table doesn't exist; create returned {0}", flag ? "SUCCESS" : "FAILURE");
		}
	}
}
