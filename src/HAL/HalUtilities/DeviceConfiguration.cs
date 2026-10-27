namespace HALUtilities;

public class DeviceConfiguration
{
	public readonly int DevicePixelWidth;

	public readonly int DeviceHeight;

	internal DeviceConfiguration(int width, int height)
	{
		DevicePixelWidth = width;
		DeviceHeight = height;
	}
}
