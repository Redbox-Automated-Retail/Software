using System.Threading.Tasks;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Transaction;

public interface ITransactionService
{
	Task<AuthorizeResponse> Authorize(AuthorizeRequest request);

	Task<APIResponse<ReturnResponse>> Return(ReturnRequest request);

	Task<GCAuthResponse> GiftCardAuthorize(GCAuthRequest request);

	Task<AuthorizeResponse> PickupAuthorize(PickupAuthorizeRequest request);

	Task<AuthorizeResponse> ReservationAuthorize(ResvAuthRequest request);

	Task<APIResponse<CancelReservationResponse>> CancelReservation(CancelReservationRequest request);

	Task<APIResponse<ExpireReservationResponse>> ExpireReservation(ExpireReservationRequest request);

	Task<APIResponse<ReconcileResponse>> Reconcile(ReconcileRequest request);

	Task<APIResponse<ReconcileResponse>> ReservationPickup(ReservationPickupRequest request);

	Task<APIResponse<GetCustomerOffersResponse>> GetCustomerOffers(GetCustomerOffersRequest request);

	Task<APIResponse<PickupValidationResponse>> PickupValidation(PickupValidationRequest request);

	Task<ReturnPromoCheckResponse> ReturnPromoCheck(string barcode);
}
