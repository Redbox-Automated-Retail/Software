using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Redbox.HAL.Component.Model;

public interface IInventoryService
{
	bool IsTrackingEmptyStuck { get; }

	bool MachineIsFull();

	bool EmptyCountExceeds(int numberOfEmpty);

	ILocation Lookup(string id);

	ILocation Get(int deck, int slot);

	bool Reset(ILocation location);

	bool Reset(IList<ILocation> locations);

	bool Save(ILocation location);

	bool Save(IList<ILocation> locations);

	int GetExcludedSlotsCount();

	IList<ILocation> GetExcludedSlots();

	IList<ILocation> GetEmptySlots();

	IList<ILocation> GetUnknowns();

	bool IsBarcodeDuplicate(string id, out ILocation original);

	int GetMachineEmptyCount();

	bool UpdateEmptyStuck(ILocation location);

	bool IsStuck(ILocation loc);

	void DumpStore(TextWriter writer);

	bool MarkDeckInventory(IDeck deck, string newMatrix);

	IList<int> SwapEmptyWith(IDeck deck, string id, MerchFlags flags, IRange<int> slotRange);

	bool ResetAndMark(IDeck deck, string id);

	void GetState(XmlTextWriter writer);

	void ResetState(XmlDocument document, ErrorList errors);
}
