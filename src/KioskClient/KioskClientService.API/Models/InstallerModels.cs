using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Models;

public static class InstallerModels
{
	public class Banner
	{
		public int Id { get; set; }

		public string Name { get; set; }
	}

	public class PendingKiosk
	{
		public int Id { get; set; }

		public string Address { get; set; }

		public string City { get; set; }

		public string State { get; set; }

		public string ZipCode { get; set; }

		public string DueTime { get; set; }

		public string MarketName { get; set; }

		public string KaseyaMarketName { get; set; }
	}

	public class State
	{
		public short Id { get; set; }

		public string Description { get; set; }
	}

	public class OpsMarketResponse : BaseResponse
	{
		public string OpsMarket { get; set; }

		public string KaseyaServer { get; set; }

		public OpsMarketResponse()
			: base(200)
		{
		}
	}

	public class PendingBannersResponse : BaseResponse
	{
		public List<Banner> Banners { get; }

		public PendingBannersResponse()
			: base(200)
		{
			Banners = new List<Banner>();
		}
	}

	public class PendingKiosksResponse : BaseResponse
	{
		public List<PendingKiosk> PendingKiosks { get; }

		public PendingKiosksResponse()
			: base(200)
		{
			PendingKiosks = new List<PendingKiosk>();
		}
	}

	public class PendingStatesResponse : BaseResponse
	{
		public List<State> States { get; }

		public PendingStatesResponse()
			: base(200)
		{
			States = new List<State>();
		}
	}
}
