using System;
using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IEmptySearchResult : IDisposable
{
	IList<ILocation> EmptyLocations { get; }

	int FoundEmpty { get; }
}
