using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.DataUpdate;

public class DataUpdateResponse : ApiBaseResponse
{
	public Guid RequestId { get; set; }

	public UpdateDataTable TableName { get; set; }

	public List<RecordResponse> RecordResponses { get; set; } = new List<RecordResponse>();

	public int? PageNumber { get; set; }

	public int? PageCount { get; set; }

	[JsonIgnore]
	public bool IsFirstPage
	{
		get
		{
			if (PageNumber.HasValue)
			{
				return PageNumber == 1;
			}
			return true;
		}
	}

	[JsonIgnore]
	public bool IsLastPage
	{
		get
		{
			if (PageNumber.HasValue)
			{
				if (PageNumber.HasValue)
				{
					return PageNumber == PageCount;
				}
				return false;
			}
			return true;
		}
	}
}
