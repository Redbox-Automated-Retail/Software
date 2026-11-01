using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client;

public sealed class SlotRange : IRange<int>
{
	public int End { get; private set; }

	public int Start { get; private set; }

	public SlotRange(int start, int end)
	{
		End = end;
		Start = start;
	}

	public SlotRange(string value)
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
}
