using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class KioskFunctionCheckService : IKioskFunctionCheckService
{
	private readonly IDataTable<IKioskFunctionCheckData> Table;

	public bool Add(IKioskFunctionCheckData data)
	{
		return Table.Insert(data);
	}

	public IList<IKioskFunctionCheckData> Load()
	{
		return Table.LoadEntries();
	}

	public int CleanOldEntries()
	{
		return Table.Clean();
	}

	internal KioskFunctionCheckService()
	{
		IDataTableService service = ServiceLocator.Instance.GetService<IDataTableService>();
		Table = service.GetTable<IKioskFunctionCheckData>();
		if (!Table.Exists)
		{
			bool flag = Table.Create();
			LogHelper.Instance.Log("[KioskFunctionCheckService] KFC table doesn't exist; create returned {0}", flag ? "SUCCESS" : "FAILURE");
		}
	}
}
