using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Pricing;
using Microsoft.AspNetCore.Mvc;

namespace KioskClientService.API.Controllers;

[Route("api/pricing")]
[ApiController]
public class PricingController : ControllerBase
{
	private readonly IPricingService _pricingService;

	public PricingController(IPricingService pricingService)
	{
		_pricingService = pricingService;
	}

	[HttpPost("all/refresh")]
	[ProducesResponseType(200)]
	public async Task<ActionResult<BaseResponse>> RefreshPricing()
	{
		await _pricingService.RefreshPricing();
		return Ok();
	}

	[HttpPost("rentaldefault/refresh")]
	[ProducesResponseType(200)]
	public async Task<ActionResult<BaseResponse>> RefreshDefaultPricing()
	{
		await _pricingService.RefreshDefaultPricing();
		return Ok();
	}

	[HttpPost("rental/refresh")]
	[ProducesResponseType(200)]
	public async Task<ActionResult<BaseResponse>> RefreshRentalPricingbyRental()
	{
		await _pricingService.RefreshRentalPricingByPrice();
		return Ok();
	}

	[HttpPost("purchase/refresh")]
	[ProducesResponseType(200)]
	public async Task<ActionResult<BaseResponse>> RefreshPurchasePricingByPrice()
	{
		await _pricingService.RefreshPurchasePricingByPrice();
		return Ok();
	}

	[HttpPost("all/refreshifneeded")]
	[ProducesResponseType(200)]
	public async Task<ActionResult<BaseResponse>> RefreshAllIfNeeded()
	{
		await _pricingService.RefreshIfNeeded();
		return Ok();
	}

	[HttpPost("rentaldefault/refreshifneeded")]
	[ProducesResponseType(200)]
	public async Task<ActionResult<BaseResponse>> RefreshDefaultPricingIfNeede()
	{
		await _pricingService.RefreshDefaultPricingIfNeeded();
		return Ok();
	}

	[HttpPost("rental/refreshifneeded")]
	[ProducesResponseType(200)]
	public async Task<ActionResult<BaseResponse>> RefreshRentalPricingbyRentalIfNeede()
	{
		await _pricingService.RefreshRentalPricingByPriceIfNeeded();
		return Ok();
	}

	[HttpPost("purchase/refreshifneeded")]
	[ProducesResponseType(200)]
	public async Task<ActionResult<BaseResponse>> RefreshPurchasePricingByPriceIfNeeded()
	{
		await _pricingService.RefreshPurchasdePricingByPriceIfNeeded();
		return Ok();
	}

	[HttpGet("rentaldefault")]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(GetRentalDefaultPricingResponse), 200)]
	public async Task<ActionResult<GetRentalDefaultPricingResponse>> GetRentalDefaultPricing()
	{
		return (await _pricingService.GetRentalDefaultPricing()).ToObjectResult();
	}

	[HttpGet("rental")]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(GetRentalPricingByPriceResponse), 200)]
	public async Task<ActionResult<GetRentalPricingByPriceResponse>> GetRentalPricing()
	{
		return (await _pricingService.GetRentalPricingByPrice()).ToObjectResult();
	}

	[HttpGet("purchase")]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(GetPurchasePricingByPriceResponse), 200)]
	public async Task<ActionResult<GetPurchasePricingByPriceResponse>> GetPurchasePricing()
	{
		return (await _pricingService.GetPurchasePricingByPrice()).ToObjectResult();
	}

	[HttpDelete("all")]
	[ProducesResponseType(200)]
	public ActionResult<BaseResponse> DeleteAll()
	{
		_pricingService.DeleteAllPricing();
		return Ok();
	}

	[HttpDelete("rentaldefault")]
	[ProducesResponseType(200)]
	public ActionResult<BaseResponse> DeleteRentalDefault()
	{
		_pricingService.DeleteRentalDefaultPricing();
		return Ok();
	}

	[HttpDelete("rental")]
	[ProducesResponseType(200)]
	public ActionResult<BaseResponse> DeleteRental()
	{
		_pricingService.DeleteRentalPricingByPrice();
		return Ok();
	}

	[HttpDelete("purchase")]
	[ProducesResponseType(200)]
	public ActionResult<BaseResponse> DeletePurcahse()
	{
		_pricingService.DeletePurchasePricingByPrice();
		return Ok();
	}

	[HttpPost("product")]
	[ProducesResponseType(200)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(GetProductPricingResponse), 200)]
	public async Task<ActionResult<GetProductPricingResponse>> PostProductPricing(ProductPricingRequest request)
	{
		return (await _pricingService.GetProductPricing(request.ProductId, request.PriceLevelType, request.RentalTaxRate, request.PurchaseTaxRate, request.State)).ToObjectResult();
	}

	[HttpPost("product/pricelevel")]
	[ProducesResponseType(200)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(GetProductPricingResponse), 200)]
	public async Task<ActionResult<GetProductPricingResponse>> PostProductPriceLevelPricing(ProductPriceLevelPricingRequest request)
	{
		return (await _pricingService.GetProductDefaultPricing(request.PriceLevelType, request.RentalTaxRate, request.PurchaseTaxRate, request.State)).ToObjectResult();
	}
}
