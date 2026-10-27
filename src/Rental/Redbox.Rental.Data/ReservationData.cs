using System.IO;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model.Reservation;

namespace Redbox.Rental.Data;

public class ReservationData : IReservationData
{
	public class Constants
	{
		public const string DataStoreName = "Reservation Data Store";

		public const string ReservationFolderName = "Reservations";

		public const string ExtensionName = "dat";

		public const string CorruptFileExtensionName = "corrupt_dat";
	}

	private IDataFileStoreService _dataFileStoreService;

	private string _dataPath;

	public ReservationData(string baseDataPath)
	{
		_dataPath = Path.Combine(baseDataPath, "Reservations");
		_dataFileStoreService = new DataFileStoreService();
		_dataFileStoreService.Initialize("Reservation Data Store", _dataPath, "dat", "corrupt_dat", useEncryption: true);
	}

	public IReservationDataInstance LoadData()
	{
		ReservationDataInstance reservationDataInstance = new ReservationDataInstance(_dataFileStoreService);
		((IReservationDataInstance)reservationDataInstance).LoadAll();
		return reservationDataInstance;
	}
}
