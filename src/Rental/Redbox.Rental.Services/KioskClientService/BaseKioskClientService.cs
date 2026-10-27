using Redbox.Core;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService;

public class BaseKioskClientService : BaseClientService
{
	protected string KioskClientServiceBaseUrl => ServiceLocator.Instance.GetService<IConfiguration>()?.KioskClientServiceBaseUrl;
}
