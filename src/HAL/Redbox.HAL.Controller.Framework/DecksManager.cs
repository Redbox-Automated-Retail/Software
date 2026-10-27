using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DecksManager : IDecksService
{
	private readonly List<IDeck> Decks = new List<IDeck>();

	public IDeck First => Decks[0];

	public IDeck Last => Decks[Decks.Count - 1];

	public int DeckCount => Decks.Count;

	public IDeck QlmDeck
	{
		get
		{
			IDeck qlmDeck = null;
			ForAllDecksDo(delegate(IDeck d)
			{
				if (d.IsQlm)
				{
					qlmDeck = d;
					return false;
				}
				return true;
			});
			return qlmDeck;
		}
	}

	public IDeck GetByNumber(int number)
	{
		return Decks.Find((IDeck each) => each.Number == number);
	}

	public IDeck GetFrom(ILocation location)
	{
		return Decks.Find((IDeck each) => each.Number == location.Deck);
	}

	public bool IsValidLocation(ILocation loc)
	{
		Deck deck = GetByNumber(loc.Deck) as Deck;
		if (!(deck == null))
		{
			return deck.IsSlotValid(loc.Slot);
		}
		return false;
	}

	public void Add(IDeck deck)
	{
		if (deck.IsQlm && QlmDeck != null)
		{
			throw new ArgumentException("Only one QLM deck is allowed per configuration.");
		}
		Decks.Add(deck);
	}

	public void ForAllDecksDo(Predicate<IDeck> predicate)
	{
		foreach (Deck deck in Decks)
		{
			if (!predicate(deck))
			{
				break;
			}
		}
	}

	public void ForAllReverseDecksDo(Predicate<IDeck> predicate)
	{
		int num = Decks.Count;
		while (num >= 1)
		{
			IDeck obj = Decks[num - 1];
			if (predicate(obj))
			{
				num--;
				continue;
			}
			break;
		}
	}

	internal void Clear()
	{
		Decks.Clear();
	}

	internal void Initialize(XmlNode decksNode)
	{
		Decks.Clear();
		foreach (XmlNode childNode in decksNode.ChildNodes)
		{
			Deck item = Deck.FromXmlNode(childNode);
			Decks.Add(item);
		}
		Decks.Sort((IDeck x, IDeck y) => x.Number.CompareTo(y.Number));
		LogHelper.Instance.Log("[DecksManager] Kiosk configured for {0} decks.", Decks.Count);
	}

	internal void SaveConfiguration(XmlDocument document)
	{
		using (StringWriter stringWriter = new StringWriter())
		{
			XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter)
			{
				Formatting = Formatting.Indented
			};
			foreach (IDeck deck in Decks)
			{
				(deck as Deck).ToXmlWriter(xmlTextWriter);
			}
			xmlTextWriter.Flush();
			document.DocumentElement.SelectSingleNodeAndSetInnerXml("Controller/Decks", stringWriter);
		}
		LogHelper.Instance.Log("Completed decks save.", LogEntryType.Info);
	}

	internal void ToPropertyXml(XmlWriter writer)
	{
		foreach (Deck deck in Decks)
		{
			deck.ToXmlWriter(writer);
		}
	}

	internal void UpdateFromPropertyXml(XmlNode propertyNode)
	{
		XmlNodeList xmlNodeList = propertyNode.SelectNodes("Deck");
		if (xmlNodeList == null)
		{
			return;
		}
		List<Deck> list = new List<Deck>();
		using (new DisposeableList<Deck>(list))
		{
			if (xmlNodeList.Count != Decks.Count)
			{
				LogHelper.Instance.Log("The deck configuration has changed in count; this is unsupported.");
				return;
			}
			foreach (XmlNode item in xmlNodeList)
			{
				Deck deck = Deck.FromXmlNode(item);
				Deck deck2 = GetByNumber(deck.Number) as Deck;
				if (deck != deck2)
				{
					list.Add(deck);
				}
			}
			if (list.Count == 0)
			{
				LogHelper.Instance.Log("Update decks: there were no changes.");
				return;
			}
			try
			{
				LogHelper.Instance.Log("Update decks info:");
				foreach (Deck item2 in list)
				{
					Deck deck3 = GetByNumber(item2.Number) as Deck;
					LogHelper.Instance.Log(" Deck Number {0} changes", deck3.Number);
					LogHelper.Instance.Log("  Y-offset: new = {0} old = {1}", item2.YOffset, deck3.YOffset);
					if (item2.Quadrants.Count == deck3.Quadrants.Count)
					{
						for (int i = 0; i < item2.Quadrants.Count; i++)
						{
							LogHelper.Instance.Log("   Quadrant {0} offsets: new = {1} ( excluded = {2} ) old = {3} ( excluded = {4} )", i + 1, item2.Quadrants[i].Offset, item2.Quadrants[i].IsExcluded, deck3.Quadrants[i].Offset, deck3.Quadrants[i].IsExcluded);
						}
						deck3.UpdateFrom(item2);
					}
					else
					{
						LogHelper.Instance.Log(LogEntryType.Error, "[DecksManager] Unsupported configuration change: slot count ( old count = {0} new count = {1}", deck3.NumberOfSlots, item2.NumberOfSlots);
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("There was an exception during deck update.", e);
			}
		}
	}

	internal DecksManager()
	{
	}
}
