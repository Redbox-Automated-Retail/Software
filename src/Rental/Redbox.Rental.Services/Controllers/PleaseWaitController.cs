using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class PleaseWaitController : BaseViewController
{
	public static void ConfigureView(PleaseWaitParameters parameters)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			BaseViewController.SetCurrentUICulture();
			IAd ad = null;
			if (!BaseViewController.ApplicationState.IsADAAccessible)
			{
				ad = ServiceLocator.Instance.GetService<IAdService>()?.GetScreenAd(AdLocation.PleaseWait);
			}
			PleaseWaitViewModel pleaseWaitViewModel = (PleaseWaitViewModel)(viewUserControl.DataContext = new PleaseWaitViewModel
			{
				TitleText = Resources.just_a_sec_label,
				MessageText = parameters?.MessageText,
				AdImageSource = ad?.Image,
				AdImageVisibility = ((ad == null) ? Visibility.Collapsed : Visibility.Visible),
				AdImpression = ((ad != null) ? new AdImpression
				{
					Ad = ad,
					AdLocation = AdLocation.PleaseWait
				} : null)
			});
			pleaseWaitViewModel.AdImpression?.Start();
			parameters?.CallbackAction?.Invoke(viewUserControl);
		}
	}

	public static void OnLeave(IViewFrameInstance viewFrameInstance)
	{
		BaseViewController.RecordAdImpression(BaseViewController.GetViewModel<PleaseWaitViewModel>(viewFrameInstance.Id)?.AdImpression);
	}
}
