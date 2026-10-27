using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Redbox.HAL.Component.Model;
using Redbox.HardwareServices.Proxy.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

internal static class Extensions
{
	public static HardwareCorrectionStatisticType ToStatType(this HardwareCorrectionStatistic stat)
	{
		return (HardwareCorrectionStatisticType)stat;
	}

	public static HardwareCorrectionStatistic ToStat(this HardwareCorrectionStatisticType type)
	{
		return (HardwareCorrectionStatistic)type;
	}

	public static ReadOnlyCollection<Redbox.HardwareServices.Proxy.ComponentModel.IHardwareCorrectionStatistic> ToStatList(this IEnumerable<Redbox.HAL.Component.Model.IHardwareCorrectionStatistic> data)
	{
		List<Redbox.HardwareServices.Proxy.ComponentModel.IHardwareCorrectionStatistic> results = new List<Redbox.HardwareServices.Proxy.ComponentModel.IHardwareCorrectionStatistic>();
		data?.ToList().ForEach(delegate(Redbox.HAL.Component.Model.IHardwareCorrectionStatistic x)
		{
			results.Add(new HardwareCorrectionStatisticAdapter(x));
		});
		return results.AsReadOnly();
	}
}
