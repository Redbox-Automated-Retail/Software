using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class NilDumpbinService : IDumpbinService
{
	public ILocation PutLocation => null;

	public int Capacity => 0;

	public ILocation RotationLocation { get; private set; }

	public bool IsFull()
	{
		return true;
	}

	public bool IsBin(ILocation loc)
	{
		return false;
	}

	public int CurrentCount()
	{
		return 0;
	}

	public int RemainingSpace()
	{
		return 0;
	}

	public bool ClearItems()
	{
		return false;
	}

	public void DumpContents(TextWriter writer)
	{
		writer.WriteLine("The dumpbin is not configured.");
	}

	public IList<IDumpBinInventoryItem> GetBarcodesInBin()
	{
		return new List<IDumpBinInventoryItem>();
	}

	public bool AddBinItem(string m)
	{
		return false;
	}

	public bool AddBinItem(IDumpBinInventoryItem i)
	{
		return false;
	}

	public void GetState(XmlTextWriter writer)
	{
	}

	public void ResetState(XmlDocument document, ErrorList errors)
	{
	}

	internal NilDumpbinService()
	{
	}
}
