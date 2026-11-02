using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class RepositoryEntry
{
	public long ID { get; set; }

	public long Size { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Hash { get; set; } = null!;

	public bool Deleted { get; set; }

	public string Name { get; set; } = null!;

	public string Path { get; set; } = null!;

	public string System { get; set; } = null!;

	public string CreatedBy { get; set; } = null!;

	public string ModifiedBy { get; set; } = null!;

	public string DisplayName { get; set; } = null!;

	public DateTime CreatedOn { get; set; }

	public DateTime? ModifiedOn { get; set; }

	public List<Identifier> Patches { get; set; } = null!;
}
