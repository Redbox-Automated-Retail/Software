using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KioskClientService.API.App;
using KioskClientService.API.Validation;

namespace KioskClientService.API.Services.ProxyApi;

public class NearbyKiosksResponse : ApiBaseResponse
{
	public class NearbyKiosk
	{
		[Required]
		[NotDefault]
		public long KioskId { get; set; }

		public bool IsDual { get; set; }

		public string Label { get; set; }

		public string LocationName { get; set; }

		public LocationType LocationType { get; set; }

		public string Address { get; set; }

		public decimal DistanceMiles { get; set; }
	}

	public Guid? MessageId { get; set; }

	public List<NearbyKiosk> Kiosks { get; set; } = new List<NearbyKiosk>();

	public override object Scrub()
	{
		return this.ToJson();
	}
}
