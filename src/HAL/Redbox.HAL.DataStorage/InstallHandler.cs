using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class InstallHandler : IPackageInstallHandler
{
	public void OnRegister()
	{
	}

	public void OnUpgrade(ErrorList errors, TextWriter writer)
	{
		IDataTableService service = ServiceLocator.Instance.GetService<IDataTableService>();
		IDataTable<IDumpBinInventoryItem> table = service.GetTable<IDumpBinInventoryItem>();
		if (table.Exists)
		{
			writer.WriteLine("[InventoryService.Upgrade] The bin table already exists.");
		}
		else if (!table.Create())
		{
			errors.Add(Error.NewError("I0777", "Create table error.", "Unable to create dumpbin table in haldata.vdb3."));
		}
		else
		{
			IDataTable<IDumpBinInventoryItem> legacyTable = service.GetLegacyTable<IDumpBinInventoryItem>();
			if (legacyTable.Exists)
			{
				List<IDumpBinInventoryItem> list = legacyTable.LoadEntries();
				using (new DisposeableList<IDumpBinInventoryItem>(list))
				{
					if (table.Insert(list))
					{
						writer.WriteLine("[InventoryService.OnUpgrade] Successfully imported {0} dumpbin entries.", list.Count);
					}
					else
					{
						errors.Add(Error.NewError("I667", "Import error.", "Unable to move dumpbin table data from halcounters."));
					}
				}
				legacyTable.Drop();
			}
		}
		OnNewInstall(errors, writer);
	}

	public void OnNewInstall(ErrorList errors, TextWriter writer)
	{
		IDataTableService service = ServiceLocator.Instance.GetService<IDataTableService>();
		IDataTable<IKioskFunctionCheckData> table = service.GetTable<IKioskFunctionCheckData>();
		TestAndCreate(table, errors, writer);
		IDataTable<IHardwareCorrectionStatistic> table2 = service.GetTable<IHardwareCorrectionStatistic>();
		TestAndCreate(table2, errors, writer);
		IDataTable<IHardwareCorrectionStatistic> legacyTable = service.GetLegacyTable<IHardwareCorrectionStatistic>();
		if (legacyTable != null && legacyTable.Exists)
		{
			legacyTable.Drop();
		}
	}

	private void TestAndCreate<T>(IDataTable<T> table, ErrorList errors, TextWriter writer)
	{
		if (!table.Exists)
		{
			if (!table.Create())
			{
				errors.Add(Error.NewError("I0777", "Create table error.", $"Unable to create table '{table.Name}'"));
			}
			else
			{
				writer.WriteLine("Successfully created table '{0}'", table.Name);
			}
		}
		else
		{
			writer.WriteLine("The table '{0}' already exists.", table.Name);
		}
	}
}
