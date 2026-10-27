using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services;

public sealed class AirExchangerState
{
	private const string ExchangerMnemonic = "AIRXCHGR";

	private readonly HardwareService Service;

	public ExchangerFanStatus FanStatus { get; private set; }

	public bool Configured { get; private set; }

	public ExchangerFanStatus Configure(AirExchangerStatus exchgrStatus, ExchangerFanStatus fanStatus)
	{
		Configured = exchgrStatus != AirExchangerStatus.NotConfigured;
		FanStatus = fanStatus;
		return FanStatus;
	}

	public ExchangerFanStatus ToggleFan()
	{
		string statement = string.Format("{0} {1}", "AIRXCHGR", (ExchangerFanStatus.On == FanStatus) ? "FANOFF" : "FANON");
		if (Service.ExecuteImmediate(statement, out var _).Success)
		{
			if (ExchangerFanStatus.On == FanStatus)
			{
				FanStatus = ExchangerFanStatus.Off;
			}
			else if (ExchangerFanStatus.Off == FanStatus)
			{
				FanStatus = ExchangerFanStatus.On;
			}
		}
		return FanStatus;
	}

	public bool ToggleConfiguration()
	{
		bool configured = !Configured;
		if (Service.ExecuteImmediate($"SETCFG \"EnableIceQubePolling\" \"{configured.ToString()}\" TYPE=CONTROLLER", out var _).Success)
		{
			Configured = configured;
			return true;
		}
		return false;
	}

	public AirExchangerState(HardwareService s)
	{
		Service = s;
	}
}
