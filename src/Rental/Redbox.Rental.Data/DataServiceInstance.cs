using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskData;
using Redbox.Rental.Model.Local;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Reservation;

namespace Redbox.Rental.Data;

public class DataServiceInstance : IDataServiceInstance
{
	// The instances below are set by the data service right after construction.
	public IProfileDataInstance ProfileDataInstance { get; set; } = null!;

	public ICacheDataInstance CacheDataInstance { get; set; } = null!;

	public IKioskDataInstance KioskDataInstance { get; set; } = null!;

	public ILocalDataInstance LocalDataInstance { get; set; } = null!;

	public IReservationDataInstance ReservationDataInstance { get; set; } = null!;
}
