using System.Threading.Tasks;
using Coravel.Invocable;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Planograms;

public class SyncPlanogramsJob : IInvocable
{
	private ILogger<SyncPlanogramsJob> _logger;

	private readonly IPlanogramService _planogramService;

	private readonly IKioskConfiguration _kioskConfiguration;

	public SyncPlanogramsJob(ILogger<SyncPlanogramsJob> logger, IPlanogramService planogramService, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration)
	{
		_logger = logger;
		_planogramService = planogramService;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
	}

	public async Task Invoke()
	{
		if (!_kioskConfiguration.KioskClientDefault.PlanogramSyncEnabled)
		{
			_logger.LogInfoWithSource("Planogram sync is not enabled", "Invoke");
			return;
		}
		_logger.LogInfoWithSource("Invoking sync planograms Job to see if updates are needed", "Invoke");
		await _planogramService.RefreshIfNeeded();
		_logger.LogInfoWithSource("Finished", "Invoke");
	}
}
