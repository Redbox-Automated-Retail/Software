using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Redbox.Core;

namespace Redbox.UpdateService.Model;

public class FileSetPollRequestList
{
	public List<FileSetPollRequest> FileSetPollRequests { get; set; }

	public FileSetPollRequestList()
	{
		FileSetPollRequests = new List<FileSetPollRequest>();
	}

	public string ToShortFormat()
	{
		if (FileSetPollRequests.Count == 0)
		{
			return string.Empty;
		}
		int pos = 0;
		int count = FileSetPollRequests.Count();
		StringBuilder sb = new StringBuilder();
		FileSetPollRequests.ForEach(delegate(FileSetPollRequest item)
		{
			sb.Append($"{item.FileSetId},{item.FileSetRevisionId},{(int)item.FileSetState}");
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
			FileSetPollRequests.Clear();
			data.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries).ForEach(delegate(string item)
			{
				string[] array = item.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
				FileSetPollRequest item2 = new FileSetPollRequest
				{
					FileSetId = Convert.ToInt64(array[0]),
					FileSetRevisionId = Convert.ToInt64(array[1]),
					FileSetState = (FileSetState)Convert.ToInt32(array[2])
				};
				FileSetPollRequests.Add(item2);
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("FileSetPollRequestList - Unhandled exception.", e);
		}
	}
}
