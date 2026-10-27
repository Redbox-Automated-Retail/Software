using Redbox.HAL.Core;

namespace Redbox.HAL.DirectShow.Plugin;

public sealed class Factory
{
	private static Service m_instance;

	public static ICameraPlugin GetCameraPlugin()
	{
		if (m_instance == null)
		{
			m_instance = new Service();
		}
		return (ICameraPlugin)(object)m_instance;
	}
}
