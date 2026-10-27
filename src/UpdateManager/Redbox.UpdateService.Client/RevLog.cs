using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class RevLog
{
	public long ID { get; set; }

	public string Hash { get; set; }

	public string Name { get; set; }

	public List<Patch> ChangeSet { get; set; }

	public DeploymentStatus DeploymentStatus { get; set; }

	public DateTime CreatedOn { get; set; }
}
