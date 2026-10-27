using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "check-iceqube-board", Operand = "CHECK-ICEQUBE-BOARD")]
internal sealed class TestIceQubeExchangerJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IAirExchangerService service = ServiceLocator.Instance.GetService<IAirExchangerService>();
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Service", flushOnWrite: true, "IceQubeBoard");
		if (!service.ShouldRetry())
		{
			OnPersistentError();
			return;
		}
		AirExchangerStatus airExchangerStatus = service.CheckStatus();
		if (AirExchangerStatus.Error != airExchangerStatus)
		{
			return;
		}
		LogHelper.Instance.WithContext("Air exchanger board status in error; attempt reset.");
		AirExchangerStatus airExchangerStatus2 = AirExchangerStatus.NotConfigured;
		for (int i = 0; i < 3; i++)
		{
			airExchangerStatus2 = service.Reset();
			applicationLog.Write($"Reset status is {airExchangerStatus2.ToString().ToUpper()}");
			if (AirExchangerStatus.Ok == airExchangerStatus2)
			{
				service.ResetFailureCount();
				return;
			}
			ServiceLocator.Instance.GetService<IRuntimeService>().Wait(2000);
		}
		if (AirExchangerStatus.Error == airExchangerStatus2)
		{
			service.IncrementResetFailureCount();
			if (!service.ShouldRetry())
			{
				OnPersistentError();
			}
		}
	}

	internal TestIceQubeExchangerJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private void OnPersistentError()
	{
		string text = "The board is in persistent error; no further queries.";
		base.Context.AppLog.Write(text);
		LogHelper.Instance.WithContext(text);
		AddError("IceQube board in persistent error state.");
	}
}
