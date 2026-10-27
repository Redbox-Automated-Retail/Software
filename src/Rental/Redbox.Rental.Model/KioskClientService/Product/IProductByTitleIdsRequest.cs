using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Product;

public interface IProductByTitleIdsRequest
{
	List<long> TitleIds { get; }
}
