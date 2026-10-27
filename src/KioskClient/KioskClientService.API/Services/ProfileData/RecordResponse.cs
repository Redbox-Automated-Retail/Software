namespace KioskClientService.API.Services.ProfileData;

public class RecordResponse
{
	public long Id { get; set; }

	public DataUpdateRecordStatus RecordStatus { get; set; }

	public string Data { get; set; }

	public string Hash { get; set; }
}
