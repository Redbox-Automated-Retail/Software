using System;
using System.Collections.Generic;
using System.Linq;

namespace UpdateClientService.API.Services.Broker;

public class PingStatistics : List<PingStatistic>, IPersistentData
{
	public PingStatistic GetLatest()
	{
		return this.FirstOrDefault((PingStatistic x) => x.StartInterval == this.Max((PingStatistic y) => y.StartInterval));
	}

	public PingStatistic GetLastSuccessful()
	{
		PingStatistic pingStatistic = null;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			PingStatistic current = enumerator.Current;
			if (current.PingSuccess && (pingStatistic == null || current.EndIntervalUTC > pingStatistic.EndIntervalUTC))
			{
				pingStatistic = current;
			}
		}
		return pingStatistic;
	}

	public void Cleanup()
	{
		List<PingStatistic> list = this.Where((PingStatistic x) => x.StartInterval.Date < DateTime.Today.AddDays(-30.0)).ToList();
		list.ForEach(delegate(PingStatistic x)
		{
			Remove(x);
		});
	}
}
