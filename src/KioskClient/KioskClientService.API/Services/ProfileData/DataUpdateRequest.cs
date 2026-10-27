using System;
using System.Collections.Generic;

namespace KioskClientService.API.Services.ProfileData;

public class DataUpdateRequest
{
	public Guid RequestId { get; set; }

	public UpdateDataTable TableName { get; set; }

	public long KioskId { get; set; }

	public DataUpdateRequestType RequestType { get; set; }

	public List<RecordRequest> RecordRequests { get; set; }

	public string Query { get; set; }

	public FileTypeEnum? FileType { get; set; }

	public DateTime CreatedDateTime { get; set; } = DateTime.Now;
}
