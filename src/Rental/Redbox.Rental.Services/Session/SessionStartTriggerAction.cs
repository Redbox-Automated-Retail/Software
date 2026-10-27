using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class SessionStartTriggerAction : ISessionStartTriggerAction
{
	public string Name { get; set; }

	public bool StartedTimedSession { get; set; }

	public override string ToString()
	{
		return Name ?? "";
	}
}
