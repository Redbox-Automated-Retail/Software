namespace KioskClientService.API.Services.ProfileData;

public class DataTableResponse<T>
{
	public IDataTableRequest DataTableRequest { get; set; }

	public DataTable<T> DataTable { get; set; }

	public bool Success { get; set; }
}
