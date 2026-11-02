using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class Label
{
	public long ID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Name { get; set; } = null!;

	public Identifier Hash { get; set; } = null!;

	public Identifier Repository { get; set; } = null!;

	public List<Identifier> Stores { get; set; } = null!;
}
