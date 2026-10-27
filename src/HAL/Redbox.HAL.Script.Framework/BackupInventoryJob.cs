using System.IO;
using System.Text;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "backup-inventory")]
internal sealed class BackupInventoryJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service.CheckIntegrity() == ErrorCodes.Success)
		{
			BackupHelper inventoryHelper = BackupHelper.GetInventoryHelper();
			using (FileStream w = new FileStream(inventoryHelper.MakeNewBackup(), FileMode.OpenOrCreate))
			{
				XmlTextWriter xmlTextWriter = new XmlTextWriter(w, Encoding.ASCII)
				{
					Formatting = Formatting.Indented
				};
				xmlTextWriter.WriteStartDocument();
				xmlTextWriter.WriteStartElement("inventory-items");
				service.GetState(xmlTextWriter);
				ServiceLocator.Instance.GetService<IDumpbinService>().GetState(xmlTextWriter);
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.WriteEndDocument();
				xmlTextWriter.Flush();
			}
			inventoryHelper.Trim();
		}
		else
		{
			ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Service", flushOnWrite: true, null);
			applicationLog.WriteFormatted("Integrity check on the datastore failed.");
			base.Result.Errors.Dump(applicationLog);
			AddError("Integrity check failure.");
		}
	}

	internal BackupInventoryJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
