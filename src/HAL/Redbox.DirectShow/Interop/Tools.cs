using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

public static class Tools
{
	public static IPin GetPin(IBaseFilter filter, PinDirection dir, int num)
	{
		IPin[] pins = new IPin[1];
		IEnumPins enumPins = null;
		if (filter.EnumPins(out enumPins) == 0)
		{
			try
			{
				int pinsFetched;
				while (enumPins.Next(1, pins, out pinsFetched) == 0)
				{
					pins[0].QueryDirection(out var pinDirection);
					if (pinDirection == dir)
					{
						if (num == 0)
						{
							return pins[0];
						}
						num--;
					}
					Marshal.ReleaseComObject(pins[0]);
					pins[0] = null;
				}
			}
			finally
			{
				Marshal.ReleaseComObject(enumPins);
			}
		}
		return null;
	}

	public static IPin GetInPin(IBaseFilter filter, int num)
	{
		return GetPin(filter, PinDirection.Input, num);
	}

	public static IPin GetOutPin(IBaseFilter filter, int num)
	{
		return GetPin(filter, PinDirection.Output, num);
	}
}
