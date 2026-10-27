using System.Threading.Tasks;
using Coravel.Invocable;

namespace UpdateClientService.API.Services.Broker;

public class ReportFailedPingsJob : IReportFailedPingsJob, IInvocable
{
	private readonly IPingStatisticsService _pingStatisticsService;

	public ReportFailedPingsJob(IPingStatisticsService pingStatisticsService)
	{
		_pingStatisticsService = pingStatisticsService;
	}

	public async Task Invoke()
	{
		await _pingStatisticsService.ReportFailedPing();
	}
}
