using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class RevLog
{
	public long ID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Hash { get; set; } = null!;

	public string Name { get; set; } = null!;

	public List<Patch> ChangeSet { get; set; } = null!;

	public DeploymentStatus DeploymentStatus { get; set; }

	public DateTime CreatedOn { get; set; }
}
