using System;
using Newtonsoft.Json;

namespace Redbox.Rental.Model.Reservation.DataFile;

public class ReservationDataFile_V002 : IReservationDataFile
{
	[JsonRequired]
	public string HashedCardId { get; set; }

	[JsonIgnore]
	public string FileName => $"{ID}.dat";

	[JsonRequired]
	public Guid ID { get; set; }

	[JsonRequired]
	public string ReferenceNumber { get; set; }

	public ReservationDataInfo_V001 ReservationDataInfo { get; set; }
}
