using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "dump-inventory-store-job")]
internal sealed class DumpInventoryStoreJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string path = string.Format("StoreDump-{0}", service.GenerateUniqueFile("txt"));
		using StreamWriter streamWriter = new StreamWriter(Path.Combine("c:\\Program Files\\Redbox\\KioskLogs\\Service", path));
		ServiceLocator.Instance.GetService<IInventoryService>().DumpStore(streamWriter);
		ServiceLocator.Instance.GetService<IDumpbinService>().DumpContents(streamWriter);
		streamWriter.WriteLine();
	}

	internal DumpInventoryStoreJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
