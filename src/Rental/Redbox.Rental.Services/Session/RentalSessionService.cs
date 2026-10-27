using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class RentalSessionService : IRentalSessionService
{
	public ISession GetCurrentSession()
	{
		return Session.Instance;
	}
}
