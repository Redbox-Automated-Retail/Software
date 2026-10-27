using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Products;

namespace Redbox.Rental.Services.KioskClientService.Products;

public class GetSortOrderResponse : BaseResponse
{
	public ProductGroupSortOrder ProductGroupSortOrder { get; set; }
}
