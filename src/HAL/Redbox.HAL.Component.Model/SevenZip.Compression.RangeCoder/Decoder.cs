using System.IO;

namespace SevenZip.Compression.RangeCoder;

internal class Decoder
{
	public const uint kTopValue = 16777216u;

	public uint Range;

	public uint Code;

	public Stream? Stream;

	public void Init(Stream stream)
	{
		Stream = stream;
		Code = 0u;
		Range = uint.MaxValue;
		for (int i = 0; i < 5; i++)
		{
			Code = (Code << 8) | (byte)Stream.ReadByte();
		}
	}

	public void ReleaseStream()
	{
		Stream = null;
	}

	public void CloseStream()
	{
		Stream!.Close();
	}

	public void Normalize()
	{
		while (Range < 16777216)
		{
			Code = (Code << 8) | (byte)Stream!.ReadByte();
			Range <<= 8;
		}
	}

	public void Normalize2()
	{
		if (Range < 16777216)
		{
			Code = (Code << 8) | (byte)Stream!.ReadByte();
			Range <<= 8;
		}
	}

	public uint GetThreshold(uint total)
	{
		return Code / (Range /= total);
	}

	public void Decode(uint start, uint size, uint total)
	{
		Code -= start * Range;
		Range *= size;
		Normalize();
	}

	public uint DecodeDirectBits(int numTotalBits)
	{
		uint range = Range;
		uint code = Code;
		uint result = 0u;
		for (int i = numTotalBits; i > 0; i--)
		{
			range >>= 1;
			uint t = code - range >> 31;
			code -= range & (t - 1);
			result = (result << 1) | (1 - t);
			if (range < 16777216)
			{
				code = (code << 8) | (byte)Stream!.ReadByte();
				range <<= 8;
			}
		}
		Range = range;
		Code = code;
		return result;
	}

	public uint DecodeBit(uint size0, int numTotalBits)
	{
		uint newBound = (Range >> numTotalBits) * size0;
		uint result;
		if (Code < newBound)
		{
			result = 0u;
			Range = newBound;
		}
		else
		{
			result = 1u;
			Code -= newBound;
			Range -= newBound;
		}
		Normalize();
		return result;
	}
}
