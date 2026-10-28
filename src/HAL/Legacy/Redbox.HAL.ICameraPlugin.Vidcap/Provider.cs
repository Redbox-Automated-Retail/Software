using Redbox.HAL.Core;

namespace Redbox.HAL.ICameraPlugin.Vidcap;

public class Provider
{
	public static global::Redbox.HAL.Core.ICameraPlugin GetCameraPlugin()
	{
		return (global::Redbox.HAL.Core.ICameraPlugin)(object)VidcapService.Instance;
	}
}
