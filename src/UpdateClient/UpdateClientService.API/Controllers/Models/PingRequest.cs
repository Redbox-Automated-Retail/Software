using System;
using Newtonsoft.Json;

namespace UpdateClientService.API.Controllers.Models;

public class PingRequest
{
	[JsonIgnore]
	public Guid MessageId { get; set; }

	public long KioskId { get; set; }

	public string AppName { get; set; }

	public string UTCDate { get; set; }

	public string LocalDate { get; set; }

	public long TTL => DateTimeOffset.UtcNow.AddDays(30.0).ToUnixTimeSeconds();

	public bool? SendToTable { get; set; }
}
