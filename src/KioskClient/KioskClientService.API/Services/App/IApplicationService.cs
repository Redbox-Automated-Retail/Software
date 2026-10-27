using System.Threading.Tasks;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.App;

public interface IApplicationService
{
	Task<ApplicationResponse> Start();

	Task<ApplicationResponse> End();

	Task<ViewShowResponse> ViewChangedEvent(ViewStateChangedEvent e);
}
