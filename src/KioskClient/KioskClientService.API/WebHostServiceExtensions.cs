using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API;

public static class WebHostServiceExtensions
{
	public static void RunAsDeviceServiceService(this IWebHost host)
	{
		using (host.Services.CreateScope())
		{
			ApplicationWebHostService service = new ApplicationWebHostService(host, host.Services.GetRequiredService<ILogger<ApplicationWebHostService>>());
			ServiceBase.Run(service);
		}
	}
}
