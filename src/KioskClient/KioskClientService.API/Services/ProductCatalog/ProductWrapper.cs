using System;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.ProductCatalog;

public class ProductWrapper
{
	public DateTime DownloadedOn { get; set; }

	public Product Product { get; set; }
}
