using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class DataDescriptor : IDataTableDescriptor
{
	public string Source { get; private set; }

	public bool ExclusiveReadWrite => true;

	public bool SupportsPooling => false;

	public bool UseTransaction => true;

	internal DataDescriptor(string dataPath)
	{
		Source = Path.Combine(dataPath, "HALData.vdb3");
	}
}
