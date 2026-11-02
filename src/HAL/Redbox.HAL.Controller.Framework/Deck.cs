using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Range = Redbox.HAL.Component.Model.Range;

namespace Redbox.HAL.Controller.Framework;

public sealed class Deck : IDeck
{
	internal const int DefaultSlot1Offset = 780;

	private const int SlotsPerQuadrantDenseDeck = 15;

	private const int SlotsPerQuadrantSparseDeck = 6;

	private readonly List<IQuadrant> m_quadrants = new List<IQuadrant>();

	public bool IsSparse => NumberOfSlots == 72;

	public int Number { get; private set; }

	public int YOffset { get; private set; }

	public bool IsQlm { get; private set; }

	public int NumberOfSlots { get; private set; }

	public decimal SlotWidth { get; private set; }

	public int? SellThruSlots { get; private set; }

	public int? SellThruOffset { get; private set; }

	public int SlotsPerQuadrant { get; private set; }

	public List<IQuadrant> Quadrants => m_quadrants;

	public override int GetHashCode()
	{
		return Number;
	}

	public override bool Equals(object? obj)
	{
		if (!(obj is Deck deck))
		{
			return false;
		}
		return deck == this;
	}

	public static bool operator !=(Deck? lhs, Deck? rhs)
	{
		return !(lhs == rhs);
	}

