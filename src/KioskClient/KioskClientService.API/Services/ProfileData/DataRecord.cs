namespace KioskClientService.API.Services.ProfileData;

public class DataRecord<T>
{
	public long Id { get; set; }

	public T Data { get; set; }

	public string Hash { get; set; }
}
