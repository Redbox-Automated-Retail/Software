using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Product;

public interface IProductByTitleIdsResponse
{
	List<Product> Products { get; }
}
