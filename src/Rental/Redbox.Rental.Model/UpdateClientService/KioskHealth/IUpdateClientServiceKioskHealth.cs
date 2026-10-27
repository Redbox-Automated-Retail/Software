using System.Threading.Tasks;

namespace Redbox.Rental.Model.UpdateClientService.KioskHealth;

public interface IUpdateClientServiceKioskHealth
{
	Task<PingResponse> SendPing(IPingRequest pingRequest);
}
