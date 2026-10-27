using System.Collections.Generic;
using KioskClientService.API.Services.App;

namespace KioskClientService.API.Services.ProductCatalog;

public class ProductData : IPersistentData
{
	public List<ProductWrapper> Products { get; set; }
}
