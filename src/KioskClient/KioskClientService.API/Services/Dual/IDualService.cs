using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskClientService.API.Services.Dual;

public interface IDualService
{
	Task<RefreshResponse> Refresh(long dualKioskId);

	Task<UpdateTitlesResponse> UpdateTitles(long dualKioskId, IEnumerable<long> titleIds);

	Task<InStockInOtherResponse> InStockInOther(IEnumerable<long> titleIds);
}
