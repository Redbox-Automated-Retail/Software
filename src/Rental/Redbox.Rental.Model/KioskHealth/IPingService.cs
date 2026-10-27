using System.Threading.Tasks;
using Redbox.Rental.Model.UpdateClientService.KioskHealth;

namespace Redbox.Rental.Model.KioskHealth;

public interface IPingService
{
	bool Enabled { get; }

	int PingInterval { get; }

	Task<PingResponse> SendPing();
}
