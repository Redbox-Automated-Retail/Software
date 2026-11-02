using System;

namespace Redbox.UpdateManager.Environment;

public class StoreFileState
{
	// The properties below are filled by the object initializer or the JSON reader.
	public StoreFilePollRequest StoreFilePollRequest { get; set; } = null!;

	public string Destination { get; set; } = null!;

	public FileState State { get; set; }

	public string StateText => Enum.GetName(typeof(FileState), State)!; // State is a FileState

	public DateTime Timestamp { get; set; }
}
