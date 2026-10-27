using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Campaigns;

public class KioskStartScreenDetails
{
	public ControlList Controls { get; set; } = new ControlList();

	public static KioskStartScreenDetails FromStartScreenDetails(StartScreenDetails startScreenData)
	{
		if (startScreenData == null)
		{
			return null;
		}
		KioskStartScreenDetails kioskStartScreenDetails = new KioskStartScreenDetails();
		startScreenData.Controls.ForEach(kioskStartScreenDetails.Controls.Add);
		return kioskStartScreenDetails;
	}
}
