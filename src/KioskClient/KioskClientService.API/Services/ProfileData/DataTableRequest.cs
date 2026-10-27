namespace KioskClientService.API.Services.ProfileData;

public class DataTableRequest : IDataTableRequest
{
	public UpdateDataTable TableName { get; set; }

	public FileTypeEnum? FileType { get; set; }
}
