using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "qlm-kiosk-test", HideFromList = true)]
internal sealed class QlmKioskTestJob : NativeJobAdapter
{
	private class QlmTestAdapter : KioskTestAdapter
	{
		private readonly bool RunOp;

		protected override ErrorCodes OnUnitComplete()
		{
			ErrorCodes result = ErrorCodes.Success;
			if (RunOp)
			{
				switch (ControlSystem.GetQlmStatus())
				{
				case QlmStatus.Engaged:
					result = ControlSystem.DisengageQlm(Context.AppLog);
					break;
				case QlmStatus.Disengaged:
					result = ControlSystem.EngageQlm(Context.AppLog);
					break;
				}
			}
			return result;
		}

		protected override bool OnVendDisk(string matrix)
		{
			IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
			if (!ControlSystem.VendDoorRent().Success)
			{
				Context.CreateInfoResult("VendDoorRentError", "Failed to open the vend door.");
				return false;
			}
			service.SpinWait(500);
			IControlResponse controlResponse = ControlSystem.VendDoorClose();
			if (!controlResponse.Success)
			{
				Context.CreateInfoResult("VendDoorCloseError", "Failed to close the vend door.");
			}
			return controlResponse.Success;
		}

		internal QlmTestAdapter(ExecutionResult r, ExecutionContext c, bool runOp)
			: base(r, c)
		{
			RunOp = runOp;
		}
	}

	protected override void ExecuteInner()
	{
		bool runOp = base.Context.PopTop<bool>();
		using QlmTestAdapter qlmTestAdapter = new QlmTestAdapter(base.Result, base.Context, runOp);
		qlmTestAdapter.Run();
	}

	internal QlmKioskTestJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
