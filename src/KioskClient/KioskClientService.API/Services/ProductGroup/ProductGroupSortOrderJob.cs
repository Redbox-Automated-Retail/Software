using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.ProductGroup;

public class ProductGroupSortOrderJob : IInvocable
{
	private ILogger<ProductGroupSortOrderJob> _logger;

	private IProductGroupSortOrderService _productGroupSortOrderService;

	public ProductGroupSortOrderJob(ILogger<ProductGroupSortOrderJob> logger, IProductGroupSortOrderService productGroupSortOrderService)
	{
		_logger = logger;
		_productGroupSortOrderService = productGroupSortOrderService;
	}

	public async Task Invoke()
	{
		_logger.LogInfoWithSource("Invoking Sort Order Job to see if updates are needed", "Invoke");
		await _productGroupSortOrderService.RefreshIfNeeded();
		_logger.LogInfoWithSource("Finished", "Invoke");
	}
}
