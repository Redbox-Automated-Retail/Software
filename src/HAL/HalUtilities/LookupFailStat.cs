namespace HALUtilities;

internal class LookupFailStat
{
	internal int Count { get; private set; }

	internal string Matrix { get; private set; }

	internal void Increment()
	{
		int count = Count + 1;
		Count = count;
	}

	internal LookupFailStat(string barcode)
	{
		Matrix = barcode;
	}
}
