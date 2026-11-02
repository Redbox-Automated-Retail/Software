using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class WorkResult
{
	public long ID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public Identifier Store { get; set; } = null!;

	public Identifier Script { get; set; } = null!;

	public Dictionary<object, object> Result { get; set; } = null!;
}
