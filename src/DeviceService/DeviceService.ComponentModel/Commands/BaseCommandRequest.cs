using System;
using Newtonsoft.Json;

namespace DeviceService.ComponentModel.Commands;

public class BaseCommandRequest : IMessageScrub
{
	private Guid _guid = Guid.NewGuid();

	private DateTime _createdDate = DateTime.Now;

	public Guid RequestId
	{
		get
		{
			return _guid;
		}
		set
		{
			_guid = value;
		}
	}

	public virtual string CommandName { get; set; }

	public virtual string SignalRCommand => "ExecuteBaseCommand";

	public DateTime CreatedDate
	{
		get
		{
			return _createdDate;
		}
		set
		{
			_createdDate = value;
		}
	}

	public virtual bool IsQueuedCommand => true;

	public int? CommandTimeout { get; set; }

	public Version DeviceServiceClientVersion { get; set; }

	public BaseCommandRequest(string commandName, Version deviceServiceClientVersion)
	{
		CommandName = commandName;
		DeviceServiceClientVersion = deviceServiceClientVersion;
	}

	public virtual object Scrub()
	{
		return JsonConvert.SerializeObject(this);
	}
}
