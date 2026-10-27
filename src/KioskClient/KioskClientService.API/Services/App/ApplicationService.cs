using System.Threading.Tasks;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.App;

public class ApplicationService : IApplicationService
{
	private ILogger<ApplicationService> _logger;

	private IAdService _adService;

	public ApplicationService(ILogger<ApplicationService> logger, IAdService adService)
	{
		_logger = logger;
		_adService = adService;
	}

	public async Task<ApplicationResponse> Start()
	{
		await (_adService?.StartApplication());
		return new ApplicationResponse();
	}

	public async Task<ApplicationResponse> End()
	{
		await _adService.EndApplication();
		return new ApplicationResponse();
	}

	public async Task<ViewShowResponse> ViewChangedEvent(ViewStateChangedEvent e)
	{
		if (e.ToView != null)
		{
			await _adService.GetAds(e.ToView, isPreload: true);
		}
		return new ViewShowResponse();
	}
}
