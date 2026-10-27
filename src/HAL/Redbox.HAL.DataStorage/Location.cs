using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class Location : ILocation
{
	private readonly int m_deck;

	private readonly int m_slot;

	private readonly string m_stringRep;

	public bool IsEmpty => ID == "EMPTY";

	public bool IsWide => ServiceLocator.Instance.GetService<IDecksService>().GetByNumber(m_deck).IsSlotSellThru(m_slot);

	public int Deck => m_deck;

	public int Slot => m_slot;

	public string ID { get; set; }

	public DateTime? ReturnDate { get; set; }

	public bool Excluded { get; set; }

	public int StuckCount { get; set; }

	public MerchFlags Flags { get; set; }

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		Location location = (Location)obj;
		if (location.Deck == Deck)
		{
			return location.Slot == Slot;
		}
		return false;
	}

	public override string ToString()
	{
		return m_stringRep;
	}

	public override int GetHashCode()
	{
		return 101 * m_deck + m_slot;
	}

	internal Location(int deck, int slot)
	{
		m_deck = deck;
		m_slot = slot;
		ID = "EMPTY";
		ReturnDate = null;
		Excluded = false;
		StuckCount = 0;
		Flags = MerchFlags.None;
		m_stringRep = $"Deck = {m_deck} Slot = {m_slot}";
	}
}
