using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Redbox.Core;

namespace Redbox.UpdateManager.Environment;

public class StoreFilePollRequestList
{
	public List<StoreFilePollRequest> StoreFilePollRequests { get; set; }

	public StoreFilePollRequestList()
	{
		StoreFilePollRequests = new List<StoreFilePollRequest>();
	}

	public string ToShortFormat()
	{
		if (StoreFilePollRequests.Count == 0)
		{
			return string.Empty;
		}
		int pos = 0;
		int count = StoreFilePollRequests.Count();
		StringBuilder sb = new StringBuilder();
		StoreFilePollRequests.ForEach(delegate(StoreFilePollRequest item)
		{
			sb.Append($"{item.StoreFile},{item.StoreFileData},{item.SyncId}");
			pos++;
			if (pos < count)
			{
				sb.Append("|");
			}
		});
		return sb.ToString();
	}

	public void FromShortFormat(string data)
	{
		try
		{
			StoreFilePollRequests.Clear();
			data.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries).ForEach(delegate(string item)
			{
				string[] array = item.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
				StoreFilePollRequest item2 = new StoreFilePollRequest
				{
					StoreFile = Convert.ToInt64(array[0]),
					StoreFileData = Convert.ToInt64(array[1]),
					SyncId = Convert.ToInt32(array[2])
				};
				StoreFilePollRequests.Add(item2);
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("StoreFilePollRequestList - Unhandled exception.", e);
		}
	}
}
