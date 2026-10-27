using System.Threading.Tasks;

namespace KioskClientService.API.Services.ProductGroup;

public interface IProductGroupSortOrderService
{
	Task<ProductGroupSortOrder> GetSortOrder();

	Task RefreshSortOrder();

	Task RefreshIfNeeded();

	void DeleteSortOrder();
}
