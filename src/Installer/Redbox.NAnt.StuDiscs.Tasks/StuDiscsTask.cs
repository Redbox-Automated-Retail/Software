using System;
using System.Data;
using System.IO;
using NAnt.Core;
using NAnt.Core.Attributes;
using Redbox.NAnt.Core;

namespace Redbox.NAnt.StuDiscs.Tasks;

[TaskName("stuDiscs")]
public class StuDiscsTask : TaskBase
{
	private long _kioskId;

	[TaskAttribute("kioskid", Required = true)]
	public long KioskId
	{
		get
		{
			return _kioskId;
		}
		set
		{
			_kioskId = value;
		}
	}

	protected override void ExecuteTask()
	{
		StuClient stuClient = new StuClient();
		ClientDatabase clientDatabase = new ClientDatabase(this);
		DataSet dataSet = stuClient.ExecuteRule();
		DataView dataView = new DataView(dataSet.Tables[0], null, "TitleID", DataViewRowState.CurrentRows);
		using StreamWriter streamWriter = new StreamWriter("C:\\stuDiscs.txt", append: false);
		streamWriter.WriteLine("KioskId,Title,ForSaleCount,OnHandCount");
		int titleCount = 0;
		long currentTitleId = -1L;
		string? titleName = null;
		int onHandCount = -1;
		foreach (DataRow row in dataView.Table.Rows)
		{
			long titleId = Convert.ToInt64(row["TitleID"]);
			if (row["OnHandCount"] != DBNull.Value)
			{
				onHandCount = Convert.ToInt32(row["OnHandCount"]);
				Log(Level.Warning, $"Current On Hand Count for Title {titleId}: {onHandCount}");
			}
			else
			{
				Log(Level.Warning, $"Current On Hand Count is NULL for Title {titleId}");
			}
			if (currentTitleId == -1)
			{
				currentTitleId = titleId;
				titleName = clientDatabase.GetTitleName(currentTitleId).Replace(",", " ");
				titleCount = 1;
			}
			if (titleId != currentTitleId)
			{
				streamWriter.WriteLine(GetLine(titleName, titleCount, onHandCount));
				currentTitleId = titleId;
				titleName = clientDatabase.GetTitleName(currentTitleId).Replace(",", " ");
				titleCount = 1;
			}
			else
			{
				titleCount++;
			}
		}
		streamWriter.WriteLine(GetLine(titleName, titleCount, onHandCount));
	}

	private string GetLine(string? titleName, int titleCount, int onHandCount)
	{
		return $"{_kioskId},{titleName},{titleCount},{((onHandCount == -1) ? titleCount : onHandCount)}";
	}
}
