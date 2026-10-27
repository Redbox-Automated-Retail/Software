using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UpdateClientService.API;

public static class WebHostServiceExtensions
{
	public static void RunAsWindowsService(this IWebHost host)
	{
		using (host.Services.CreateScope())
		{
			ApplicationWebHostService service = new ApplicationWebHostService(host, host.Services.GetRequiredService<ILogger<ApplicationWebHostService>>());
			ServiceBase.Run(service);
		}
	}
}
