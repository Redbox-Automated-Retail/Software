using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Model.KioskEvents;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskHealth;

public class ArcusHealth : IArcusHealth, IKioskHealthItem
{
	private IKioskEventService _kioskEvents;

	private const string HardwareCorrectionStart = "HardwareCorrectionStart";

	private const string HardwareCorrectionSuccess = "HardwareCorrectionSuccess";

	private const string HardwareCorrectionVendSuccess = "HardwareCorrectionVendSuccess";

	private const string HardwareCorrectionVendFailure = "HardwareCorrectionVendFailure";

	private bool _arcusActive;

	private bool _arcusReporting;

	private DateTime? _lastReportedStartTime;

	private Guid? _relatedId;

	public ArcusHealth(IKioskEventService kioskEvents)
	{
		_kioskEvents = kioskEvents;
	}

	public void Start()
	{
		try
		{
			LogHelper.Instance.Log("ArcusHealth.Start");
			ReadConfiguration();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("ArcusHealth.Start - exception occurred", e);
		}
	}

	public void Stop()
	{
		LogHelper.Instance.Log("ArcusHealth.Stop");
		_arcusActive = false;
		_arcusReporting = false;
	}

	public void Restart()
	{
		try
		{
			LogHelper.Instance.Log("ArcusHealth.Restart");
			ReadConfiguration();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("ArcusHealth.Restart - exception occurred", e);
		}
	}

	public void EventOccurred(string eventType)
	{
		if (!_arcusActive)
		{
			return;
		}
		LogHelper.Instance.Log("ArcusHealth Event Occurred: {0}", eventType);
		int? elapsed = null;
		if (eventType == "HardwareCorrectionStart")
		{
			_relatedId = ServiceLocator.Instance.GetService<IShoppingSession>()?.Id;
			_lastReportedStartTime = DateTime.Now;
		}
		else if (_arcusReporting)
		{
			int kioskEventSubType = 200;
			if (eventType == "HardwareCorrectionVendSuccess" || eventType == "HardwareCorrectionVendFailure")
			{
				kioskEventSubType = 201;
			}
			bool success = eventType == "HardwareCorrectionVendSuccess" || eventType == "HardwareCorrectionSuccess";
			elapsed = (int)(DateTime.Now - _lastReportedStartTime.Value).TotalMilliseconds;
			_kioskEvents.AddEvent(KioskEvent.NewEvent(2, kioskEventSubType, success, elapsed, _relatedId, new
			{
				Job = "Vend"
			}.ToJson()));
		}
	}

	private void ReadConfiguration()
	{
		KioskHealthConfiguration kioskHealth = Redbox.Rental.Services.Configuration.Configuration.Instance.Current.KioskHealth;
		_arcusActive = kioskHealth.ArcusActive;
		_arcusReporting = kioskHealth.ArcusReporting;
		LogHelper.Instance.Log("ArcusHealth Active {0}", _arcusActive);
		LogHelper.Instance.Log("ArcusHealth Reporting {0}", _arcusReporting);
	}
}
