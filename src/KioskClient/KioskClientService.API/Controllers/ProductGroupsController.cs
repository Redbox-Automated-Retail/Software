using System.Linq;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProductGroup;
using KioskClientService.API.Services.Products.MoreLikeThis;
using Microsoft.AspNetCore.Mvc;

namespace KioskClientService.API.Controllers;

[Route("api/productgroups")]
[ApiController]
public class ProductGroupsController : ControllerBase
{
	private readonly IMoreLikeThisService _moreLikeThisService;

	private readonly IProductGroupSortOrderService _productGroupOrderService;

	public ProductGroupsController(IMoreLikeThisService moreLikeThisService, IProductGroupSortOrderService productGroupOrderService)
	{
		_moreLikeThisService = moreLikeThisService;
		_productGroupOrderService = productGroupOrderService;
	}

	[HttpGet("{id}/morelikethis")]
	public ActionResult<GetMoreLikeThisResponse> MoreLikeThis(long id)
	{
		GetMoreLikeThisResponse mLT = _moreLikeThisService.GetMLT(id);
		if (mLT.Errors.Any())
		{
			return StatusCode(500, mLT);
		}
		if (!mLT.MoreLikeThis.Any())
		{
			return NotFound(mLT);
		}
		return mLT;
	}

	[HttpPost("morelikethis/refresh")]
	public async Task<ActionResult<BaseResponse>> CacheMoreLikeThisTitle([FromBody] ScrubbedProductGroupIds productGroupIds)
	{
		Task.Run(() => _moreLikeThisService.RefreshMoreLikeThisAsync(productGroupIds));
		return Ok();
	}

	[HttpGet("sortorder")]
	[ProducesResponseType(404)]
	[ProducesResponseType(typeof(GetSortOrderResponse), 200)]
	public async Task<ActionResult<GetSortOrderResponse>> GetProductGroupSortOrder()
	{
		ProductGroupSortOrder productGroupSortOrder = await _productGroupOrderService.GetSortOrder();
		if (productGroupSortOrder == null || productGroupSortOrder.SortOrders.Count == 0)
		{
			return new NotFoundResult();
		}
		return new OkObjectResult(new GetSortOrderResponse
		{
			ProductGroupSortOrder = productGroupSortOrder
		});
	}

	[HttpPost("sortorder/refresh")]
	[ProducesResponseType(200)]
	public async Task<ActionResult<BaseResponse>> RefreshSortOrder()
	{
		await _productGroupOrderService.RefreshSortOrder();
		return Ok();
	}

	[HttpPost("sortorder/refreshifneeded")]
	[ProducesResponseType(200)]
	public async Task<ActionResult<BaseResponse>> RefreshSortOrderIfNeeded()
	{
		await _productGroupOrderService.RefreshIfNeeded();
		return Ok();
	}

	[HttpDelete("sortorder")]
	[ProducesResponseType(200)]
	public ActionResult<BaseResponse> DeleteSortOrder()
	{
		_productGroupOrderService.DeleteSortOrder();
		return Ok();
	}
}
