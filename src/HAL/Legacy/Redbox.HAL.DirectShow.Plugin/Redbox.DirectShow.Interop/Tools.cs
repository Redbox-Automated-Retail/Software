using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

internal static class Tools
{
	public static IPin GetPin(IBaseFilter filter, PinDirection dir, int num)
	{
		IPin[] array = new IPin[1];
		IEnumPins enumPins = null;
		if (filter.EnumPins(out enumPins) == 0)
		{
			try
			{
				int pinsFetched;
				while (enumPins.Next(1, array, out pinsFetched) == 0)
				{
					array[0].QueryDirection(out var pinDirection);
					if (pinDirection == dir)
					{
						if (num == 0)
						{
							return array[0];
						}
						num--;
					}
					Marshal.ReleaseComObject(array[0]);
					array[0] = null;
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
