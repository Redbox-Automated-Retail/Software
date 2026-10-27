using System.ServiceProcess;
using DeviceService.ComponentModel.Analytics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DeviceService.WebApi;

public static class WebHostServiceExtensions
{
	public static void RunAsDeviceServiceService(this IWebHost host)
	{
		ServiceBase.Run(new DeviceServiceWebHostService(host, host.Services.GetRequiredService<ILogger<DeviceServiceWebHostService>>(), host.Services.GetRequiredService<IAnalyticsService>()));
	}
}
