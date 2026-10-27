using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class CountersDescriptor : IDataTableDescriptor
{
	public string Source { get; private set; }

	public bool ExclusiveReadWrite { get; private set; }

	public bool SupportsPooling { get; private set; }

	public bool UseTransaction => false;

	internal CountersDescriptor(string dataPath, bool exclusive)
	{
		Source = Path.Combine(dataPath, "HALCounters.vdb3");
		ExclusiveReadWrite = exclusive;
		SupportsPooling = !exclusive;
	}
}
