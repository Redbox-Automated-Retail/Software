using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Campaign;

public interface IKioskClientServiceCampaign
{
	bool EnableInCart { get; }

	bool ShowStartScreen { get; }

	int LoadCampaignTimerDuration { get; }

	void LoadCampaign();

	KioskInCartDetails GetInCart();

	KioskStartScreenDetails GetStartScreen();

	ICarouselControl GetCarouselControl();

	IList<IStartAssetControl> GetStartAssetControls();
}
