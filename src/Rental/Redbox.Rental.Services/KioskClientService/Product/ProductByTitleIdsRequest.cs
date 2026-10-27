using System.Collections.Generic;
using Redbox.Rental.Model.KioskClientService.Product;

namespace Redbox.Rental.Services.KioskClientService.Product;

public class ProductByTitleIdsRequest : IProductByTitleIdsRequest
{
	public List<long> TitleIds { get; set; } = new List<long>();
}
