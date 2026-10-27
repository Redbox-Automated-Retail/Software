using Newtonsoft.Json;

namespace KioskClientService.API.Services.HttpQueue;

public interface IReplayableApiCall
{
	[JsonIgnore]
	bool Replayable { get; }
}
