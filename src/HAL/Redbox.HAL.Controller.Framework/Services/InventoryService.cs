using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.Configuration;

namespace Redbox.HAL.Controller.Framework.Services;

public sealed class InventoryService : IConfigurationObserver, IInventoryService
{
	private readonly List<IInventoryObserver> Observers = new List<IInventoryObserver>();

	private readonly IDataTable<ILocation> Table;

	private readonly string EmptyStuckLogFile;

	private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

	private ErrorCodes m_storeState;

	private ILocation[][] LocationInfo;

	public bool IsTrackingEmptyStuck => ControllerConfiguration.Instance.TrackProblemLocations;

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[InventoryService] Notify configuration load");
		IEmptySearchPatternService service = ServiceLocator.Instance.GetService<IEmptySearchPatternService>();
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			service.AddObserver(new VmzExcludePolicy());
		}
		else
		{
			service.AddObserver(new QlmExcludePolicy());
		}
		using (new WithWriteLock(Lock))
		{
			InitializeUnderLock(new ErrorList());
		}
		if (m_storeState == ErrorCodes.Success)
		{
			Observers.ForEach(delegate(IInventoryObserver each)
			{
				each.OnInventoryInitialize();
			});
		}
	}

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[InventoryService] Configuration change end.");
		int num = 0;
		using (new WithWriteLock(Lock))
		{
			IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
			List<ILocation> updates = new List<ILocation>();
			using (new DisposeableList<ILocation>(updates))
			{
				service.ForAllDecksDo(delegate(IDeck deck)
				{
					int number = deck.Number;
					if (LocationInfo[number - 1].Length == deck.NumberOfSlots)
					{
						deck.Quadrants.ForEach(delegate(IQuadrant q)
						{
							for (int i = q.Slots.Start; i <= q.Slots.End; i++)
							{
								ILocation underLock = GetUnderLock(deck.Number, i);
								if (underLock.Excluded != q.IsExcluded)
								{
									ResetLocation(underLock);
									underLock.Excluded = q.IsExcluded;
									updates.Add(underLock);
								}
							}
						});
					}
					return true;
				});
				if (updates.Count > 0)
				{
					num = updates.Count;
					Table.Update(updates);
				}
			}
		}
		if (num > 0)
		{
			Observers.ForEach(delegate(IInventoryObserver each)
			{
				each.OnInventoryChange();
			});
		}
		LogHelper.Instance.Log("[InventoryService] The service {0} configured to track problem locations.", IsTrackingEmptyStuck ? "is" : "is not");
	}

	public void AddObserver(IInventoryObserver o)
	{
		Observers.Add(o);
	}

	public bool MachineIsFull()
	{
		int machineEmptyCount = GetMachineEmptyCount();
		bool flag = machineEmptyCount <= ControllerConfiguration.Instance.ReturnSlotBuffer;
		if (flag)
		{
			LogHelper.Instance.WithContext(LogEntryType.Info, "The machine is full; there is {0} empty slot(s).", machineEmptyCount);
		}
		return flag;
	}

	public bool EmptyCountExceeds(int numberOfEmpty)
	{
		return GetMachineEmptyCount() > numberOfEmpty;
	}

	public ILocation Lookup(string id)
	{
		using (new WithReadLock(Lock))
		{
			ILocation location = LookupUnderLock(id);
			if (location == null)
			{
				string msg = $"LOOKUP of item {id} returned NOT FOUND.";
				LogHelper.Instance.WithContext(LogEntryType.Error, msg);
				ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext().ContextLog.WriteFormatted(msg);
			}
			return location;
		}
	}

	public ILocation Get(int deck, int slot)
	{
		using (new WithReadLock(Lock))
		{
			return GetUnderLock(deck, slot);
		}
	}

	public bool Reset(ILocation loc)
	{
		using (new WithWriteLock(Lock))
		{
			ResetLocation(loc);
			return Table.Update(loc);
		}
	}

	public bool Reset(IList<ILocation> locs)
	{
		using (new WithWriteLock(Lock))
		{
			foreach (ILocation loc in locs)
			{
				ResetLocation(loc);
			}
			return Table.Update(locs);
		}
	}

	public bool Save(ILocation location)
	{
		using (new WithWriteLock(Lock))
		{
			return Table.Update(location);
		}
	}

	public bool Save(IList<ILocation> locations)
	{
		using (new WithWriteLock(Lock))
		{
			return Table.Update(locations);
		}
	}

	public int GetExcludedSlotsCount()
	{
		return ComputeCount((ILocation loc) => loc.Excluded);
	}

	public List<ILocation> GetExcludedSlots()
	{
		List<ILocation> list = ComputeList((ILocation loc) => loc.Excluded);
		list.Sort((ILocation x, ILocation y) => (x.Deck == y.Deck) ? x.Slot.CompareTo(y.Slot) : x.Deck.CompareTo(y.Deck));
		return list;
	}

	public List<ILocation> GetEmptySlots()
	{
		return ComputeList((ILocation loc) => !loc.Excluded && loc.ID == "EMPTY");
	}

	public List<ILocation> GetUnknowns()
	{
		return ComputeList((ILocation loc) => !loc.Excluded && loc.ID == "UNKNOWN");
	}

	public bool IsBarcodeDuplicate(string id, out ILocation original)
	{
		new DuplicateSearchResult().Original = null;
		original = null;
		if (InventoryConstants.IsKnownInventoryToken(id))
		{
			return false;
		}
		using (new WithReadLock(Lock))
		{
			original = LookupUnderLock(id);
			return original != null;
		}
	}

	public int GetMachineEmptyCount()
	{
		return ComputeCount((ILocation loc) => !loc.Excluded && loc.ID == "EMPTY");
	}

	public bool UpdateEmptyStuck(ILocation location)
	{
		if (!IsTrackingEmptyStuck)
		{
			LogHelper.Instance.Log("[UpdateEmptyStuck] Service is not tracking.");
			return false;
		}
		using (new WithWriteLock(Lock))
		{
			int stuckCount = location.StuckCount + 1;
			location.StuckCount = stuckCount;
			LogFormattedMessage("Incrementing Deck = {0} Slot = {1}; current stuck count = {2}", location.Deck, location.Slot, location.StuckCount);
			if (location.StuckCount >= ControllerConfiguration.Instance.MarkLocationUnknownThreshold && location.ID != "UNKNOWN")
			{
				LogFormattedMessage(" ** Error threshold met: set inventory to {0} at Deck = {1} Slot = {2}", "UNKNOWN", location.Deck, location.Slot);
				location.ID = "UNKNOWN";
			}
			return Table.Update(location);
		}
	}

	public bool IsStuck(ILocation location)
	{
		if (!IsTrackingEmptyStuck)
		{
			return false;
		}
		return location.StuckCount > 0;
	}

	public void DumpStore(TextWriter writer)
	{
		using (new WithReadLock(Lock))
		{
			List<ILocation> list = Table.LoadEntries();
			writer.WriteLine("-- {0} Dump inventory store ( {1} total items )--", DateTime.Now.ToLongDateString(), list.Count);
			using (new DisposeableList<ILocation>(list))
			{
				foreach (ILocation item in list)
				{
					writer.WriteLine("{0} ID = {1} ReturnTime = {2} Excluded = {3} StuckCount = {4} MerchFlags = {5}", item.ToString(), item.ID, item.ReturnDate.HasValue ? item.ReturnDate.ToString() : "NONE", item.Excluded, item.StuckCount, item.Flags);
				}
			}
		}
	}

	public bool MarkDeckInventory(IDeck deck, string newMatrix)
	{
		List<ILocation> list = new List<ILocation>();
		using (new WithWriteLock(Lock))
		{
			using (new DisposeableList<ILocation>(list))
			{
				for (int i = 1; i <= deck.NumberOfSlots; i++)
				{
					ILocation underLock = GetUnderLock(deck.Number, i);
					underLock.ID = newMatrix;
					list.Add(underLock);
				}
				return Table.Update(list);
			}
		}
	}

	public List<int> SwapEmptyWith(IDeck deck, string id, MerchFlags flags, IRange<int> range)
	{
		List<int> list = new List<int>();
		List<ILocation> list2 = new List<ILocation>();
		using (new WithWriteLock(Lock))
		{
			using (new DisposeableList<ILocation>(list2))
			{
				for (int i = range.Start; i <= range.End; i++)
				{
					ILocation underLock = GetUnderLock(deck.Number, i);
					if (underLock.ID == "EMPTY" && !underLock.Excluded)
					{
						underLock.ID = id;
						underLock.Flags = flags;
						list2.Add(underLock);
						list.Add(i);
					}
				}
				Table.Update(list2);
				return list;
			}
		}
	}

	public bool ResetAndMark(IDeck deck, string id)
	{
		List<ILocation> list = new List<ILocation>();
		using (new DisposeableList<ILocation>(list))
		{
			using (new WithWriteLock(Lock))
			{
				for (int i = 1; i <= deck.NumberOfSlots; i++)
				{
					ILocation underLock = GetUnderLock(deck.Number, i);
					ResetLocation(underLock);
					underLock.ID = id;
					list.Add(underLock);
				}
				return Table.Update(list);
			}
		}
	}

	public void GetState(XmlTextWriter writer)
	{
		using (new WithReadLock(Lock))
		{
			ForeachUnderLock(delegate(ILocation loc)
			{
				writer.WriteStartElement("item");
				writer.WriteAttributeString("deck", XmlConvert.ToString(loc.Deck));
				writer.WriteAttributeString("slot", XmlConvert.ToString(loc.Slot));
				writer.WriteAttributeString("id", loc.ID);
				writer.WriteAttributeString("ReturnTime", (!loc.ReturnDate.HasValue) ? "NONE" : loc.ReturnDate.Value.ToString());
				writer.WriteAttributeString("excluded", loc.Excluded.ToString());
				writer.WriteAttributeString("emptyStuckCount", loc.StuckCount.ToString());
				writer.WriteAttributeString("merchFlags", loc.Flags.ToString());
				writer.WriteEndElement();
				return true;
			}, excludeQLM: false);
		}
	}

	public void ResetState(XmlDocument xmlDocument, ErrorList errors)
	{
		List<ILocation> inventory = new List<ILocation>();
		using (new WithUpgradeableReadLock(Lock))
		{
			using (new DisposeableList<ILocation>(inventory))
			{
				try
				{
					XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("item");
					if (xmlNodeList == null)
					{
						errors.Add(Error.NewError("I001", "Invalid document.", "The specified document has no nodes."));
						return;
					}
					foreach (XmlNode item in xmlNodeList)
					{
						ILocation location = FromXML(item, errors);
						if (location == null)
						{
							errors.Add(Error.NewError("I002", "Load error.", "Unable to load the deck from the XML."));
							return;
						}
						inventory.Add(location);
					}
					using (new WithWriteLock(Lock))
					{
						ServiceLocator.Instance.GetService<IDecksService>().ForAllDecksDo(delegate(IDeck deck)
						{
							if (deck.Number - 1 < 0 || deck.Number - 1 > LocationInfo.GetLength(0))
							{
								errors.Add(Error.NewError("P004", $"The deck {deck.Number} was not present in the deck configuration.", "Ensure the inventory is valid."));
								return false;
							}
							List<ILocation> list = inventory.FindAll((ILocation each) => each.Deck == deck.Number);
							if (list.Count != deck.NumberOfSlots)
							{
								errors.Add(Error.NewError("P004", $"The slot count {list.Count} did not match the deck slot count for deck {deck.Number} ({deck.NumberOfSlots} slots).", "Ensure the inventory is valid."));
								LogHelper.Instance.Log($"Deck {deck.Number} expected {deck.NumberOfSlots} slots to be imported but received {list.Count}.", LogEntryType.Error);
								return true;
							}
							Table.Update(list);
							LocationInfo[deck.Number - 1] = Sort(list);
							return true;
						});
					}
				}
				catch (Exception e)
				{
					errors.Add(Error.NewError("P999", "An unhandled exception was raised in InventoryService ResetState.", e));
					LogHelper.Instance.Log("RESET INVENTORY Failed to import inventory.", e, LogEntryType.Error);
				}
			}
		}
	}

	public void Rebuild(ErrorList errors)
	{
		m_storeState = ErrorCodes.StoreError;
		using (new WithWriteLock(Lock))
		{
			if (!Table.Recreate(errors))
			{
				LogHelper.Instance.Log("[InventoryService] Failed to recreate inventory table.");
				errors.DumpToLog();
				return;
			}
			InitializeUnderLock(errors);
		}
		if (m_storeState == ErrorCodes.Success)
		{
			Observers.ForEach(delegate(IInventoryObserver each)
			{
				each.OnInventoryRebuild();
			});
		}
	}

	public void InstallFromLegacy(ErrorList errors, TextWriter writer)
	{
		int platterSlots = new GampHelper().GetPlatterSlots();
		int registrySlotCount = ControllerConfiguration.Instance.RegistrySlotCount;
		ITableTypeFactory factory = ServiceLocator.Instance.GetService<ITableTypeFactory>();
		if (platterSlots == registrySlotCount)
		{
			List<ILocation> locs = new List<ILocation>();
			ServiceLocator.Instance.GetService<IDecksService>().ForAllDecksDo(delegate(IDeck deck)
			{
				for (int i = 1; i <= deck.NumberOfSlots; i++)
				{
					locs.Add(factory.NewLocation(deck.Number, i));
				}
				return true;
			});
			if (!Table.Insert(locs))
			{
				errors.Add(Error.NewError("I055", "Database insert failed.", "Failed to insert entries into inventory table."));
			}
		}
		else
		{
			writer.WriteLine("The machine is configured for {0} slots; the gamp data says there are {1} slots - mark db UNKNOWN with {2} slots", registrySlotCount, platterSlots, registrySlotCount);
		}
	}

	public ErrorCodes CheckIntegrity()
	{
		return CheckIntegrity(testStore: false);
	}

	public ErrorCodes CheckIntegrity(bool testStore)
	{
		if (!ControllerConfiguration.Instance.EnableInventoryDatabaseCheck)
		{
			return ErrorCodes.Success;
		}
		if (!testStore)
		{
			return m_storeState;
		}
		using (new WithReadLock(Lock))
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			List<ILocation> inventory = Table.LoadEntries();
			using (new DisposeableList<ILocation>(inventory))
			{
				int total = 0;
				ServiceLocator.Instance.GetService<IDecksService>().ForAllDecksDo(delegate(IDeck deck)
				{
					List<ILocation> list = inventory.FindAll((ILocation each) => each.Deck == deck.Number);
					if (list.Count != deck.NumberOfSlots)
					{
						LogHelper.Instance.WithContext(LogEntryType.Error, "[InventoryService] Integrity mismatch: Found {0} entries for deck {1} ( # slots = {2} ) ", list.Count, deck.Number, deck.NumberOfSlots);
					}
					else
					{
						total += deck.NumberOfSlots;
					}
					return true;
				});
				executionTimer.Stop();
				LogHelper.Instance.Log("[CheckStoreIntegrity] Execution time = {0}ms", executionTimer.ElapsedMilliseconds);
				return (total != inventory.Count) ? ErrorCodes.StoreError : ErrorCodes.Success;
			}
		}
	}

	internal InventoryService()
	{
		IDataTableService service = ServiceLocator.Instance.GetService<IDataTableService>();
		Table = service.GetTable<ILocation>();
		string path = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().CreateSubpath("ErrorLogs");
		EmptyStuckLogFile = Path.Combine(path, "ErrorLocationsHistory.txt");
		ControllerConfiguration.Instance.AddObserver(this);
	}

	private void LogFormattedMessage(string format, params object[] parms)
	{
		if (!IsTrackingEmptyStuck)
		{
			return;
		}
		string arg = string.Format(format, parms);
		try
		{
			using StreamWriter streamWriter = new StreamWriter(EmptyStuckLogFile, append: true);
			streamWriter.WriteLine("{0} {1} {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), arg);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[InventoryService] Failed to write to empty/stuck log.", e);
		}
	}

	private List<ILocation> ComputeList(Predicate<ILocation> predicate)
	{
		return ComputeList(predicate, excludeQLM: true);
	}

	private List<ILocation> ComputeList(Predicate<ILocation> predicate, bool excludeQLM)
	{
		using (new WithReadLock(Lock))
		{
			List<ILocation> rv = new List<ILocation>();
			ForeachUnderLock(delegate(ILocation location)
			{
				if (predicate(location))
				{
					rv.Add(location);
				}
				return true;
			}, excludeQLM);
			return rv;
		}
	}

	private int ComputeCount(Predicate<ILocation> predicate)
	{
		return ComputeCount(predicate, excludeQLM: true);
	}

	private int ComputeCount(Predicate<ILocation> predicate, bool excludeQLM)
	{
		using (new WithReadLock(Lock))
		{
			int count = 0;
			ForeachUnderLock(delegate(ILocation location)
			{
				if (predicate(location))
				{
					int num = count + 1;
					count = num;
				}
				return true;
			}, excludeQLM);
			return count;
		}
	}

	private void ForeachUnderLock(Predicate<ILocation> action, bool excludeQLM)
	{
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		int length = LocationInfo.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			IDeck byNumber = service.GetByNumber(i + 1);
			if (excludeQLM && byNumber.IsQlm)
			{
				continue;
			}
			int num = LocationInfo[i].Length;
			for (int j = 0; j < num; j++)
			{
				ILocation obj = LocationInfo[i][j];
				if (!action(obj))
				{
					return;
				}
			}
		}
	}

	private ILocation LookupUnderLock(string id)
	{
		ILocation location = null;
		ForeachUnderLock(delegate(ILocation loc)
		{
			if (loc.ID == id)
			{
				location = loc;
				return false;
			}
			return true;
		}, excludeQLM: true);
		return location;
	}

	private void InitializeUnderLock(ErrorList errors)
	{
		m_storeState = ErrorCodes.Success;
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		try
		{
			List<ILocation> inventory = Table.LoadEntries();
			using (new DisposeableList<ILocation>(inventory))
			{
				LocationInfo = new ILocation[service.DeckCount][];
				service.ForAllDecksDo(delegate(IDeck deck)
				{
					List<ILocation> list = inventory.FindAll((ILocation each) => each.Deck == deck.Number);
					if (list.Count == deck.NumberOfSlots)
					{
						LogHelper.Instance.Log("[InventoryService] Loaded {0} entries for deck {1} ( # slots = {2} )", list.Count, deck.Number, deck.NumberOfSlots);
						LocationInfo[deck.Number - 1] = Sort(list);
						return true;
					}
					if (list.Count > 0)
					{
						LogHelper.Instance.Log(LogEntryType.Error, "[InventoryService] LoadInventory: res.count = {0}; deck count = {1}; deleting deck & initializing to UNKNOWN", list.Count, deck.NumberOfSlots);
						if (!Table.Delete(list))
						{
							m_storeState = ErrorCodes.StoreError;
							LogHelper.Instance.Log("[InventoryService] Delete returned false.");
							return false;
						}
						list.Clear();
					}
					ILocation[] array = InitializeDeck(deck);
					if (array != null)
					{
						LocationInfo[deck.Number - 1] = array;
						return true;
					}
					m_storeState = ErrorCodes.StoreError;
					return false;
				});
			}
			LogHelper.Instance.Log("[InventoryService] The service {0} configured to track problem locations.", IsTrackingEmptyStuck ? "is" : "is not");
			LogHelper.Instance.Log("[InventoryService] Store integrity -> {0}", (m_storeState == ErrorCodes.Success) ? "OK" : "CORRUPT");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[InventoryService] Initialize under lock caught an exception.", e);
			m_storeState = ErrorCodes.StoreError;
		}
	}

	private ILocation[] InitializeDeck(IDeck deck)
	{
		ITableTypeFactory service = ServiceLocator.Instance.GetService<ITableTypeFactory>();
		List<ILocation> list = new List<ILocation>();
		using (new DisposeableList<ILocation>(list))
		{
			for (int i = 1; i <= deck.NumberOfSlots; i++)
			{
				ILocation location = service.NewLocation(deck.Number, i);
				location.ID = "UNKNOWN";
				list.Add(location);
			}
			if (!Table.Insert(list))
			{
				LogHelper.Instance.Log("[InventoryService] Insert of new locations returned false.");
				return null;
			}
			return Sort(list);
		}
	}

	private ILocation[] Sort(List<ILocation> deckEntries)
	{
		deckEntries.Sort((ILocation x, ILocation y) => x.Slot.CompareTo(y.Slot));
		return deckEntries.ToArray();
	}

	private ILocation FromXML(XmlNode node, ErrorList errors)
	{
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		int? attributeValue = node.GetAttributeValue<int?>("deck", null);
		int? attributeValue2 = node.GetAttributeValue<int?>("slot", null);
		string attributeValue3 = node.GetAttributeValue<string>("id", null);
		string attributeValue4 = node.GetAttributeValue<string>("ReturnTime", null);
		DateTime? returnTime = null;
		if (!string.IsNullOrEmpty(attributeValue4) && !attributeValue4.Equals("NONE"))
		{
			try
			{
				returnTime = DateTime.Parse(attributeValue4);
			}
			catch (Exception)
			{
				LogHelper.Instance.Log($"Unable to parse date time {attributeValue4}", LogEntryType.Error);
				returnTime = null;
			}
		}
		bool attributeValue5 = node.GetAttributeValue("excluded", defaultValue: false);
		int attributeValue6 = node.GetAttributeValue("emptyStuckCount", 0);
		MerchFlags flags = Enum<MerchFlags>.ParseIgnoringCase(node.GetAttributeValue("merchFlags", "None"), MerchFlags.None);
		if (!attributeValue.HasValue || !attributeValue2.HasValue || string.IsNullOrEmpty(attributeValue3))
		{
			errors.Add(Error.NewError("P004", "A valid deck, slot, and id must be specified for each item element.", "Add a valid item element with a deck, slot, and id attributes."));
			LogHelper.Instance.Log($"Attempt to import invalid item. XmlNode: {node.OuterXml}", LogEntryType.Error);
			return null;
		}
		IDeck byNumber = service.GetByNumber(attributeValue.Value);
		if (byNumber == null)
		{
			errors.Add(Error.NewError("P003", $"The deck value must be between 1 and {service.DeckCount}.", "Ensure the deck attribute value is within the valid range."));
			LogHelper.Instance.Log($"Deck {attributeValue} doesn't match any decks in the configuration.", LogEntryType.Error);
			return null;
		}
		if (attributeValue2 < 1 || byNumber.NumberOfSlots < attributeValue2)
		{
			errors.Add(Error.NewError("P003", $"The slot must be between 1 and {byNumber.NumberOfSlots}.", "Ensure the slot number is within the valid range."));
			LogHelper.Instance.Log($"Deck {byNumber.Number} expects a value between 1 and {byNumber.NumberOfSlots}, received {attributeValue2}", LogEntryType.Error);
			return null;
		}
		return ServiceLocator.Instance.GetService<ITableTypeFactory>().NewLocation(attributeValue.Value, attributeValue2.Value, attributeValue3, returnTime, attributeValue5, attributeValue6, flags);
	}

	private ILocation GetUnderLock(int deck, int slot)
	{
		return LocationInfo[deck - 1][slot - 1];
	}

	private void ResetLocation(ILocation loc)
	{
		loc.ReturnDate = null;
		loc.ID = "EMPTY";
		loc.StuckCount = 0;
		loc.Flags = MerchFlags.None;
	}
}
