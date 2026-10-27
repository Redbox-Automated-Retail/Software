using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API;

public class ApplicationWebHostService : WebHostService
{
	private ILogger<ApplicationWebHostService> _logger;

	public ApplicationWebHostService(IWebHost host, ILogger<ApplicationWebHostService> logger)
		: base(host)
	{
		_logger = logger;
	}

	protected override void OnStarting(string[] args)
	{
		_logger?.LogInfoWithSource("WebHost: OnStarting", "OnStarting", "/sln/src/UpdateClientService.API/ApplicationWebHostService.cs");
		base.OnStarting(args);
	}

	protected override void OnStarted()
	{
		_logger?.LogInfoWithSource("WebHost: OnStarted", "OnStarted", "/sln/src/UpdateClientService.API/ApplicationWebHostService.cs");
		base.OnStarted();
	}

	protected override void OnStopping()
	{
		_logger?.LogInfoWithSource("WebHost: OnStopping", "OnStopping", "/sln/src/UpdateClientService.API/ApplicationWebHostService.cs");
		base.OnStopping();
	}

	protected override void OnStopped()
	{
		_logger?.LogInfoWithSource("WebHost: OnStopped", "OnStopped", "/sln/src/UpdateClientService.API/ApplicationWebHostService.cs");
		base.OnStopped();
	}
}
