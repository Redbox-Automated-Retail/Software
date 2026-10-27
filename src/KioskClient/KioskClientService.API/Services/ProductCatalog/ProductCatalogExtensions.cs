using System;
using System.Collections.Generic;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.ProductCatalog;

public static class ProductCatalogExtensions
{
	public static IServiceCollection AddProductCatalogService(this IServiceCollection services)
	{
		return services.AddScoped<IProductCatalogService, ProductCatalogService>();
	}

	public static List<ProductWrapper> ToProductWraper(this List<Product> list)
	{
		List<ProductWrapper> list2 = new List<ProductWrapper>();
		DateTime now = DateTime.Now;
		foreach (Product item in list)
		{
			list2.Add(new ProductWrapper
			{
				DownloadedOn = now,
				Product = item
			});
		}
		return list2;
	}
}
