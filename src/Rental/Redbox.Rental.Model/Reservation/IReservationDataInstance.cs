using System.Collections.Generic;
using Redbox.Rental.Model.Reservation.DataFile;

namespace Redbox.Rental.Model.Reservation;

public interface IReservationDataInstance
{
	Dictionary<string, IReservationDataFile> DataFiles { get; set; }

	void LoadAll();

	bool Save(IReservationDataFile reservationDataFile);

	void Delete(IReservationDataFile reservationDataFile);
}
