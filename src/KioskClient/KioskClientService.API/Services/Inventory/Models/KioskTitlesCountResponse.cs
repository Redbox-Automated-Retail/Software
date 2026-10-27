using System;
using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Inventory.Models;

public class KioskTitlesCountResponse : ApiBaseResponse
{
	public class _KioskTitlesCount
	{
		public Dictionary<long, int> TitlesCount = new Dictionary<long, int>();

		public long KioskId { get; set; }
	}

	public Guid? MessageId { get; set; }

	public List<_KioskTitlesCount> KioskTitlesCount { get; set; } = new List<_KioskTitlesCount>();

	public override object Scrub()
	{
		return this.ToJson();
	}
}
