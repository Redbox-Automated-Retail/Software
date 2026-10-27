using System;
using System.Collections.Generic;
using System.Linq;
using DeviceService.ComponentModel.FileUpdate;

namespace DeviceService.Domain.FileUpdate;

public class UpdateFileManifest : IUpdateFileManifest
{
	public Guid Id { get; set; }

	public int Revision { get; set; }

	public string Name { get; set; }

	public DateTime? StartDate { get; set; }

	public TimeSpan? UpdateRangeStart { get; set; }

	public TimeSpan? UpdateRangeEnd { get; set; }

	public List<FileModel> FileModels { get; set; }

	public override string ToString()
	{
		return string.Format("File {0} - {1} Revision {2} Starts: {3} between {4} and {5} with files: {6}", Id, Name, Revision, StartDate.HasValue ? StartDate.ToString() : "On Receipt", UpdateRangeStart ?? TimeSpan.MinValue, UpdateRangeEnd ?? TimeSpan.FromMilliseconds(86399999.0), (FileModels != null) ? string.Join(", ", FileModels.Select((FileModel x) => x.Path)) : string.Empty);
	}
}
