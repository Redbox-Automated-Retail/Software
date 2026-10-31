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
		int num = 0;
		long num2 = -1L;
		string titleName = null;
		int num3 = -1;
		foreach (DataRow row in dataView.Table.Rows)
		{
			long num4 = Convert.ToInt64(row["TitleID"]);
			if (row["OnHandCount"] != DBNull.Value)
			{
				num3 = Convert.ToInt32(row["OnHandCount"]);
				Log(Level.Warning, $"Current On Hand Count for Title {num4}: {num3}");
			}
			else
			{
				Log(Level.Warning, $"Current On Hand Count is NULL for Title {num4}");
			}
			if (num2 == -1)
			{
				num2 = num4;
				titleName = clientDatabase.GetTitleName(num2).Replace(",", " ");
				num = 1;
			}
			if (num4 != num2)
			{
				streamWriter.WriteLine(GetLine(titleName, num, num3));
				num2 = num4;
				titleName = clientDatabase.GetTitleName(num2).Replace(",", " ");
				num = 1;
			}
			else
			{
				num++;
			}
		}
		streamWriter.WriteLine(GetLine(titleName, num, num3));
	}

	private string GetLine(string titleName, int titleCount, int onHandCount)
	{
		return $"{_kioskId},{titleName},{titleCount},{((onHandCount == -1) ? titleCount : onHandCount)}";
	}
}
