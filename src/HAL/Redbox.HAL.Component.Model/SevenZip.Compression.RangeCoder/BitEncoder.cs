namespace SevenZip.Compression.RangeCoder;

internal struct BitEncoder
{
	public const int kNumBitModelTotalBits = 11;

	public const uint kBitModelTotal = 2048u;

	private const int kNumMoveBits = 5;

	private const int kNumMoveReducingBits = 2;

	public const int kNumBitPriceShiftBits = 6;

	private uint Prob;

	private static uint[] ProbPrices;

	public void Init()
	{
		Prob = 1024u;
	}

	public void UpdateModel(uint symbol)
	{
		if (symbol == 0)
		{
			Prob += 2048 - Prob >> 5;
		}
		else
		{
			Prob -= Prob >> 5;
		}
	}

	public void Encode(Encoder encoder, uint symbol)
	{
		uint newBound = (encoder.Range >> 11) * Prob;
		if (symbol == 0)
		{
			encoder.Range = newBound;
			Prob += 2048 - Prob >> 5;
		}
		else
		{
			encoder.Low += newBound;
			encoder.Range -= newBound;
			Prob -= Prob >> 5;
		}
		if (encoder.Range < 16777216)
		{
			encoder.Range <<= 8;
			encoder.ShiftLow();
		}
	}

	static BitEncoder()
	{
		ProbPrices = new uint[512];
		for (int i = 8; i >= 0; i--)
		{
			uint start = (uint)(1 << 9 - i - 1);
			uint end = (uint)(1 << 9 - i);
			for (uint j = start; j < end; j++)
			{
				ProbPrices[j] = (uint)(i << 6) + (end - j << 6 >> 9 - i - 1);
			}
		}
	}

	public uint GetPrice(uint symbol)
	{
		return ProbPrices[(((Prob - symbol) ^ (int)(0 - symbol)) & 0x7FF) >> 2];
	}

	public uint GetPrice0()
	{
		return ProbPrices[Prob >> 2];
	}

	public uint GetPrice1()
	{
		return ProbPrices[2048 - Prob >> 2];
	}
}
