using System.Collections.Generic;
using Redbox.Rental.Model.KioskClientService.Product;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Product;

public class ProductByTitleIdsResponse : ApiBaseResponse, IProductByTitleIdsResponse
{
	public List<Redbox.Rental.Model.KioskClientService.Product.Product> Products { get; set; } = new List<Redbox.Rental.Model.KioskClientService.Product.Product>();
}
