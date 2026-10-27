using Redbox.Core;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.UpdateClientService;

public class BaseUpdateClientService : BaseClientService
{
	protected string UpdateClientServiceBaseUrl => ServiceLocator.Instance.GetService<IConfiguration>()?.UpdateClientServiceBaseUrl;
}
