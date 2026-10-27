using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DirectShow.Frame.Plugin;

public sealed class Factory
{
	private static ICameraPlugin m_instance;

	public static ICameraPlugin GetCameraPlugin()
	{
		if (m_instance == null)
		{
			m_instance = new Service();
		}
		return m_instance;
	}

	private Factory()
	{
	}
}
