namespace SevenZip.Compression.RangeCoder;

internal struct BitTreeDecoder(int numBitLevels)
{
	private BitDecoder[] Models = new BitDecoder[1 << numBitLevels];

	private int NumBitLevels = numBitLevels;

	public void Init()
	{
		for (uint i = 1u; i < 1 << NumBitLevels; i++)
		{
			Models[i].Init();
		}
	}

	public uint Decode(Decoder rangeDecoder)
	{
		uint m = 1u;
		for (int bitIndex = NumBitLevels; bitIndex > 0; bitIndex--)
		{
			m = (m << 1) + Models[m].Decode(rangeDecoder);
		}
		return m - (uint)(1 << NumBitLevels);
	}

	public uint ReverseDecode(Decoder rangeDecoder)
	{
		uint m = 1u;
		uint symbol = 0u;
		for (int i = 0; i < NumBitLevels; i++)
		{
			uint bit = Models[m].Decode(rangeDecoder);
			m <<= 1;
			m += bit;
			symbol |= bit << i;
		}
		return symbol;
	}

	public static uint ReverseDecode(BitDecoder[] Models, uint startIndex, Decoder rangeDecoder, int NumBitLevels)
	{
		uint m = 1u;
		uint symbol = 0u;
		for (int i = 0; i < NumBitLevels; i++)
		{
			uint bit = Models[startIndex + m].Decode(rangeDecoder);
			m <<= 1;
			m += bit;
			symbol |= bit << i;
		}
		return symbol;
	}
}
