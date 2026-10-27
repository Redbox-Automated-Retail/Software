using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class ReturnViewController : BaseViewController
{
	public static void ConfigureReturnView()
	{
		OrchestrationService.SuspendIdleTimer("idle_timer");
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.SetCurrentUICulture();
			IAd ad = null;
			if (!BaseViewController.ApplicationState.IsADAAccessible)
			{
				ad = ServiceLocator.Instance.GetService<IAdService>()?.GetScreenAd(AdLocation.Return);
			}
			IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
			ReturnViewModel returnViewModel = (ReturnViewModel)(viewUserControl.DataContext = new ReturnViewModel
			{
				MessageText = Resources.disc_return_message,
				Bullet1Text = Resources.return_bullet_1,
				Bullet2Text = Resources.return_bullet_2,
				TouchlessBannerVisibility = ((!Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode || !service.ZeroTouchPickupEnabled) ? Visibility.Collapsed : Visibility.Visible),
				TouchlessBannerHeaderText = Resources.return_touchless_banner_title,
				TouchlessBannerMessageText = GetTouchlessBannerMessageText(),
				AdImageSource = ad?.Image,
				AdImageVisibility = ((ad == null) ? Visibility.Collapsed : Visibility.Visible),
				AdImpression = ((ad != null) ? new AdImpression
				{
					Ad = ad,
					AdLocation = AdLocation.Return
				} : null)
			});
			returnViewModel.AdImpression?.Start();
		}
		else
		{
			LogHelper.Instance.Log("user control is not of type ReturnViewUserControl");
		}
	}

	public static void OnLeave(IViewFrameInstance viewFrameInstance)
	{
		BaseViewController.RecordAdImpression(BaseViewController.GetViewModel<ReturnViewModel>(viewFrameInstance.Id)?.AdImpression);
	}

	private static string GetTouchlessBannerMessageText()
	{
		ITrackDataService service = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (service == null || service.IsOlderReader)
		{
			return Resources.return_touchless_banner_swipe_message;
		}
		if (!ServiceLocator.Instance.GetService<IConfiguration>().ZeroTouchContactlessEnabled)
		{
			return Resources.return_touchless_banner_insert_message;
		}
		return Resources.return_touchless_banner_contactless_message;
	}
}
