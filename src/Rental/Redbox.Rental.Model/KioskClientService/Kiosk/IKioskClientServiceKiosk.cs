using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redbox.Rental.Model.KioskClientService.Kiosk;

public interface IKioskClientServiceKiosk
{
	List<NearbyKiosk> NearbyKiosks { get; }

	NearbyKiosk DualKiosk { get; }

	bool HasDual { get; }

	string KioskLabel { get; }

	void LoadNearbyKiosks();

	IKioskClientServiceNearbyKiosksResponse GetNearbyKiosks(long kioskId);

	Task<IKioskClientServiceNearbyKiosksResponse> GetNearbyKiosksAsync(long kioskId);
}
