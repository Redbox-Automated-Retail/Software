using System;
using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Services.Session;

public interface IKioskSession
{
	Guid KioskSessionId { get; set; }

	DateTime Created { get; set; }

	DateTime UserInteractionStarted { get; set; }

	IKioskConfiguration Configuration { get; set; }

	SessionType SessionType { get; set; }
}
