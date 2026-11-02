using System;

namespace Redbox.UpdateService.Model;

public class ClientRepositoryDataDTO : IComparable
{
	// Filled by the constructor, the object initializer or the JSON reader.
	public string R = null!;

	public string I = null!;

	public string S = null!;

	public string A = null!;

	public string H = null!;

	public ClientRepositoryDataDTO()
	{
	}

	public ClientRepositoryDataDTO(ClientRepositoryData crd)
	{
		R = crd.RepositoryName;
		I = ((crd.InTransit != null) ? crd.InTransit.Hash : "0000000000000000000000000000000000000000");
		S = ((crd.Staged != null) ? crd.Staged.Hash : "0000000000000000000000000000000000000000");
		A = ((crd.Active != null) ? crd.Active.Hash : "0000000000000000000000000000000000000000");
		H = ((crd.Head != null) ? crd.Head.Hash : "0000000000000000000000000000000000000000");
	}

	public int CompareTo(object? obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (obj is ClientRepositoryDataDTO clientRepositoryDataDTO)
		{
			return R.CompareTo(clientRepositoryDataDTO.R);
		}
		throw new ArgumentException("Object is not a ClientRepositoryDataDTO");
	}

	public ClientRepositoryDataDTO Copy()
	{
		return new ClientRepositoryDataDTO
		{
			R = string.Copy(R),
			I = string.Copy(I),
			S = string.Copy(S),
			A = string.Copy(A),
			H = string.Copy(H)
		};
	}
}
