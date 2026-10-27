using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "mark-kiosk-inventory")]
internal sealed class MarkKioskInventoryJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		string inventoryToken = base.Context.PopTop<string>();
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		service.ForAllDecksDo(delegate(IDeck deck)
		{
			inventoryService.ResetAndMark(deck, inventoryToken);
			return true;
		});
	}

	internal MarkKioskInventoryJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
