using System.Collections.Generic;

namespace Redbox.Rental.Model;

public interface IBaseResponse
{
	bool Success { get; set; }

	List<Error> Errors { get; set; }

	string HostName { get; }

	int StatusCode { get; set; }
}
