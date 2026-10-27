using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class WorkResultPollRequestDTO
{
	public string StoreNumber;

	public long ScriptID;

	public Dictionary<object, object> Results;
}
