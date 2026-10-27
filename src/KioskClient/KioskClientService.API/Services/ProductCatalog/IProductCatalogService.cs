using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.ProductCatalog;

public interface IProductCatalogService
{
	Task<ApiBaseResponse> RemoveTitleCache(long titleId);

	Task<ProductByTitleIdsResponse> ProductsByTitleIds(ProductByTitleIdsRequest request);
}
