using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Subscription;

public class SubscriptionJob : IInvocable
{
	private ILogger<SubscriptionJob> _logger;

	private ISubscriptionService _subscriptionService;

	public SubscriptionJob(ILogger<SubscriptionJob> logger, ISubscriptionService subscriptionService)
	{
		_logger = logger;
		_subscriptionService = subscriptionService;
	}

	public async Task Invoke()
	{
		_logger.LogInfoWithSource("Invoking Subscription Job to see if updates are needed", "Invoke");
		await _subscriptionService.RefreshIfNeeded();
		_logger.LogInfoWithSource("Finished", "Invoke");
	}
}
