using System.Collections.Generic;

namespace Redbox.Rental.Model;

public class InCartOfferTemplate
{
	public string Name { get; set; }

	public object ProgramName { get; set; }

	public object VariablesUsed { get; set; }

	public List<Dictionary<string, object>> Content { get; set; }
}
