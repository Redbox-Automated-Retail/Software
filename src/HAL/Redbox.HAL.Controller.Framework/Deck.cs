using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

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

	public override bool Equals(object obj)
	{
		if (!(obj is Deck deck))
		{
			return false;
		}
		return deck == this;
	}

	public static bool operator !=(Deck lhs, Deck rhs)
	{
		return !(lhs == rhs);
	}

	public static bool operator ==(Deck lhs, Deck rhs)
	{
		if ((object)lhs == rhs)
		{
			return true;
		}
		if ((object)lhs == null || (object)rhs == null)
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
		int num = slot - 1;
		int num2 = num / SlotsPerQuadrant;
		if (num2 >= Quadrants.Count)
		{
			throw new ArgumentException($"The computed quadrant offset {num2} is outside the range of defined quadrants.  This usuallys means the deck is incorrectly configured.");
		}
		int offset = Quadrants[num2].Offset;
		IQuadrant quadrant = FindQuadrantThatContainsSlot(slot);
		int num3 = num % SlotsPerQuadrant;
		if (quadrant != null)
		{
			num3 = slot - quadrant.Slots.Start;
			offset = quadrant.Offset;
		}
		decimal num4 = (decimal)num3 * SlotWidth;
		if (IsSlotSellThru(slot))
		{
			num4 = SellThruOffset ?? 915;
		}
		return (int)((decimal)offset + num4);
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
		int attributeValue = node.GetAttributeValue("Number", 1);
		int attributeValue2 = node.GetAttributeValue("Offset", -18760);
		bool attributeValue3 = node.GetAttributeValue("IsQlm", defaultValue: false);
		decimal attributeValue4 = node.GetAttributeValue("SlotWidth", 166.6667m);
		int attributeValue5 = node.GetAttributeValue("NumberOfSlots", 90);
		int? attributeValue6 = node.GetAttributeValue<int?>("SellThruSlots", null);
		int? attributeValue7 = node.GetAttributeValue<int?>("SellThruOffset", null);
		List<Quadrant> list = new List<Quadrant>();
		int num = 0;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			int attributeValue8 = childNode.GetAttributeValue("Offset", 780);
			int? attributeValue9 = childNode.GetAttributeValue<int?>("StartSlot", null);
			int? attributeValue10 = childNode.GetAttributeValue<int?>("EndSlot", null);
			bool attributeValue11 = childNode.GetAttributeValue("IsExcluded", defaultValue: false);
			Range range = null;
			if (attributeValue9.HasValue && attributeValue10.HasValue)
			{
				range = new Range(attributeValue9.Value, attributeValue10.Value);
			}
			else
			{
				int num2 = ((90 == attributeValue5) ? 15 : 6);
				int num3 = num * num2 + 1;
				int end = num3 + num2 - 1;
				range = new Range(num3, end);
			}
			Quadrant quadrant = new Quadrant(attributeValue8, range);
			list.Add(quadrant);
			quadrant.IsExcluded = attributeValue11;
			num++;
		}
		return new Deck(attributeValue, attributeValue2, attributeValue3, attributeValue5, attributeValue4, attributeValue6, attributeValue7, list.ToArray());
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

	private IQuadrant FindQuadrantThatContainsSlot(int slot)
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
