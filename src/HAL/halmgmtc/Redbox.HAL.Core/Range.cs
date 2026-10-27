using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public sealed class Range : IRange<int>
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
		string[] array = value.Split("..".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		if (array.Length < 2)
		{
			throw new ArgumentException("A range must be formatted as x..y");
		}
		End = 0;
		Start = 0;
		if (int.TryParse(array[1], out var result))
		{
			End = result;
		}
		if (int.TryParse(array[0], out var result2))
		{
			Start = result2;
		}
	}

	public bool Includes(int value)
	{
		if (value >= Start)
		{
			return value <= End;
		}
		return false;
	}

	public bool Includes(IRange<int> range)
	{
		if (range.Start >= Start)
		{
			return range.End <= End;
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
