using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Redbox.HAL.Component.Model;

public interface IDumpbinService
{
	ILocation PutLocation { get; }

	int Capacity { get; }

	ILocation RotationLocation { get; }

	bool IsFull();

	bool IsBin(ILocation loc);

	int CurrentCount();

	int RemainingSpace();

	bool ClearItems();

	void DumpContents(TextWriter writer);

	IList<IDumpBinInventoryItem> GetBarcodesInBin();

	bool AddBinItem(string matrix);

	bool AddBinItem(IDumpBinInventoryItem item);

	void GetState(XmlTextWriter writer);

	void ResetState(XmlDocument document, ErrorList errors);
}
