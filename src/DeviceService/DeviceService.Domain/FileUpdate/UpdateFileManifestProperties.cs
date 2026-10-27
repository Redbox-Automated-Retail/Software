using System;
using DeviceService.ComponentModel.FileUpdate;

namespace DeviceService.Domain.FileUpdate;

public class UpdateFileManifestProperties : IUpdateFileManifestProperties
{
	public ManifestPropertiesStatus Status { get; set; }

	public int Revision { get; set; }

	public Guid Id { get; set; }

	public string Name { get; set; }

	public DateTime? StartDate { get; set; }

	public TimeSpan? UpdateRangeStart { get; set; }

	public TimeSpan? UpdateRangeEnd { get; set; }
}
