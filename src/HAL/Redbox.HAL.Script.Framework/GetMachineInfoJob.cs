using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-machine-info", Operand = "GET-MACH-INFO")]
internal sealed class GetMachineInfoJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IDeck qlmDeck = ServiceLocator.Instance.GetService<IDecksService>().QlmDeck;
		if (qlmDeck == null)
		{
			base.Context.CreateInfoResult("QlmMissingInfo", "There is no QLM deck defined for this machine.");
		}
		else
		{
			base.Context.CreateResult("QlmInfo", "Designated QLM deck for this machine, slot equals maximum number for this deck.", qlmDeck.Number, qlmDeck.NumberOfSlots, null, null, null);
		}
		base.DecksService.ForAllReverseDecksDo(delegate(IDeck deck)
		{
			base.Context.CreateResult("DeckInfo", "Slot equals maximum number for this deck.", deck.Number, deck.NumberOfSlots, null, null, null);
			return true;
		});
		base.Context.CreateResult("MaxDeckInfo", "Deck equals maximum number for this machine.", base.DecksService.DeckCount, 1, null, null, null);
		base.Context.CreateInfoResult("BufferSlot", ControllerConfiguration.Instance.ReturnSlotBuffer.ToString());
		base.Context.CreateInfoResult("MerchType", (qlmDeck == null) ? "VMZ2" : "QLM");
	}

	internal GetMachineInfoJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
