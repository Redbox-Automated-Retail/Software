using System.Collections.Generic;
using KioskClientService.API.App;
using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.ProxyApi;

public static class ProxyExtensions
{
	public static IServiceCollection AddProxyApiService(this IServiceCollection services)
	{
		return services.AddScoped<IProxyApi, ProxyApi>();
	}

	public static List<Header> AddCompression(this List<Header> headers)
	{
		if (headers == null)
		{
			headers = new List<Header>();
		}
		headers.Add(new Header("Accept-Encoding", "gzip"));
		return headers;
	}
}
