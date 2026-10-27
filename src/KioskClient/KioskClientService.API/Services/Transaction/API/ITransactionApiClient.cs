using System.Threading.Tasks;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Transaction.API;

public interface ITransactionApiClient
{
	Task<APIResponse<AuthorizeResponse>> Authorize(AuthorizeRequest request);

	Task<APIResponse<ReturnResponse>> Return(ReturnRequest request);

	Task<APIResponse<GCAuthResponse>> GiftCardAuthorize(GCAuthRequest request);

	Task<APIResponse<AuthorizeResponse>> PickupAuthorize(PickupAuthorizeRequest request);

	Task<APIResponse<AuthorizeResponse>> ReservationAuthorize(AuthorizeRequest request);

	Task<APIResponse<CancelReservationResponse>> CancelReservation(CancelReservationRequest request);

	Task<APIResponse<ExpireReservationResponse>> ExpireReservation(ExpireReservationRequest request);

	Task<APIResponse<ReconcileResponse>> Reconcile(ReconcileRequest request);

	Task<APIResponse<ReconcileResponse>> ReservationPickup(ReservationPickupRequest request);

	Task<APIResponse<GetCustomerOffersResponse>> GetCustomerOffers(GetCustomerOffersRequest request);

	Task<APIResponse<PickupValidationResponse>> PickupValidation(PickupValidationRequest request);

	Task<APIResponse<ReturnPromoCheckApiResponse>> ReturnPromoCheck(string barcode);
}
