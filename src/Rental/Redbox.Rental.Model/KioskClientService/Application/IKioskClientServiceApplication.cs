using System.Threading.Tasks;

namespace Redbox.Rental.Model.KioskClientService.Application;

public interface IKioskClientServiceApplication
{
	Task<bool> Start();

	Task<bool> End();
}
