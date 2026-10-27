using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class Repository
{
	public long ID { get; set; }

	public DateTime CreatedOn { get; set; }

	public DateTime? ModifiedOn { get; set; }

	public string CreatedBy { get; set; }

	public string ModifiedBy { get; set; }

	public string System { get; set; }

	public List<Identifier> Logs { get; set; }

	public List<Identifier> Files { get; set; }

	public List<Identifier> Groups { get; set; }
}
