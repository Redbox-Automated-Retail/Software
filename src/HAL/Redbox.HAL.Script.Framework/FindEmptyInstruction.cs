using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class FindEmptyInstruction : Instruction
{
	public override string Mnemonic => "FINDEMPTY";

	protected override ExpectedOperands Expected => ExpectedOperands.None;

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		using IEmptySearchResult emptySearchResult = ServiceLocator.Instance.GetService<IEmptySearchPatternService>().FindEmptyLocations();
		if (emptySearchResult.FoundEmpty == 0)
		{
			context.PushTop("MACHINE FULL");
			return;
		}
		ILocation location = emptySearchResult.EmptyLocations[0];
		context.PushTop(location.Slot);
		context.PushTop(location.Deck);
		context.PushTop("FOUND");
	}
}
