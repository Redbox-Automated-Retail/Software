using System;
using Newtonsoft.Json;

namespace Redbox.NetCore.Middleware.Http;

public class Error
{
	public string Code { get; set; }

	[JsonProperty(Required = Required.DisallowNull)]
	public string Message { get; set; }

	public Guid Id { get; set; } = Guid.NewGuid();

	public string ActivityId { get; set; }

	public string KioskId { get; set; }
}
