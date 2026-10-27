using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal class IceQubeAirExchangerService : IAirExchangerService, IConfigurationObserver
{
	private const string ResetFailures = "IceQubeResetFailures";

	public bool Configured { get; private set; }

	public ExchangerFanStatus FanStatus { get; private set; }

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[IceQubeService] Configuration change start.");
		FanStatus = ExchangerFanStatus.Unknown;
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[IceQubeService] Configuration change end.");
		Configured = ControllerConfiguration.Instance.EnableIceQubePolling;
		FanStatus = ((!Configured) ? ExchangerFanStatus.NotConfigured : ExchangerFanStatus.On);
		if (!Configured)
		{
			TurnOnFan();
		}
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[IceQubeService] Configuration loaded.");
		Configured = ControllerConfiguration.Instance.EnableIceQubePolling;
		FanStatus = ((!Configured) ? ExchangerFanStatus.NotConfigured : ExchangerFanStatus.On);
	}

	public AirExchangerStatus CheckStatus()
	{
		if (!Configured)
		{
			return AirExchangerStatus.NotConfigured;
		}
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		return CheckStatusInner(service);
	}

	public void TurnOnFan()
	{
		if (Configured)
		{
			if (ExchangerFanStatus.On == FanStatus)
			{
				LogHelper.Instance.Log("Fan is already on - bypass.");
				return;
			}
			IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
			TurnOnFanChecked(service);
			Thread.Sleep(500);
			ResetChecked(service);
		}
	}

	public void TurnOffFan()
	{
		if (Configured)
		{
			if (ExchangerFanStatus.Off == FanStatus)
			{
				LogHelper.Instance.WithContext("Air exchanger fan is already off - bypass.");
				return;
			}
			IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
			TurnOffFanChecked(service);
			Thread.Sleep(10500);
		}
	}

	public AirExchangerStatus Reset()
	{
		if (!Configured)
		{
			return AirExchangerStatus.NotConfigured;
		}
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		return ResetChecked(service);
	}

	public int PersistentFailureCount()
	{
		return ServiceLocator.Instance.GetService<IPersistentCounterService>().Find("IceQubeResetFailures")?.Value ?? 0;
	}

	public bool ShouldRetry()
	{
		IPersistentCounter persistentCounter = ServiceLocator.Instance.GetService<IPersistentCounterService>().Find("IceQubeResetFailures");
		if (persistentCounter == null)
		{
			return false;
		}
		return persistentCounter.Value < 3;
	}

	public void ResetFailureCount()
	{
		ServiceLocator.Instance.GetService<IPersistentCounterService>().Reset("IceQubeResetFailures");
	}

	public void IncrementResetFailureCount()
	{
		ServiceLocator.Instance.GetService<IPersistentCounterService>().Increment("IceQubeResetFailures");
	}

	internal IceQubeAirExchangerService()
	{
		FanStatus = ExchangerFanStatus.NotConfigured;
		ControllerConfiguration.Instance.AddObserver(this);
	}

	private AirExchangerStatus ResetChecked(IControlSystem cs)
	{
		TurnOffFanChecked(cs);
		Thread.Sleep(1500);
		TurnOnFanChecked(cs);
		Thread.Sleep(1500);
		return CheckStatusInner(cs);
	}

	private bool TurnOnFanChecked(IControlSystem cs)
	{
		IControlResponse controlResponse = cs.LockQlmDoor();
		if (controlResponse.Success)
		{
			FanStatus = ExchangerFanStatus.On;
		}
		return controlResponse.Success;
	}

	private bool TurnOffFanChecked(IControlSystem cs)
	{
		IControlResponse controlResponse = cs.UnlockQlmDoor();
		if (controlResponse.Success)
		{
			FanStatus = ExchangerFanStatus.Off;
		}
		return controlResponse.Success;
	}

	private AirExchangerStatus CheckStatusInner(IControlSystem cs)
	{
		IReadInputsResult<AuxInputs> readInputsResult = cs.ReadAuxInputs();
		if (!readInputsResult.Success)
		{
			return AirExchangerStatus.Error;
		}
		if (!readInputsResult.IsInputActive(AuxInputs.QlmBayDoor))
		{
			return AirExchangerStatus.Error;
		}
		return AirExchangerStatus.Ok;
	}
}
