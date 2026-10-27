using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DumpbinServiceBridge : IDumpbinService, IConfigurationObserver
{
	private IDumpbinService Implementor;

	public ILocation PutLocation => Implementor.PutLocation;

	public int Capacity => Implementor.Capacity;

	public ILocation RotationLocation => Implementor.RotationLocation;

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[DumpbinServiceBridge] Configuration loaded.");
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			Implementor = new DumpbinService();
		}
		else
		{
			Implementor = new NilDumpbinService();
		}
	}

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
	}

	public bool IsFull()
	{
		bool flag = Implementor.IsFull();
		if (flag)
		{
			LogHelper.Instance.WithContext("The dumpbin is full.");
		}
		return flag;
	}

	public bool IsBin(ILocation loc)
	{
		return Implementor.IsBin(loc);
	}

	public int CurrentCount()
	{
		return Implementor.CurrentCount();
	}

	public int RemainingSpace()
	{
		return Implementor.RemainingSpace();
	}

	public bool ClearItems()
	{
		return Implementor.ClearItems();
	}

	public void DumpContents(TextWriter writer)
	{
		Implementor.DumpContents(writer);
	}

	public IList<IDumpBinInventoryItem> GetBarcodesInBin()
	{
		return Implementor.GetBarcodesInBin();
	}

	public bool AddBinItem(string matrix)
	{
		return Implementor.AddBinItem(matrix);
	}

	public bool AddBinItem(IDumpBinInventoryItem item)
	{
		return Implementor.AddBinItem(item);
	}

	public void GetState(XmlTextWriter writer)
	{
		Implementor.GetState(writer);
	}

	public void ResetState(XmlDocument document, ErrorList errors)
	{
		Implementor.ResetState(document, errors);
	}

	internal DumpbinServiceBridge()
	{
		ControllerConfiguration.Instance.AddObserver(this);
	}
}
