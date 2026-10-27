using System.Collections.Generic;
using Newtonsoft.Json;

namespace UpdateClientService.API.Services.Configuration;

public class KioskSettingChangesResponse
{
	public int? PageNumber { get; set; }

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

	public int? PageCount { get; set; }

	public bool VersionIsCurrent { get; set; }

	public string ConfigurationVersionHash { get; set; }

	public IEnumerable<long> RemovedConfigurationSettingValueIds { get; set; }

	public bool? RemoveAllExistingConfigurationSettingValues { get; set; }

	public long? OriginalConfigurationVersionId { get; set; }

	public GetKioskConfigurationSettingValues NewConfigurationSettingValues { get; set; }
}
