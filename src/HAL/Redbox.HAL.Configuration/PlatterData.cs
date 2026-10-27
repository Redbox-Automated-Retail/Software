using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.Configuration;

public sealed class PlatterData
{
	public int YOffset { get; private set; }

	public int[] SegmentOffsets { get; internal set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(YOffset);
		int[] segmentOffsets = SegmentOffsets;
		foreach (int num in segmentOffsets)
		{
			stringBuilder.Append($",{num}");
		}
		return stringBuilder.ToString();
	}

	internal PlatterData(List<int> data)
		: this(data, removeZeroes: true)
	{
	}

	internal PlatterData(List<int> data, bool removeZeroes)
	{
		YOffset = data[0];
		data.RemoveAt(0);
		if (removeZeroes)
		{
			data.RemoveAll((int each) => each == 0);
		}
		SegmentOffsets = data.ToArray();
	}
}
