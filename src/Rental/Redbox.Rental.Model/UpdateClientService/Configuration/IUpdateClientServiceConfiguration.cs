using System.Threading.Tasks;

namespace Redbox.Rental.Model.UpdateClientService.Configuration;

public interface IUpdateClientServiceConfiguration
{
	Task<GetConfigurationChangesResponse> GetConfigurationChanges();
}
