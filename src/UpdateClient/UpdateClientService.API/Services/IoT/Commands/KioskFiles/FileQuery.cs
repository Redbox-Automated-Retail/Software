using System.IO;
using Newtonsoft.Json;

namespace UpdateClientService.API.Services.IoT.Commands.KioskFiles;

public class FileQuery
{
	[JsonProperty("path")]
	public string Path { get; set; }

	[JsonProperty("searchPattern")]
	public string SearchPattern { get; set; }

	[JsonProperty("searchOptions")]
	public EnumerationOptions SearchOptions { get; set; }
}
