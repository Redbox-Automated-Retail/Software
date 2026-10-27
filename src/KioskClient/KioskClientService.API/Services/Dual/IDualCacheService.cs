using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskClientService.API.Services.Dual;

public interface IDualCacheService
{
	Task Refresh(IDictionary<long, int> source);

	Task Update(IEnumerable<long> expected, IDictionary<long, int> actual);

	Task<IEnumerable<long>> Existing(IEnumerable<long> expected);
}
