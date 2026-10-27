using System;
using System.Drawing;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.Controllers;

public class BaseViewController
{
	private static string IdleTimerName = "idle_timer";

	protected static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	protected static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

	protected static IApplicationState ApplicationState => ServiceLocator.Instance.GetService<IApplicationState>();

	protected static IDataService DataService => ServiceLocator.Instance.GetService<IDataService>();

	protected static bool IsADAMode => ApplicationState?.IsADAAccessible ?? false;

	protected static FrameworkElement GetViewUserControl()
	{
		IViewFrameInstance viewFrameInstance = ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame();
		if (viewFrameInstance != null)
		{
			_ = viewFrameInstance.Id;
			return GetViewUserControl(viewFrameInstance.Id);
		}
		return null;
	}

	protected static FrameworkElement GetViewUserControl(Guid userControlId)
	{
		return ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default")?.GetWPFGridChild(userControlId);
	}

	protected static T GetViewModel<T>() where T : class
	{
		return GetViewModel<T>(GetViewUserControl());
	}

	protected static T GetViewModel<T>(Guid userControlId) where T : class
	{
		return GetViewModel<T>(GetViewUserControl(userControlId));
	}

	private static T GetViewModel<T>(FrameworkElement userControl) where T : class
	{
		if (userControl != null)
		{
			return userControl.Dispatcher.Invoke(() => userControl?.DataContext as T);
		}
		return null;
	}

	protected static void ProcessWPFHit()
	{
		ServiceLocator.Instance.GetService<IRenderingService>()?.ActiveScene.PrcoessWPFHit();
		PostHealthActivity();
	}

	protected static void PostHealthActivity()
	{
		ServiceLocator.Instance.GetService<ITouchScreenHealth>()?.PostActivity();
		ServiceLocator.Instance.GetService<IViewHealth>()?.PostActivity("Button Press");
	}

	protected static Visibility BoolToVisibility(bool show)
	{
		if (!show)
		{
			return Visibility.Collapsed;
		}
		return Visibility.Visible;
	}

	protected static void Log(string message, string prefix = "")
	{
		LogHelper.Instance.Log($"{prefix}: {message}");
	}

	protected static IViewService GetViewService()
	{
		return ServiceLocator.Instance.GetService<IViewService>();
	}

	protected static IMacroService GetMacroService()
	{
		return ServiceLocator.Instance.GetService<IMacroService>();
	}

	protected static void SetCurrentUICulture()
	{
		OrchestrationService.SetCurrentUICulture();
	}

	protected static void StartIdleTimer(bool reset = false)
	{
		OrchestrationService.StartIdleTimer(IdleTimerName, reset);
	}

	protected static void StopIdleTimer()
	{
		OrchestrationService.StopIdleTimer(IdleTimerName);
	}

	protected static void ResetIdleTimer()
	{
		OrchestrationService.ResetIdleTimer(IdleTimerName);
	}

	protected static bool GetPickupStatus()
	{
		return (RentalShoppingCartService?.CurrentRentalShoppingCart)?.PickupRecommendationsShown ?? false;
	}

	protected static BitmapImage GetBundleBitmapImage(string resourceName)
	{
		IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		BitmapImage result = null;
		Image image = null;
		image = ((!resourceName.StartsWith("cache:")) ? ServiceLocator.Instance.GetService<IResourceBundleService>().GetBitmap(resourceName) : service?.GetImage(resourceName.Substring(6)));
		if (image != null)
		{
			result = service?.ToBitmapImage(image);
		}
		return result;
	}

	protected static IAd GetAd(AdLocation location)
	{
		return ServiceLocator.Instance.GetService<IAdService>().GetScreenAd(location);
	}

	protected static async Task<IAd> GetAdAsync(AdLocation location)
	{
		return await ServiceLocator.Instance.GetService<IAdService>().GetScreenAdAsync(location).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected static void InvokeActionOnIdle(Action action)
	{
		Application.Current.Dispatcher.BeginInvoke(action, DispatcherPriority.SystemIdle);
	}

	protected static SecureString StringToSecureString(string source)
	{
		if (string.IsNullOrWhiteSpace(source))
		{
			return null;
		}
		SecureString secureString = new SecureString();
		foreach (char c in source)
		{
			secureString.AppendChar(c);
		}
		secureString.MakeReadOnly();
		return secureString;
	}

	protected static void RecordAdImpression(IAdImpression adImpression)
	{
		if (adImpression != null && !adImpression.HasEnded)
		{
			adImpression.End();
			Task.Run(() => ServiceLocator.Instance.GetService<IAdService>()?.RecordAdImpression(adImpression));
		}
	}
}
