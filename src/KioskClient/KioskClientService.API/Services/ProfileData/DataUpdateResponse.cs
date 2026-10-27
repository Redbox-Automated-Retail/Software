using System;
using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProfileData;

public class DataUpdateResponse : ApiBaseResponse
{
	public Guid RequestId { get; set; }

	public UpdateDataTable TableName { get; set; }

	public List<RecordResponse> RecordResponses { get; set; } = new List<RecordResponse>();
}
