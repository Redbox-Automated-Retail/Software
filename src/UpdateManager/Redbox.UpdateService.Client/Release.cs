using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class Release
{
	public long ID { get; set; }

	public string Name { get; set; }

	public List<Identifier> Labels { get; set; }
}
