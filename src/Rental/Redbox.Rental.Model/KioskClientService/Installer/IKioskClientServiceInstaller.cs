namespace Redbox.Rental.Model.KioskClientService.Installer;

public interface IKioskClientServiceInstaller
{
	OpsMarketResponse GetOpsMarketForKiosk(int kioskId);

	PendingKiosksResponse GetPendingKiosksForStateAndBanner(int stateId, int banner);

	PendingBannersResponse GetPendingBannersForState(int stateId);

	PendingStatesResponse GetPendingStates();
}
