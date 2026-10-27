namespace Redbox.Rental.Model.Session;

public interface ISessionStartTriggerAction
{
	string Name { get; set; }

	bool StartedTimedSession { get; set; }
}
