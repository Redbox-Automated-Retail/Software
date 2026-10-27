using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Sync;

internal sealed class FraudSyncDecorator : SyncDecorator
{
	internal override SyncResult AfterDiskRead(ScanResult sr)
	{
		using (FraudValidator fraudValidator = new FraudValidator(Context))
		{
			fraudValidator.Validate(sr);
		}
		return SyncResult.Success;
	}

	internal FraudSyncDecorator(ExecutionContext ctx, ExecutionResult r, ILocation l)
		: base(ctx, r, l)
	{
	}
}
