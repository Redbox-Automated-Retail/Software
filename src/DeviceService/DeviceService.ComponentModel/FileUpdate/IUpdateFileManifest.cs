using System;

namespace DeviceService.ComponentModel.FileUpdate;

public interface IUpdateFileManifest
{
	Guid Id { get; set; }

	int Revision { get; set; }

	string Name { get; set; }

	DateTime? StartDate { get; set; }

	TimeSpan? UpdateRangeStart { get; set; }

	TimeSpan? UpdateRangeEnd { get; set; }
}
