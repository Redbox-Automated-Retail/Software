using System;
using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Services.Session;

public class KioskSession : IKioskSession
{
	public Guid KioskSessionId { get; set; }

	public DateTime Created { get; set; } = DateTime.Now;

	public DateTime UserInteractionStarted { get; set; }

	public IKioskConfiguration Configuration { get; set; }

	public SessionType SessionType { get; set; }
}
