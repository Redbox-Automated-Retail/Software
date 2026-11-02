using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class Repository
{
	public long ID { get; set; }

	public DateTime CreatedOn { get; set; }

	public DateTime? ModifiedOn { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string CreatedBy { get; set; } = null!;

	public string ModifiedBy { get; set; } = null!;

	public string System { get; set; } = null!;

	public List<Identifier> Logs { get; set; } = null!;

	public List<Identifier> Files { get; set; } = null!;

	public List<Identifier> Groups { get; set; } = null!;
}
