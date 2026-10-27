using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskClientService.API.Services.Products.ProductReelsApi;

public interface IProductReelsApiClient
{
	Task<ConcurrentDictionary<long, List<long>>> MoreLikeThisBulkAsync(List<long> productGroupId);
}
