using Redbox.HAL.Core;

namespace Redbox.HAL.ICameraPlugin.Vidcap;

public class Provider
{
	public static ICameraPlugin GetCameraPlugin()
	{
		return (ICameraPlugin)(object)VidcapService.Instance;
	}
}
