using DeviceService.ComponentModel.FileUpdate;

namespace DeviceService.Domain.FileUpdate;

public class FileModel : IFileModel
{
	public string Path { get; set; }

	public int Order { get; set; }

	public bool RebootRequired { get; set; }
}