	public static bool operator ==(Deck? lhs, Deck? rhs)
	{
		if ((object?)lhs == rhs)
		{
			return true;
		}
		if ((object?)lhs == null || (object?)rhs == null)
		{
			return false;
		}
		if (lhs.YOffset != rhs.YOffset)
		{
			return false;
		}
		if (lhs.NumberOfSlots != rhs.NumberOfSlots)
		{
			return false;
		}
		if (lhs.IsQlm != rhs.IsQlm)
		{
			return false;
		}
		if (lhs.SlotWidth != rhs.SlotWidth)
		{
			return false;
		}
		if (lhs.SellThruOffset != rhs.SellThruOffset)
		{
			return false;
		}
		if (lhs.Quadrants.Count != rhs.Quadrants.Count)
		{
			return false;
		}
		for (int i = 0; i < lhs.Quadrants.Count; i++)
		{
			IQuadrant quadrant = lhs.Quadrants[i];
			IQuadrant quadrant2 = rhs.Quadrants[i];
			if (quadrant.Offset != quadrant2.Offset || quadrant.IsExcluded != quadrant2.IsExcluded)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsSlotSellThru(int slot)
	{
		if (SellThruSlots.HasValue)
		{
			return slot % SellThruSlots.Value == 0;
		}
		return false;
	}

	public int GetSlotOffset(int slot)
	{
		if (!IsSlotValid(slot))
		{
			throw new ArgumentException($"The slot parameter must be between 1 and {NumberOfSlots}.");
		}
		int slotIndex = slot - 1;
		int quadrantIndex = slotIndex / SlotsPerQuadrant;
		if (quadrantIndex >= Quadrants.Count)
		{
			throw new ArgumentException($"The computed quadrant offset {quadrantIndex} is outside the range of defined quadrants.  This usuallys means the deck is incorrectly configured.");
		}
		int offset = Quadrants[quadrantIndex].Offset;
		IQuadrant? quadrant = FindQuadrantThatContainsSlot(slot);
		int slotInQuadrant = slotIndex % SlotsPerQuadrant;
		if (quadrant != null)
		{
			slotInQuadrant = slot - quadrant.Slots!.Start;
			offset = quadrant.Offset;
		}
		decimal slotOffset = (decimal)slotInQuadrant * SlotWidth;
		if (IsSlotSellThru(slot))
		{
			slotOffset = SellThruOffset ?? 915;
		}
		return (int)((decimal)offset + slotOffset);
	}

	public bool IsSlotValid(int slot)
	{
		if (slot >= 1)
		{
			return slot <= NumberOfSlots;
		}
		return false;
	}

	internal Deck(int number, int yoffset, bool isQlm, int numberOfSlots, decimal slotWidth, int? sellThruSlots, int? sellThruOffset, Quadrant[] quadrantOffsets)
	{
		Number = number;
		YOffset = yoffset;
		IsQlm = isQlm;
		SlotWidth = slotWidth;
		SellThruSlots = sellThruSlots;
		SellThruOffset = sellThruOffset;
		NumberOfSlots = numberOfSlots;
		if (quadrantOffsets != null)
		{
			m_quadrants.AddRange(quadrantOffsets);
		}
		else
		{
			m_quadrants.Add(new Quadrant(780));
		}
		SlotsPerQuadrant = NumberOfSlots / Quadrants.Count;
	}

	internal static Deck FromXmlNode(XmlNode node)
	{
		int number = node.GetAttributeValue("Number", 1);
		int offset = node.GetAttributeValue("Offset", -18760);
		bool isQlm = node.GetAttributeValue("IsQlm", defaultValue: false);
		decimal slotWidth = node.GetAttributeValue("SlotWidth", 166.6667m);
		int numberOfSlots = node.GetAttributeValue("NumberOfSlots", 90);
		int? sellThruSlots = node.GetAttributeValue<int?>("SellThruSlots", null);
		int? sellThruOffset = node.GetAttributeValue<int?>("SellThruOffset", null);
		List<Quadrant> quadrants = new List<Quadrant>();
		int quadrantIndex = 0;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			int quadrantOffset = childNode.GetAttributeValue("Offset", 780);
			int? startSlot = childNode.GetAttributeValue<int?>("StartSlot", null);
			int? endSlot = childNode.GetAttributeValue<int?>("EndSlot", null);
			bool isExcluded = childNode.GetAttributeValue("IsExcluded", defaultValue: false);
			Range? range = null;
			if (startSlot.HasValue && endSlot.HasValue)
			{
				range = new Range(startSlot.Value, endSlot.Value);
			}
			else
			{
				int slotsPerQuadrant = ((90 == numberOfSlots) ? 15 : 6);
				int firstSlot = quadrantIndex * slotsPerQuadrant + 1;
				int end = firstSlot + slotsPerQuadrant - 1;
				range = new Range(firstSlot, end);
			}
			Quadrant quadrant = new Quadrant(quadrantOffset, range);
			quadrants.Add(quadrant);
			quadrant.IsExcluded = isExcluded;
			quadrantIndex++;
		}
		return new Deck(number, offset, isQlm, numberOfSlots, slotWidth, sellThruSlots, sellThruOffset, quadrants.ToArray());
	}

	internal void ToXmlWriter(XmlWriter writer)
	{
		writer.WriteStartElement("Deck");
		writer.WriteAttributeString("Number", XmlConvert.ToString(Number));
		writer.WriteAttributeString("Offset", XmlConvert.ToString(YOffset));
		writer.WriteAttributeString("IsQlm", XmlConvert.ToString(IsQlm));
		writer.WriteAttributeString("SlotWidth", XmlConvert.ToString(SlotWidth));
		writer.WriteAttributeString("NumberOfSlots", XmlConvert.ToString(NumberOfSlots));
		if (SellThruSlots.HasValue)
		{
			writer.WriteAttributeString("SellThruSlots", XmlConvert.ToString(SellThruSlots.Value));
		}
		if (SellThruOffset.HasValue)
		{
			writer.WriteAttributeString("SellThruOffset", XmlConvert.ToString(SellThruOffset.Value));
		}
		foreach (IQuadrant quadrant in Quadrants)
		{
			writer.WriteStartElement("Quadrant");
			writer.WriteAttributeString("Offset", XmlConvert.ToString(quadrant.Offset));
			if (quadrant.Slots != null)
			{
				writer.WriteAttributeString("StartSlot", XmlConvert.ToString(quadrant.Slots.Start));
				writer.WriteAttributeString("EndSlot", XmlConvert.ToString(quadrant.Slots.End));
			}
			if (quadrant.IsExcluded)
			{
				writer.WriteAttributeString("IsExcluded", XmlConvert.ToString(value: true));
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	internal void UpdateFrom(Deck newProperties)
	{
		YOffset = newProperties.YOffset;
		IsQlm = newProperties.IsQlm;
		SlotWidth = newProperties.SlotWidth;
		SellThruOffset = newProperties.SellThruOffset;
		_ = NumberOfSlots;
		NumberOfSlots = newProperties.NumberOfSlots;
		m_quadrants.Clear();
		m_quadrants.AddRange(newProperties.Quadrants);
	}

	private IQuadrant? FindQuadrantThatContainsSlot(int slot)
	{
		foreach (IQuadrant quadrant in Quadrants)
		{
			if (quadrant.Slots == null)
			{
				return null;
			}
			if (quadrant.Slots.Includes(slot))
			{
				return quadrant;
			}
		}
		return null;
	}
}
