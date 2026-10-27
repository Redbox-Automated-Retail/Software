namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class BigEndianConverter
{
	internal int ToInt32(byte[] buffer, int startIndex)
	{
		return Read(buffer, startIndex, 4);
	}

	internal short ToInt16(byte[] buffer, int startIndex)
	{
		return (short)Read(buffer, startIndex, 2);
	}

	internal ushort ToUInt16(byte[] buffer, int startIndex)
	{
		return (ushort)Read(buffer, startIndex, 2);
	}

	internal BigEndianConverter()
	{
	}

	private int Read(byte[] buffer, int startIndex, int bytesToConvert)
	{
		int num = 0;
		for (int i = 0; i < bytesToConvert; i++)
		{
			num = (num << 8) | buffer[startIndex + i];
		}
		return num;
	}
}
