using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.UpdateClientService;
using Redbox.Rental.Model.UpdateClientService.KioskHealth;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.UpdateClientService.KioskHealth;

namespace Redbox.Rental.Services.KioskHealth;

public class PingService : IPingService
{
	private Timer _pingTimer;

	public bool Enabled => Redbox.Rental.Services.Configuration.Configuration.Instance.Current.KioskHealth.PingServiceEnabled;

	public int PingInterval => Redbox.Rental.Services.Configuration.Configuration.Instance.Current.KioskHealth.PingInterval;

	public PingService()
	{
		StartTimer();
	}

	public async Task<PingResponse> SendPing()
	{
		PingResponse pingResponse = new PingResponse
		{
			StatusCode = 500,
			Success = false
		};
		if (Enabled)
		{
			IUpdateClientServiceKioskHealth updateClientServiceKioskHealth = ServiceLocator.Instance.GetService<IUpdateClientService>()?.KioskHealth;
			if (updateClientServiceKioskHealth != null)
			{
				int num = (int)(ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0);
				ServiceLocator.Instance.GetService<IResourceBundleService>();
				pingResponse = await updateClientServiceKioskHealth.SendPing(new PingRequest
				{
					AppName = "KioskEngine",
					MessageId = Guid.NewGuid(),
					KioskId = num,
					UTCDate = DateTime.UtcNow.ToString("s"),
					LocalDate = DateTime.Now.ToString("s")
				});
			}
		}
		else
		{
			pingResponse.StatusCode = 503;
			pingResponse.Success = false;
			pingResponse.Errors.Add(new Redbox.Rental.Model.Error
			{
				Code = HttpStatusCode.ServiceUnavailable.ToString(),
				Message = "Ping Service is not enabled."
			});
		}
		return pingResponse;
	}

	private void StartTimer()
	{
		LogHelper.Instance.Log($"Starting PingService timer with interval {PingInterval}");
		if (_pingTimer != null)
		{
			return;
		}
		Random random = new Random();
		_pingTimer = new Timer(delegate
		{
			try
			{
				if (_pingTimer != null)
				{
					_pingTimer.Change(-1, -1);
				}
				if (Enabled)
				{
					SendPing();
				}
			}
			finally
			{
				if (_pingTimer != null)
				{
					_pingTimer.Change(PingInterval, PingInterval);
				}
			}
		}, null, random.Next(0, PingInterval), PingInterval);
	}
}
