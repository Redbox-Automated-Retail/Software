using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redbox.NetCore.Middleware.Abstractions;

public interface ICommonMetrics
{
	Task CaptureMetrics(Dictionary<string, string> metricTags, long elapsedMilliseconds);
}
