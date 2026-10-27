using System;
using System.Threading.Tasks;

namespace Redbox.Rental.Model.KioskClientService.Session;

public interface IKioskClientServiceSession
{
	bool Create(Guid KioskSessionId);

	Task<bool> Start(SessionType sessionType);

	Task<bool> End();
}
