using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateService.Model;

public class RepositoryDiffWrapper
{
	public string O { get; set; }

	public string I { get; set; }

	public List<Dictionary<string, string>> U { get; set; }

	public List<ClientRepositoryDataDTO> A { get; set; }

	public List<string> D { get; set; }

	public RepositoryDiffWrapper Copy()
	{
		return new RepositoryDiffWrapper
		{
			O = string.Copy(O),
			I = string.Copy(I),
			A = A.Copy(),
			D = D.ToList(),
			U = U.Select((Dictionary<string, string> each) => new Dictionary<string, string>(each)).ToList()
		};
	}
}
