using System;
using System.Collections.Generic;
using System.Linq;

namespace UpdateClientService.API.Services.IoT;

public class IoTStatistics : IPersistentData
{
	public List<IoTConnectionAttempt> ConnectionAttempts { get; set; } = new List<IoTConnectionAttempt>();

	public void StartConnectionAttempt()
	{
		IoTConnectionAttempt ioTConnectionAttempt = GetLatest();
		if (ioTConnectionAttempt != null && ioTConnectionAttempt.Connected.HasValue)
		{
			ioTConnectionAttempt = null;
		}
		if (ioTConnectionAttempt == null)
		{
			ioTConnectionAttempt = CreateIoTConnectionAttempt();
		}
		ioTConnectionAttempt.LatestConnectionAttempt = (ioTConnectionAttempt.LatestConnectionAttempt.HasValue ? new DateTime?(DateTime.Now) : ioTConnectionAttempt.StartConnectionAttempt);
		ioTConnectionAttempt.ConnectionAttempts++;
	}

	public void Connected()
	{
		IoTConnectionAttempt latest = GetLatest();
		if (latest != null)
		{
			latest.Connected = DateTime.Now;
		}
	}

	public void Disconnected()
	{
		IoTConnectionAttempt latest = GetLatest();
		if (latest != null && latest.Connected.HasValue)
		{
			latest.Disconnected = DateTime.Now;
		}
	}

	public void AddException(string exceptionMessage)
	{
		IoTConnectionAttempt latest = GetLatest();
		if (latest != null)
		{
			if (latest.ConnectionExceptions == null)
			{
				latest.ConnectionExceptions = new Dictionary<string, int>();
			}
			int value = 0;
			latest.ConnectionExceptions.TryGetValue(exceptionMessage, out value);
			value++;
			latest.ConnectionExceptions[exceptionMessage] = value;
		}
	}

	private IoTConnectionAttempt GetLatest()
	{
		return ConnectionAttempts.FirstOrDefault(delegate(IoTConnectionAttempt x)
		{
			DateTime? startConnectionAttempt = x.StartConnectionAttempt;
			DateTime? dateTime = ConnectionAttempts.Max((IoTConnectionAttempt y) => y.StartConnectionAttempt);
			if (startConnectionAttempt.HasValue != dateTime.HasValue)
			{
				return false;
			}
			return !startConnectionAttempt.HasValue || startConnectionAttempt.GetValueOrDefault() == dateTime.GetValueOrDefault();
		});
	}

	private IoTConnectionAttempt CreateIoTConnectionAttempt()
	{
		IoTConnectionAttempt ioTConnectionAttempt = new IoTConnectionAttempt
		{
			StartConnectionAttempt = DateTime.Now
		};
		ConnectionAttempts.Add(ioTConnectionAttempt);
		return ioTConnectionAttempt;
	}

	public void Cleanup()
	{
		List<IoTConnectionAttempt> list = ConnectionAttempts.Where((IoTConnectionAttempt x) => x.StartConnectionAttempt.HasValue && x.StartConnectionAttempt.Value.Date < DateTime.Today.AddDays(-30.0)).ToList();
		list.ForEach(delegate(IoTConnectionAttempt x)
		{
			ConnectionAttempts.Remove(x);
		});
	}
}
