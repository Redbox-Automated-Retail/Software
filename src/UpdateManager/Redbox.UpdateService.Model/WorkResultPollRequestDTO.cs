using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class WorkResultPollRequestDTO
{
	// Filled by the object initializer or the JSON reader.
	public string StoreNumber = null!;

	public long ScriptID;

	public Dictionary<object, object> Results = null!;
}
