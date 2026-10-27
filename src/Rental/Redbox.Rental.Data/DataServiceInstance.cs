using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskData;
using Redbox.Rental.Model.Local;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Reservation;

namespace Redbox.Rental.Data;

public class DataServiceInstance : IDataServiceInstance
{
	public IProfileDataInstance ProfileDataInstance { get; set; }

	public ICacheDataInstance CacheDataInstance { get; set; }

	public IKioskDataInstance KioskDataInstance { get; set; }

	public ILocalDataInstance LocalDataInstance { get; set; }

	public IReservationDataInstance ReservationDataInstance { get; set; }
}
