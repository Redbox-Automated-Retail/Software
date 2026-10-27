using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.Rental.Model.KioskEvents;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskHealth;

public class KioskHealthService : IKioskHealthService
{
	private IKioskEventService _kioskEvents;

	private bool _kioskHealthActive;

	private List<IKioskHealthItem> _kioskHealthItems = new List<IKioskHealthItem>();

	public KioskHealthService(IKioskEventService kioskEvents)
	{
		_kioskEvents = kioskEvents;
	}

	public void Start()
	{
		try
		{
			ReadConfiguration();
			if (!_kioskHealthActive)
			{
				LogHelper.Instance.Log("KioskHealthService is not active");
				return;
			}
			LogHelper.Instance.Log("KioskHealthService.Start");
			_kioskHealthItems.ForEach(delegate(IKioskHealthItem hi)
			{
				hi.Start();
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("KioskHealthService.Start - exception occurred", e);
		}
	}

	public void Stop()
	{
		try
		{
			LogHelper.Instance.Log("KioskHealthService.Stop");
			_kioskHealthItems.ForEach(delegate(IKioskHealthItem hi)
			{
				hi.Stop();
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("KioskHealthService.Stop - exception occured", e);
		}
	}

	public void Restart()
	{
		try
		{
			LogHelper.Instance.Log("KioskHealthService.ReStart");
			_kioskHealthItems.ForEach(delegate(IKioskHealthItem hi)
			{
				hi.Restart();
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("KioskHealthService.Restart - exception occured", e);
		}
	}

	public IKioskHealthItem AddHealthItem(IKioskHealthItem healthItem)
	{
		_kioskHealthItems.Add(healthItem);
		return healthItem;
	}

	private void ReadConfiguration()
	{
		_kioskHealthActive = Redbox.Rental.Services.Configuration.Configuration.Instance.Current.KioskHealth.KioskHealthActive;
		LogHelper.Instance.Log("KioskHealthService Active {0}", _kioskHealthActive);
	}
}
