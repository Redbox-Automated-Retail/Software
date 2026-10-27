using System.Threading.Tasks;
using Coravel.Invocable;
using KioskClientService.API.Services.Campaigns;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Campaign;

public class CampaignJob : IInvocable
{
	private ILogger<CampaignJob> _logger;

	private readonly ICampaignService _campaignService;

	private readonly IOptionsSnapshotKioskConfiguration _configuration;

	public CampaignJob(ILogger<CampaignJob> logger, ICampaignService campaignService, IOptionsSnapshotKioskConfiguration configuration)
	{
		_logger = logger;
		_campaignService = campaignService;
		_configuration = configuration;
	}

	public async Task Invoke()
	{
		if (!_configuration.Campaigns.JobActive)
		{
			_logger.LogDebugWithSource("Campaign job is not active", "Invoke");
		}
		else
		{
			await _campaignService.RefreshIfNeeded();
		}
	}
}
