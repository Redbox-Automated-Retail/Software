using System.Collections.Generic;
using DeviceService.ComponentModel.FileUpdate;
using Newtonsoft.Json;

namespace DeviceService.Domain.FileUpdate;

public class FileUpdateResults : IFileUpdateResults
{
	private List<IZipFileUpdateResult> _zipFileUpdateResultCollection = new List<IZipFileUpdateResult>();

	public bool Success { get; set; }

	public FileUpdateHighLevelStatus Status { get; set; }

	public string StatusMessage { get; set; }

	public List<IZipFileUpdateResult> ZipFileUpdateResultCollection
	{
		get
		{
			return _zipFileUpdateResultCollection;
		}
		set
		{
			_zipFileUpdateResultCollection = value;
		}
	}

	public override string ToString()
	{
		return "FileUpdateResults: " + JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
