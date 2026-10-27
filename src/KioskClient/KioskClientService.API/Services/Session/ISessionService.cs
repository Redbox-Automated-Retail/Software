using System;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Services.Session;

public interface ISessionService
{
	ApplicationResponse CreateKioskSession(Guid sessionId);

	ApplicationResponse StartUserInteraction(SessionType sessionType);

	ApplicationResponse End();

	KioskConfigurationResponse GetCurrentKioskSessionConfiguration();
}
