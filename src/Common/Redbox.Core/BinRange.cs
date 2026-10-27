namespace Redbox.Core;

public class BinRange
{
	public int StartingRange { get; private set; }

	public int EndingRange { get; private set; }

	public BinRange()
	{
	}

	public BinRange(int startingRange, int endingRange)
	{
		StartingRange = startingRange;
		EndingRange = endingRange;
	}

	public bool Between(int bin)
	{
		if (bin >= StartingRange)
		{
			return bin <= EndingRange;
		}
		return false;
	}
}
