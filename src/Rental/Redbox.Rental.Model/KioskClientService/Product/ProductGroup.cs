using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Product;

public class ProductGroup
{
	public long ProductGroupId { get; set; }

	public Dictionary<string, long> ProductIds { get; set; } = new Dictionary<string, long>();
}
