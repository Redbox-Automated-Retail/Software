using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.Local;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Reservation;

namespace Redbox.Rental.Model.DataService;

public interface IDataServiceInstance
{
	IProfileDataInstance ProfileDataInstance { get; set; }

	ICacheDataInstance CacheDataInstance { get; set; }

	ILocalDataInstance LocalDataInstance { get; set; }

	IReservationDataInstance ReservationDataInstance { get; set; }
}
