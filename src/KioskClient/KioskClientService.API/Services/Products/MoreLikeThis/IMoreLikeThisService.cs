using System.Threading.Tasks;
using KioskClientService.API.Services.ProductGroup;

namespace KioskClientService.API.Services.Products.MoreLikeThis;

public interface IMoreLikeThisService
{
	GetMoreLikeThisResponse GetMLT(long productGroupId);

	Task RefreshMoreLikeThisAsync(ScrubbedProductGroupIds productGroupIds);
}
