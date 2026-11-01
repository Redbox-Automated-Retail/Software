using System;
using System.Collections.Generic;

namespace Redbox.Core;

public class Range
{
	public int Size => End - Start + 1;

	public int End { get; private set; }

	public int Start { get; private set; }

	public Range(int start, int end)
	{
		End = end;
		Start = start;
	}

	public Range(string value)
	{
		string[] parts = value.Split("..".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length < 2)
		{
			throw new ArgumentException("A range must be formatted as x..y");
		}
		End = 0;
		Start = 0;
		if (int.TryParse(parts[1], out var result))
		{
			End = result;
		}
		if (int.TryParse(parts[0], out var result2))
		{
			Start = result2;
		}
	}

	public bool InRange(int value)
	{
		return value >= Start && value <= End;
	}

	public IEnumerable<int> GetNextInRnage()
	{
		for (int i = Start; i <= End; i++)
		{
			yield return i;
		}
	}
}
