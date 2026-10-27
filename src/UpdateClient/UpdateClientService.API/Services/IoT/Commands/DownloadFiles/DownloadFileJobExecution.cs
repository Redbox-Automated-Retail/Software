using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UpdateClientService.API.Services.IoT.Commands.DownloadFiles;

public class DownloadFileJobExecution
{
	public string DownloadFileJobExecutionId { get; set; }

	public string DownloadFileJobId { get; set; }

	public long KioskId { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public JobStatus Status { get; set; }

	public DateTime CompletedOn { get; set; }

	public DateTime CompletedOnUtc => CompletedOn.ToUniversalTime();

	public DateTime EventTime { get; set; }

	public DateTime EventTimeUtc => EventTime.ToUniversalTime();
}
