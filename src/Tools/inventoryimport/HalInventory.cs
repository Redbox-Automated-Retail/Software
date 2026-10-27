using System;
using System.Collections.Generic;
using Microsoft.Win32;
using Redbox.Core;

namespace InventoryImport;

public class HalInventory
{
	private static readonly HalInventory m_instance = new HalInventory();

	private readonly Dictionary<int, List<string>> m_decks = new Dictionary<int, List<string>>();

	public static HalInventory Instance => m_instance;

	public static void Add(Disc disc)
	{
		string text = disc.Barcode.ToHalBarcode(disc.DiskType);
		LogHelper.Instance.Log("HAL barcode from legacy disc: {0}", text);
		if (!int.TryParse(disc.Deck, out var result))
		{
			LogHelper.Instance.Log("...An invalid deck number was specified: {0}", disc.Deck);
			return;
		}
		if (!Instance.m_decks.ContainsKey(result))
		{
			Instance.m_decks[result] = new List<string>();
		}
		Instance.m_decks[result].Add(text);
	}

	public static void SetInventory()
	{
		try
		{
			foreach (KeyValuePair<int, List<string>> deck in Instance.m_decks)
			{
				string text = $"SOFTWARE\\Redbox\\HAL\\State\\Live\\Decks\\{deck.Key}";
				RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(text);
				if (registryKey == null)
				{
					LogHelper.Instance.Log("...Unable to create registry sub key: {0}", text);
				}
				else
				{
					registryKey.SetValue("Slots", deck.Value.ToArray(), RegistryValueKind.MultiString);
					registryKey.Close();
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised while creating the HAL inventory registry entries.", e);
		}
	}

	private HalInventory()
	{
	}
}
