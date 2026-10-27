using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class Label
{
	public long ID { get; set; }

	public string Name { get; set; }

	public Identifier Hash { get; set; }

	public Identifier Repository { get; set; }

	public List<Identifier> Stores { get; set; }
}
