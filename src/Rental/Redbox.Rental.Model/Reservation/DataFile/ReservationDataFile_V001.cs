using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Redbox.Rental.Model.Reservation.DataFile;

public class ReservationDataFile_V001 : IReservationDataFile
{
	private List<ReservationDataInfo_V001> _dataInfos = new List<ReservationDataInfo_V001>();

	public string HashedCardId { get; set; }

	[JsonIgnore]
	public string FileName => $"{ID}.dat";

	public Guid ID { get; set; }

	public List<ReservationDataInfo_V001> ReservationDataInfos
	{
		get
		{
			return _dataInfos;
		}
		set
		{
			_dataInfos = value;
		}
	}
}
