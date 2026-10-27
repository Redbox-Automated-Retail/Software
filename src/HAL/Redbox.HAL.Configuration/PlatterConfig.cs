using System;

namespace Redbox.HAL.Configuration;

public sealed class PlatterConfig
{
	private const int SellThruSlotWidth = 466;

	public PlatterType Type { get; private set; }

	public int SlotsPerQuadrant { get; private set; }

	public int? SellThruOffset { get; private set; }

	public decimal SlotWidth { get; private set; }

	public int QuadrantCount { get; private set; }

	public int SlotCount { get; private set; }

	public PlatterData Data { get; private set; }

	public static PlatterConfig Get(PlatterData data)
	{
		return new PlatterConfig(data);
	}

	public int[] ComputeOffsets(decimal startOffset)
	{
		if (PlatterType.Qlm == Type || Type == PlatterType.None)
		{
			throw new InvalidOperationException($"Can't compute segments on type {Type}");
		}
		int[] array = new int[QuadrantCount];
		decimal num = startOffset;
		for (int i = 0; i < QuadrantCount; i++)
		{
			array[i] = (int)num;
			decimal num2 = 0m;
			num2 = ((PlatterType.Sparse != Type) ? ((decimal)SlotsPerQuadrant * SlotWidth + SlotWidth) : ((decimal)(SlotsPerQuadrant - 1) * SlotWidth + 0.5m + 466m));
			num += num2;
		}
		return array;
	}

	private PlatterConfig(PlatterData data)
	{
		Data = data;
		switch (data.SegmentOffsets.Length)
		{
		case 4:
			Type = PlatterType.Qlm;
			break;
		case 6:
			Type = PlatterType.Dense;
			break;
		case 12:
			Type = PlatterType.Sparse;
			break;
		default:
			Type = PlatterType.None;
			break;
		}
		Configure();
	}

	private PlatterConfig(PlatterType t)
	{
		Type = t;
		Configure();
	}

	private void Configure()
	{
		switch (Type)
		{
		case PlatterType.Dense:
			SlotWidth = 166.6667m;
			QuadrantCount = 6;
			SlotsPerQuadrant = 15;
			SlotCount = 90;
			break;
		case PlatterType.Sparse:
			SlotWidth = 173.3m;
			SellThruOffset = 915;
			QuadrantCount = 12;
			SlotsPerQuadrant = 6;
			SlotCount = 72;
			break;
		case PlatterType.Qlm:
			SlotWidth = 177.7m;
			break;
		}
	}
}
