using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class SyncInstruction : Instruction
{
	public override string Mnemonic => "SYNC";

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("TYPE");
		SyncType syncType = SyncType.Standard;
		if (keyValuePair != null)
		{
			syncType = Enum<SyncType>.ParseIgnoringCase(keyValuePair.Value, SyncType.None);
			if (syncType == SyncType.None)
			{
				result.Errors.Add(Error.NewError("E777", "Unrecognized sync type.", $"Unrecognized sync type {keyValuePair}"));
				return;
			}
		}
		if (!GetLocation(result, context, out var location))
		{
			result.Errors.Add(Error.NewError("E778", "Missing required operands.", "The DECK and SLOT operands are missing. Unrecognized sync type {0}"));
			return;
		}
		SyncDecorator decorator = SyncDecorator.Get(syncType, context, result, location);
		context.PushTop(SyncHelper.SyncSlot(result, context, location, decorator).ToString().ToUpper());
	}
}
