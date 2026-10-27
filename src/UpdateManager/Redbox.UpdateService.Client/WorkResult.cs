using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class WorkResult
{
	public long ID { get; set; }

	public Identifier Store { get; set; }

	public Identifier Script { get; set; }

	public Dictionary<object, object> Result { get; set; }
}
