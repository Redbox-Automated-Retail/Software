using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class VsmViewController : BaseViewController
{
	private static IAd _cachedAd;

	public static void ConfigureVsmView()
	{
		if (OrchestrationService.InvokeIsVendJobCompletedCallback())
		{
			OrchestrationService.ViewGoBack("VSM view  vend job completed.", popOnly: false);
			return;
		}
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.SetCurrentUICulture();
			VsmViewModel vsmViewModel = new VsmViewModel
			{
				TitleText = Resources.vsm_title_text
			};
			GetAdImage(vsmViewModel);
			vsmViewModel.ProcessGetSpeechControl = delegate
			{
				SpeechControl speechControl = new SpeechControl();
				SpeechPart item = new SpeechPart
				{
					Texts = { (ITextPart)new TextPart
					{
						Text = vsmViewModel?.TitleText
					} }
				};
				((ISpeechControl)speechControl).SpeechParts.Add(item);
				return speechControl;
			};
			viewUserControl.DataContext = vsmViewModel;
			vsmViewModel?.AdImpression?.Start();
		}
	}

	private static void GetAdImage(VsmViewModel vsmViewModel)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.ShowKioskClosingVSM)
		{
			vsmViewModel.MinimumDisplayTime = 7000;
			vsmViewModel.AdImage = null;
			vsmViewModel.HeaderHeight = 192f;
			vsmViewModel.AdImpression = null;
			vsmViewModel.ClosingLocationGridVisibility = Visibility.Visible;
			vsmViewModel.ClosingLocationDateText = "This location is\nclosing in the next two weeks";
			vsmViewModel.ClosingLocationText = "Visit redbox.com \"find a kiosk\" for the next closest location OR\ncheck out our app to watch On Demand";
			return;
		}
		IAd ad = null;
		if (_cachedAd != null)
		{
			ad = _cachedAd;
			_cachedAd = null;
		}
		else
		{
			ad = BaseViewController.GetAd(AdLocation.VSM);
		}
		bool flag = ad is IVistarAd;
		vsmViewModel.MinimumDisplayTime = ((ad != null && ad.MinimumDisplay.HasValue) ? ad.MinimumDisplay.Value : 3000);
		vsmViewModel.AdImage = ad?.Image;
		vsmViewModel.HeaderHeight = (flag ? 192 : 136);
		vsmViewModel.AdImpression = ((ad != null) ? new AdImpression
		{
			Ad = ad,
			AdLocation = AdLocation.VSM
		} : null);
	}

	public static void LeaveVsmView(IViewFrameInstance viewFrameInstance)
	{
		if (viewFrameInstance == null)
		{
			return;
		}
		VsmViewModel viewModel = BaseViewController.GetViewModel<VsmViewModel>(viewFrameInstance.Id);
		if (viewModel == null)
		{
			return;
		}
		IAdImpression adImpression = viewModel.AdImpression;
		if (adImpression == null)
		{
			return;
		}
		IAd ad = adImpression.Ad;
		if (ad == null || !ad.MinimumDisplay.HasValue)
		{
			return;
		}
		try
		{
			int num = viewModel.AdImpression.Ad.MinimumDisplay.Value - viewModel.AdImpression.DisplayMilliseconds;
			if (num > 0)
			{
				ServiceLocator.Instance.GetService<ITimerService>()?.Wait(num);
			}
			BaseViewController.RecordAdImpression(viewModel.AdImpression);
			LogHelper.Instance.Log($"Leaving vsm_view.  Display duration: {viewModel.AdImpression.DisplayMilliseconds}");
		}
		catch (Exception arg)
		{
			LogHelper.Instance.Log($"Exception in VsmViewController.LeaveVsmView. {arg}");
		}
	}

	public static bool IsVsmAdAvailable()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.ShowKioskClosingVSM)
		{
			return true;
		}
		_cachedAd = BaseViewController.GetAd(AdLocation.VSM);
		return _cachedAd != null;
	}
}
