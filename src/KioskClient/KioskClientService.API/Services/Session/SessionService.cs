using System;
using System.Net;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Session;

public class SessionService : ISessionService
{
	private static IKioskSession _kioskSession;

	private readonly ILogger<SessionService> _logger;

	private readonly IAdService _adService;

	private readonly IKioskConfiguration _kioskConfiguration;

	public SessionService(ILogger<SessionService> logger, IAdService adService, IOptionsSnapshotKioskConfiguration kioskConfiguration)
	{
		_logger = logger;
		_adService = adService;
		_kioskConfiguration = kioskConfiguration;
	}

	public KioskConfigurationResponse GetCurrentKioskSessionConfiguration()
	{
		KioskConfigurationResponse kioskConfigurationResponse = new KioskConfigurationResponse
		{
			KioskConfiguration = _kioskSession?.Configuration
		};
		if (kioskConfigurationResponse.KioskConfiguration == null)
		{
			kioskConfigurationResponse.StatusCode = HttpStatusCode.NotFound;
		}
		return kioskConfigurationResponse;
	}

	public ApplicationResponse CreateKioskSession(Guid kioskSessionId)
	{
		ApplicationResponse result = new ApplicationResponse();
		_kioskSession = new KioskSession
		{
			KioskSessionId = kioskSessionId
		};
		_logger?.LogInfoWithSource($"Creating KioskSession with id {_kioskSession?.KioskSessionId} with KioskConfiguration version {_kioskConfiguration?.ConfigurationVersion}", "CreateKioskSession");
		_kioskSession.Configuration = _kioskConfiguration;
		return result;
	}

	public ApplicationResponse StartUserInteraction(SessionType sessionType)
	{
		ApplicationResponse applicationResponse = new ApplicationResponse();
		if (_kioskSession != null)
		{
			_kioskSession.SessionType = sessionType;
			_kioskSession.UserInteractionStarted = DateTime.Now;
			_logger?.LogInfoWithSource($"Starting User interaction for KioskSession id {_kioskSession?.KioskSessionId} with SessionType:{sessionType}", "StartUserInteraction");
		}
		else
		{
			_logger?.LogErrorWithSource("KioskSession has not been created", "StartUserInteraction");
			applicationResponse.StatusCode = 500;
		}
		_adService.StartSession();
		return applicationResponse;
	}

	public ApplicationResponse End()
	{
		_logger?.LogInfoWithSource($"Ending KioskSession with id {_kioskSession?.KioskSessionId}", "End");
		_adService.EndSession();
		return new ApplicationResponse();
	}
}
