using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.HttpQueue;
using KioskClientService.API.Services.Transaction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionController : ControllerBase
{
	private readonly ILogger<TransactionController> _logger;

	private readonly ITransactionService _transactionService;

	private readonly IHttpQueueService _httpQueueService;

	public TransactionController(ILogger<TransactionController> logger, ITransactionService transactionService, IHttpQueueService httpQueueService)
	{
		_logger = logger;
		_transactionService = transactionService;
		_httpQueueService = httpQueueService;
	}

	[Route("Authorize")]
	[HttpPost]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(ReturnResponse), 200)]
	public async Task<ActionResult<AuthorizeResponse>> Authorize(AuthorizeRequest request)
	{
		return new OkObjectResult(await _transactionService.Authorize(request));
	}

	[Route("GiftCardAuthorize")]
	[HttpPost]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(ReturnResponse), 200)]
	public async Task<ActionResult<GCAuthResponse>> GiftCardAuthorize(GCAuthRequest request)
	{
		return new OkObjectResult(await _transactionService.GiftCardAuthorize(request));
	}

	[Route("PickupAuthorize")]
	[HttpPost]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(ReturnResponse), 200)]
	public async Task<ActionResult<AuthorizeResponse>> PickupAuthorize(PickupAuthorizeRequest request)
	{
		return new OkObjectResult(await _transactionService.PickupAuthorize(request));
	}

	[Route("ReservationAuthorize")]
	[HttpPost]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(ReturnResponse), 200)]
	public async Task<ActionResult<AuthorizeResponse>> ReservationAuthorize(ResvAuthRequest request)
	{
		return new OkObjectResult(await _transactionService.ReservationAuthorize(request));
	}

	[Route("CancelReservation")]
	[HttpPost]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(ReturnResponse), 200)]
	public async Task<ActionResult<BaseResponse>> CancelReservation(CancelReservationRequest request)
	{
		APIResponse<CancelReservationResponse> aPIResponse = await _transactionService.CancelReservation(request);
		aPIResponse.CheckAndReplay(request.MessageId, base.ControllerContext.HttpContext.Request, _httpQueueService);
		if (aPIResponse.Response == null)
		{
			return new ObjectResult(null)
			{
				StatusCode = (int)aPIResponse.StatusCode
			};
		}
		return aPIResponse.Response.ToObjectResult((int)aPIResponse.StatusCode);
	}

	[Route("ExpireReservation")]
	[HttpPost]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(ReturnResponse), 200)]
	public async Task<ActionResult<BaseResponse>> ExpireReservation(ExpireReservationRequest request)
	{
		APIResponse<ExpireReservationResponse> aPIResponse = await _transactionService.ExpireReservation(request);
		aPIResponse.CheckAndReplay(request.MessageId, base.ControllerContext.HttpContext.Request, _httpQueueService);
		if (aPIResponse.Response == null)
		{
			return new ObjectResult(null)
			{
				StatusCode = (int)aPIResponse.StatusCode
			};
		}
		return aPIResponse.Response.ToObjectResult((int)aPIResponse.StatusCode);
	}

	[Route("Reconcile")]
	[HttpPost]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(ReconcileResponse), 200)]
	public async Task<ActionResult<BaseResponse>> Reconcile(ReconcileRequest request)
	{
		APIResponse<ReconcileResponse> aPIResponse = await _transactionService.Reconcile(request);
		aPIResponse.CheckAndReplay(request.MessageId, base.ControllerContext.HttpContext.Request, _httpQueueService);
		if (aPIResponse.Response == null)
		{
			return new ObjectResult(null)
			{
				StatusCode = (int)aPIResponse.StatusCode
			};
		}
		return aPIResponse.Response.ToObjectResult((int)aPIResponse.StatusCode);
	}

	[Route("ReservationPickup")]
	[HttpPost]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(ReconcileResponse), 200)]
	public async Task<ActionResult<BaseResponse>> ReservationPickup(ReservationPickupRequest request)
	{
		APIResponse<ReconcileResponse> aPIResponse = await _transactionService.ReservationPickup(request);
		aPIResponse.CheckAndReplay(request.MessageId, base.ControllerContext.HttpContext.Request, _httpQueueService);
		if (aPIResponse.Response == null)
		{
			return new ObjectResult(null)
			{
				StatusCode = (int)aPIResponse.StatusCode
			};
		}
		return aPIResponse.Response.ToObjectResult((int)aPIResponse.StatusCode);
	}

	[Route("Return")]
	[HttpPost]
	[ProducesResponseType(401)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(ReturnResponse), 200)]
	public async Task<ActionResult<ReturnResponse>> Return(ReturnRequest request)
	{
		APIResponse<ReturnResponse> aPIResponse = await _transactionService.Return(request);
		aPIResponse.CheckAndReplay(request.MessageId, base.ControllerContext.HttpContext.Request, _httpQueueService);
		if (aPIResponse.Response == null)
		{
			return new ObjectResult(null)
			{
				StatusCode = (int)aPIResponse.StatusCode
			};
		}
		return aPIResponse.Response.ToObjectResult();
	}

	[Route("GetCustomerOffers")]
	[HttpPost]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(ConvertedGetCustomerOffersResponse), 200)]
	public async Task<ActionResult<ConvertedGetCustomerOffersResponse>> GetCustomerOffers(GetCustomerOffersRequest request)
	{
		APIResponse<GetCustomerOffersResponse> aPIResponse = await _transactionService.GetCustomerOffers(request);
		return aPIResponse.Response.ConvertExpirationDates().ToObjectResult((int)aPIResponse.StatusCode);
	}

	[Route("ValidatePickup")]
	[HttpPost]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(typeof(PickupValidationResponse), 200)]
	public async Task<ActionResult<PickupValidationResponse>> ValidatePickup(PickupValidationRequest request)
	{
		APIResponse<PickupValidationResponse> aPIResponse = await _transactionService.PickupValidation(request);
		if (aPIResponse.Response == null)
		{
			return new ObjectResult(null)
			{
				StatusCode = (int)aPIResponse.StatusCode
			};
		}
		return aPIResponse.Response.ToObjectResult();
	}

	[Route("return/{barcode}/promocheck")]
	[HttpGet]
	[ProducesResponseType(typeof(ReturnPromoCheckResponse), 200)]
	[ProducesResponseType(typeof(ReturnPromoCheckResponse), 500)]
	public async Task<ActionResult<ReturnPromoCheckResponse>> ReturnPromoCheck(string barcode)
	{
		return (await _transactionService.ReturnPromoCheck(barcode)).ToObjectResult();
	}
}
