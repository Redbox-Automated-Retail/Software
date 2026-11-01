using System;
using System.Collections.Generic;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002ERange
{
	public int Size => End - Start + 1;

	public int End { get; private set; }

	public int Start { get; private set; }

	public Redbox_002EMacros1309_002ERange(int start, int end)
	{
		End = end;
		Start = start;
	}

	public Redbox_002EMacros1309_002ERange(string value)
	{
		string[] parts = value.Split("..".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length < 2)
		{
			throw new ArgumentException("A range must be formatted as x..y");
		}
		End = 0;
		Start = 0;
		if (int.TryParse(parts[1], out var end))
		{
			End = end;
		}
		if (int.TryParse(parts[0], out var start))
		{
			Start = start;
		}
	}

	public bool InRange(int value)
	{
		if (value >= Start)
		{
			return value <= End;
		}
		return false;
	}

	public IEnumerable<int> GetNextInRnage()
	{
		for (int i = Start; i <= End; i++)
		{
			yield return i;
		}
	}
}
