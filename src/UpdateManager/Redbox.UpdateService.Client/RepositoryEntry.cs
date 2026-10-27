using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class RepositoryEntry
{
	public long ID { get; set; }

	public long Size { get; set; }

	public string Hash { get; set; }

	public bool Deleted { get; set; }

	public string Name { get; set; }

	public string Path { get; set; }

	public string System { get; set; }

	public string CreatedBy { get; set; }

	public string ModifiedBy { get; set; }

	public string DisplayName { get; set; }

	public DateTime CreatedOn { get; set; }

	public DateTime? ModifiedOn { get; set; }

	public List<Identifier> Patches { get; set; }
}
