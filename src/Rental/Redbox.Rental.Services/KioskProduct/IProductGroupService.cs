using Redbox.Rental.Model.KioskClientService.Products;

namespace Redbox.Rental.Services.KioskProduct;

public interface IProductGroupService
{
	void StartSession();

	ProductGroupSortOrder GetSortOrder();
}
