using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-counters-job", Operand = "RESET-COUNTERS-JOB")]
internal sealed class ResetCountersJob : NativeJobAdapter
{
	private DateTime? m_lastReset;

	private readonly IPersistentMap DataMap;

	private DateTime? LastCounterReset
	{
		get
		{
			if (m_lastReset.HasValue)
			{
				return m_lastReset.Value;
			}
			string value = DataMap.GetValue("LastCounterReset", "NONE");
			if (value == "NONE")
			{
				return m_lastReset = null;
			}
			try
			{
				m_lastReset = DateTime.Parse(value);
				return m_lastReset;
			}
			catch
			{
				return m_lastReset = null;
			}
		}
		set
		{
			m_lastReset = value;
			DataMap.SetValue("LastCounterReset", (!m_lastReset.HasValue) ? "NONE" : m_lastReset.Value.ToString());
		}
	}

	protected override void ExecuteInner()
	{
		DateTime now = DateTime.Now;
		if ((!LastCounterReset.HasValue || LastCounterReset.Value.Date != now.Date) && DateRange.NowIsBetweenHours(0, 1))
		{
			LastCounterReset = now;
			LogHelper.Instance.Log("Watchdog resetting counters.");
			IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
			service.Reset("EmptyOrStuckCount");
			if (now.DayOfWeek == DayOfWeek.Sunday)
			{
				LogHelper.Instance.Log("Reset timeout counters.", LogEntryType.Info);
				service.ResetWeekly();
			}
		}
		IKioskFunctionCheckService service2 = ServiceLocator.Instance.GetService<IKioskFunctionCheckService>();
		LogHelper.Instance.Log("[Watchdog] Kiosk function check cleaned {0} entries", service2.CleanOldEntries());
	}

	internal ResetCountersJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
		DataMap = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
	}
}
