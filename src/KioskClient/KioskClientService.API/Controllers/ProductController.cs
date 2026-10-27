using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProductCatalog;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.AspNetCore.Mvc;

namespace KioskClientService.API.Controllers;

[Route("api/product")]
[ApiController]
public class ProductController : ControllerBase
{
	private readonly IProductCatalogService _service;

	public ProductController(IProductCatalogService service)
	{
		_service = service;
	}

	[HttpPost("bytitleids")]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(ProductByTitleIdsResponse), 200)]
	public async Task<ActionResult<ProductByTitleIdsResponse>> ProductByTitleIds(ProductByTitleIdsRequest request)
	{
		return (await _service.ProductsByTitleIds(request)).ToObjectResult();
	}

	[HttpPost("{titleid}/remove")]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(ApiBaseResponse), 200)]
	public async Task<ActionResult<ProductByTitleIdsResponse>> RemoveTitleFromCache(long titleid)
	{
		return (await _service.RemoveTitleCache(titleid)).ToObjectResult();
	}
}
