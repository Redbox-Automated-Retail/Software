using System;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "restore-inventory-from-backup")]
internal sealed class RestoreInventoryFromBackupJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Service", flushOnWrite: true, null);
		if (!TestInventoryStore())
		{
			return;
		}
		BackupHelper inventoryHelper = BackupHelper.GetInventoryHelper();
		ServiceLocator.Instance.GetService<IRuntimeService>();
		BackupEntry youngest = inventoryHelper.GetYoungest();
		if (youngest == null)
		{
			applicationLog.WriteFormatted("There are no backup inventory files to import.");
			base.Context.CreateInfoResult("InventoryImportFailure", "There are no backup files available.");
			AddError("No backups available.");
			return;
		}
		applicationLog.WriteFormatted("Reset inventory from file '{0}'", youngest.FullPath);
		try
		{
			string xml = File.ReadAllText(youngest.FullPath);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			if (xmlDocument.DocumentElement == null)
			{
				applicationLog.WriteFormatted("Reset failed - XML load failed.");
				base.Context.CreateInfoResult("InventoryImportFailure", "Invalid inventory file.");
				base.Result.Errors.Add(Error.NewError("I001", "Invalid document.", "The specified document is invalid."));
				return;
			}
			ServiceLocator.Instance.GetService<IInventoryService>().ResetState(xmlDocument, base.Result.Errors);
			if (base.Result.Errors.Count > 0)
			{
				applicationLog.WriteFormatted("Reset inventory FAILED to import inventory.");
			}
			else
			{
				applicationLog.WriteFormatted("Reset inventory was successful.");
			}
		}
		catch (Exception e)
		{
			base.Context.CreateInfoResult("InventoryImportFailure", "Invalid inventory file.");
			base.Result.Errors.Add(Error.NewError("I001", "Invalid document.", e));
		}
	}

	internal RestoreInventoryFromBackupJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
