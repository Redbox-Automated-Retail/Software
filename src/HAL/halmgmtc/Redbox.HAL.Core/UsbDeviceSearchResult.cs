using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class UsbDeviceSearchResult : IUsbDeviceSearchResult, IDisposable
{
	private bool Disposed;

	public bool Found
	{
		get
		{
			if (Errors.Count == 0)
			{
				return Matches.Count > 0;
			}
			return false;
		}
	}

	public ErrorList Errors { get; private set; }

	public IList<IDeviceDescriptor> Matches { get; private set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Errors.Clear();
			Matches.Clear();
		}
	}

	internal UsbDeviceSearchResult()
	{
		Errors = new ErrorList();
		Matches = new List<IDeviceDescriptor>();
	}
}
