using System;
using System.Collections.Generic;

namespace UpdateClientService.API.Services.IoT;

public class IoTConnectionAttempt
{
	public DateTime? StartConnectionAttempt { get; set; }

	public DateTime? LatestConnectionAttempt { get; set; }

	public DateTime? Connected { get; set; }

	public DateTime? Disconnected { get; set; }

	public TimeSpan? ConnectionAttemptsDuration
	{
		get
		{
			DateTime? dateTime = (Connected.HasValue ? Connected : LatestConnectionAttempt);
			if (!StartConnectionAttempt.HasValue || !dateTime.HasValue)
			{
				return null;
			}
			return dateTime.Value - StartConnectionAttempt.Value;
		}
	}

	public int ConnectionAttempts { get; set; }

	public Dictionary<string, int> ConnectionExceptions { get; set; }
}
