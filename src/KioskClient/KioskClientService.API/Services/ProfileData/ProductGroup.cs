using System.Collections.Generic;

namespace KioskClientService.API.Services.ProfileData;

public class ProductGroup
{
	public long ProductGroupId { get; set; }

	public List<ProductFormat> ProductFormats { get; set; } = new List<ProductFormat>();
}
