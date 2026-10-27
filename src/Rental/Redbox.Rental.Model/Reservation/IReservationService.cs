using System.Threading.Tasks;
using Redbox.Rental.Model.UpdateClientService.Reservation;

namespace Redbox.Rental.Model.Reservation;

public interface IReservationService
{
	Task<GetReservationServiceStatusResponse> GetReservationServiceStatus();

	Task<GetReservationsResponse> GetReservations();

	Task<GetReservableTitlesResponse> GetReservableTitles();

	Task<CreateReservationResponse> CreateReservation(ReservationRequest reservationRequest);

	Task<CancelReservationResponse> CancelReservation(string referenceNumber);

	void RegisterWithAWSBrokerService();

	Task<IRegisterWithBrokerServiceResponse> RegisterWithAWSBrokerServiceAsync();

	void UnRegisterFromAWSBrokerService(UnRegisterFromBrokerServiceReason reason);

	Task<IUnregisterFromBrokerServiceResponse> UnRegisterFromAWSBrokerServiceAsync(UnRegisterFromBrokerServiceReason reason);
}
