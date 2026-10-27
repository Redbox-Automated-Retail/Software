namespace KioskClientService.API.Services.ProfileData;

public interface IDataTableRequest
{
	UpdateDataTable TableName { get; }

	FileTypeEnum? FileType { get; }
}
