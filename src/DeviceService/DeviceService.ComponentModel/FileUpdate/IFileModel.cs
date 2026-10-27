namespace DeviceService.ComponentModel.FileUpdate;

public interface IFileModel
{
	string Path { get; set; }

	int Order { get; set; }

	bool RebootRequired { get; set; }
}
