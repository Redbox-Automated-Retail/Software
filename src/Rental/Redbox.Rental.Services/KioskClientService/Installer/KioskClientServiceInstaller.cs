using System;
using Redbox.Rental.Model.KioskClientService.Installer;

namespace Redbox.Rental.Services.KioskClientService.Installer;

public class KioskClientServiceInstaller : BaseKioskClientService, IKioskClientServiceInstaller
{
	private const string _apiPath = "api/installer";

	public OpsMarketResponse GetOpsMarketForKiosk(int kioskId)
	{
		return ExecuteWebRequest<OpsMarketResponse>(new Uri(string.Format("{0}/{1}/getopsmarket?kioskid={2}", base.KioskClientServiceBaseUrl, "api/installer", kioskId)), null, "GET", 20000).Response;
	}

	public PendingBannersResponse GetPendingBannersForState(int stateId)
	{
		return ExecuteWebRequest<PendingBannersResponse>(new Uri(string.Format("{0}/{1}/getpendingbanners?stateid={2}", base.KioskClientServiceBaseUrl, "api/installer", stateId)), null, "GET", 20000).Response;
	}

	public PendingKiosksResponse GetPendingKiosksForStateAndBanner(int stateId, int bannerId)
	{
		return ExecuteWebRequest<PendingKiosksResponse>(new Uri(string.Format("{0}/{1}/getpendingkiosks?stateid={2}&bannerid={3}", base.KioskClientServiceBaseUrl, "api/installer", stateId, bannerId)), null, "GET", 20000).Response;
	}

	public PendingStatesResponse GetPendingStates()
	{
		return ExecuteWebRequest<PendingStatesResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/installer/getpendingstates"), null, "GET", 20000).Response;
	}
}
