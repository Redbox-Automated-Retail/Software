using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.HttpQueue;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Transaction.API;

public class TransactionApiClient : ITransactionApiClient
{
	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly IHttpService _httpService;

	private readonly ILogger<ITransactionApiClient> _logger;

	private readonly List<Header> _requestHeaders;

	private readonly long _kioskId;

	private readonly IHttpQueueService _httpQueueService;

	private const string _authorizeEndpoint = "api/transaction/authorize";

	private const string _gcAuthEndpoint = "api/transaction/giftcardauthorize";

	private const string _pickupAuthEndpoint = "api/transaction/pickupauthorize";

	private const string _resvAuthEndpoint = "api/transaction/reservationauthorize";

	private const string _returnEndpoint = "api/transaction/return";

	private const string _cancelReservationEndpoint = "api/reservation/cancel";

	private const string _expireReservationEndpoint = "api/reservation/expire";

	private const string _reconcileEndpoint = "api/transaction/reconcile";

	private const string _reservationPickupEndpoint = "api/transaction/reservationPickup";

	private const string _customerOffersEndpoint = "api/customeroffers";

	private const string _pickupValidationEndpoint = "api/reservation/validatepickup";

	private const string _transactionAuthHeader = "x-api-key";

	public TransactionApiClient(IStoreService store, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, IHttpService httpService, ILogger<ITransactionApiClient> logger, IHttpQueueService httpQueueService)
	{
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_httpService = httpService;
		_logger = logger;
		if (_kioskConfiguration.KioskClientDefault.TransactionApiTimeout != 0)
		{
			_httpService.Timeout = _kioskConfiguration.KioskClientDefault.TransactionApiTimeout;
		}
		_kioskId = store?.KioskId ?? 0;
		KioskCloudClientConfiguration kioskCloudClientConfiguration = new KioskCloudClientConfiguration
		{
			APIKey = _kioskConfiguration.KioskClientDefault.TransactionApiKey,
			AuthHeader = "x-api-key"
		};
		_requestHeaders = kioskCloudClientConfiguration.GetKioskCloudHeaders(null, _kioskId);
		_httpQueueService = httpQueueService;
	}

	public async Task<APIResponse<AuthorizeResponse>> Authorize(AuthorizeRequest request)
	{
		return await _httpService.SendRequest<AuthorizeResponse>(_kioskConfiguration.KioskClientDefault.TransactionApiUrl, "api/transaction/authorize", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<GCAuthResponse>> GiftCardAuthorize(GCAuthRequest request)
	{
		return await _httpService.SendRequest<GCAuthResponse>(_kioskConfiguration.KioskClientDefault.TransactionApiUrl, "api/transaction/giftcardauthorize", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AuthorizeResponse>> PickupAuthorize(PickupAuthorizeRequest request)
	{
		return await _httpService.SendRequest<AuthorizeResponse>(_kioskConfiguration.KioskClientDefault.TransactionApiUrl, "api/transaction/pickupauthorize", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AuthorizeResponse>> ReservationAuthorize(AuthorizeRequest request)
	{
		return await _httpService.SendRequest<AuthorizeResponse>(_kioskConfiguration.KioskClientDefault.TransactionApiUrl, "api/transaction/reservationauthorize", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<ReturnResponse>> Return(ReturnRequest request)
	{
		return await _httpService.SendRequest<ReturnResponse>(_kioskConfiguration.KioskClientDefault.TransactionApiUrl, "api/transaction/return", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<CancelReservationResponse>> CancelReservation(CancelReservationRequest request)
	{
		return await _httpService.SendRequest<CancelReservationResponse>(_kioskConfiguration.KioskClientDefault.TransactionApiUrl, "api/reservation/cancel", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<ExpireReservationResponse>> ExpireReservation(ExpireReservationRequest request)
	{
		return await _httpService.SendRequest<ExpireReservationResponse>(_kioskConfiguration.KioskClientDefault.TransactionApiUrl, "api/reservation/expire", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<ReconcileResponse>> Reconcile(ReconcileRequest request)
	{
		return await _httpService.SendRequest<ReconcileResponse>(_kioskConfiguration.KioskClientDefault.TransactionApiUrl, "api/transaction/reconcile", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<ReconcileResponse>> ReservationPickup(ReservationPickupRequest request)
	{
		return await _httpService.SendRequest<ReconcileResponse>(_kioskConfiguration.KioskClientDefault.TransactionApiUrl, "api/transaction/reservationPickup", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<GetCustomerOffersResponse>> GetCustomerOffers(GetCustomerOffersRequest request)
	{
		List<Header> list = new List<Header>(_requestHeaders);
		list.Remove(list.SingleOrDefault((Header h) => h.Key == "x-redbox-activityid"));
		list.Add(new Header("x-redbox-activityid", request.MessageId.ToString()));
		return await _httpService.SendRequest<GetCustomerOffersResponse>(_kioskConfiguration.KioskClientDefault.TransactionApiUrl, "api/customeroffers", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<PickupValidationResponse>> PickupValidation(PickupValidationRequest request)
	{
		return await _httpService.SendRequest<PickupValidationResponse>(_kioskConfiguration.KioskClientDefault.TransactionApiUrl, "api/reservation/validatepickup", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<ReturnPromoCheckApiResponse>> ReturnPromoCheck(string barcode)
	{
		string endpoint = "api/transaction/return/" + barcode + "/promocheck";
		if (_kioskConfiguration.KioskClientDefault.ReturnPromoCheckTimeout != 0)
		{
			_httpService.Timeout = _kioskConfiguration.KioskClientDefault.ReturnPromoCheckTimeout;
		}
		return await _httpService.SendRequest<ReturnPromoCheckApiResponse>(_kioskConfiguration.KioskClientDefault.TransactionApiUrl, endpoint, null, HttpMethod.Get, _requestHeaders);
	}
}
