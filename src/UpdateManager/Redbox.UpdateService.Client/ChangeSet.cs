using System.Collections.Generic;
using Redbox.Compression;

namespace Redbox.UpdateService.Client;

public class ChangeSet
{
	public CompressionType CompressionType { get; set; }

	public string Name { get; set; }

	public string Label { get; set; }

	public string Head { get; set; }

	public string Url { get; set; }

	public bool IsArchive { get; set; }

	public List<ChangeSetItem> Items { get; set; }

	public List<string> Revisions { get; set; }

	public string ContentHash { get; set; }
}
