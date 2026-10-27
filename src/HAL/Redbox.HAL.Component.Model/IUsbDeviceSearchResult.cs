using System;
using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IUsbDeviceSearchResult : IDisposable
{
	bool Found { get; }

	ErrorList Errors { get; }

	List<IDeviceDescriptor> Matches { get; }
}
