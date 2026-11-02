using System.Collections.Generic;
using Redbox.Compression;

namespace Redbox.UpdateService.Client;

public class ChangeSet
{
	public CompressionType CompressionType { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Name { get; set; } = null!;

	public string Label { get; set; } = null!;

	public string Head { get; set; } = null!;

	public string Url { get; set; } = null!;

	public bool IsArchive { get; set; }

	public List<ChangeSetItem> Items { get; set; } = null!;

	public List<string> Revisions { get; set; } = null!;

	public string ContentHash { get; set; } = null!;
}
