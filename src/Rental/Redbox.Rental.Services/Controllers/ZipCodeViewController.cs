using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ZipCodeViewController : BaseViewController
{
	private static bool _hasNextBeenClicked;

	public static void ConfigureZipCodeView(Action<string> nextAction)
	{
		_hasNextBeenClicked = false;
		if (!(BaseViewController.GetViewUserControl() is ZipCodeViewUserControl zipCodeViewUserControl))
		{
			return;
		}
		OrchestrationService.SetCurrentUICulture();
		ZipCodeViewModel zipCodeViewModel = new ZipCodeViewModel();
		zipCodeViewModel.KeypadModel = new KeypadModel();
		zipCodeViewModel.KeypadModel.ClearAllButtonText = Resources.keypad_clear_all_button_text;
		zipCodeViewModel.KeypadModel.BackButtonText = Resources.keypad_back_button_text;
		KeypadModel keypadModel = zipCodeViewModel.KeypadModel;
		keypadModel.OnNumberButtonClicked = (Action<string>)Delegate.Combine(keypadModel.OnNumberButtonClicked, new Action<string>(KeypadModel_OnNumberButtonClicked));
		KeypadModel keypadModel2 = zipCodeViewModel.KeypadModel;
		keypadModel2.OnClearAllButtonClicked = (Action)Delegate.Combine(keypadModel2.OnClearAllButtonClicked, new Action(KeypadModel_OnClearAllButtonClicked));
		KeypadModel keypadModel3 = zipCodeViewModel.KeypadModel;
		keypadModel3.OnBackButtonClicked = (Action)Delegate.Combine(keypadModel3.OnBackButtonClicked, new Action(KeypadModel_OnBackButtonClicked));
		zipCodeViewModel.RequiredLength = 5;
		zipCodeViewModel.DisplayText = "";
		zipCodeViewModel.HeaderLabelText = Resources.zip_code_header_text;
		zipCodeViewModel.Header2LabelText = Resources.zip_code_header_text2;
		zipCodeViewModel.Note1LabelText = Resources.zip_code_note_text;
		string note2LabelText = null;
		if (Redbox.Rental.Services.Session.Session.Instance?.PreauthorizeResult?.PromptRequired == true)
		{
			note2LabelText = string.Format(Resources.zip_code_auth_text, ConvertAuthorizeDays(Redbox.Rental.Services.Session.Session.Instance.PreauthorizeResult.AuthDays));
		}
		zipCodeViewModel.Note2LabelText = note2LabelText;
		zipCodeViewModel.NextButtonText = Resources.zip_code_next_button_text.ToUpper();
		zipCodeViewModel.IsNextButtonEnabled = false;
		zipCodeViewModel.OnNextButtonClicked = delegate(string s)
		{
			if (!_hasNextBeenClicked)
			{
				_hasNextBeenClicked = true;
				nextAction?.Invoke(s);
			}
		};
		ConfigureAdImage(zipCodeViewModel);
		ConfigureTTS(zipCodeViewModel);
		OrchestrationService.StartIdleTimer("idle_timer");
		zipCodeViewUserControl.DataContext = zipCodeViewModel;
		zipCodeViewModel?.AdImpression?.Start();
	}

	public static void OnLeave(IViewFrameInstance viewFrameInstance)
	{
		BaseViewController.RecordAdImpression(BaseViewController.GetViewModel<ZipCodeViewModel>(viewFrameInstance.Id)?.AdImpression);
	}

	private static string ConvertAuthorizeDays(int authorizeDays)
	{
		string text = null;
		try
		{
			string name = $"number_{authorizeDays}";
			text = Resources.ResourceManager.GetString(name);
		}
		catch
		{
			text = Resources.number_5;
		}
		return text?.ToLower();
	}

	private static ZipCodeViewModel GetZipCodeViewModel()
	{
		if (BaseViewController.GetViewUserControl() is ZipCodeViewUserControl zipCodeViewUserControl)
		{
			return zipCodeViewUserControl.DataContext as ZipCodeViewModel;
		}
		return null;
	}

	private static void ConfigureAdImage(ZipCodeViewModel zipCodeViewModel)
	{
		if (!BaseViewController.ApplicationState.IsADAAccessible)
		{
			IAd ad = ServiceLocator.Instance.GetService<IAdService>()?.GetScreenAd(AdLocation.ZipCode);
			zipCodeViewModel.AdImpression = ((ad != null) ? new AdImpression
			{
				Ad = ad,
				AdLocation = AdLocation.ZipCode
			} : null);
			zipCodeViewModel.AdImageSource = ad?.Image;
			zipCodeViewModel.AdImageVisibility = ((ad?.Image == null) ? Visibility.Collapsed : Visibility.Visible);
			zipCodeViewModel.StackPanelMargin = ((ad?.Image != null) ? new Thickness(0.0, 0.0, 0.0, 0.0) : new Thickness(0.0, 49.0, 0.0, 0.0));
		}
	}

	private static void ConfigureTTS(ZipCodeViewModel zipCodeViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			zipCodeViewModel.OnGetSpeechControl = () => ZipCodeViewTextToSpeech.GetSpeechControl(GetZipCodeViewModel);
		}
	}

	private static void KeypadModel_OnNumberButtonClicked(string key)
	{
		ZipCodeViewModel zipCodeViewModel = GetZipCodeViewModel();
		if (zipCodeViewModel != null && key != null && zipCodeViewModel.DisplayText.Length < zipCodeViewModel.RequiredLength)
		{
			zipCodeViewModel.DisplayText += key;
			UpdateIsNextButtonEnabled();
		}
	}

	private static void KeypadModel_OnClearAllButtonClicked()
	{
		ZipCodeViewModel zipCodeViewModel = GetZipCodeViewModel();
		if (zipCodeViewModel != null)
		{
			zipCodeViewModel.DisplayText = "";
			UpdateIsNextButtonEnabled();
		}
	}

	private static void KeypadModel_OnBackButtonClicked()
	{
		ZipCodeViewModel zipCodeViewModel = GetZipCodeViewModel();
		if (zipCodeViewModel != null && zipCodeViewModel.DisplayText.Length > 0)
		{
			zipCodeViewModel.DisplayText = zipCodeViewModel.DisplayText.Remove(zipCodeViewModel.DisplayText.Length - 1);
			UpdateIsNextButtonEnabled();
		}
	}

	private static void UpdateIsNextButtonEnabled()
	{
		ZipCodeViewModel zipCodeViewModel = GetZipCodeViewModel();
		if (zipCodeViewModel != null)
		{
			zipCodeViewModel.IsNextButtonEnabled = zipCodeViewModel.DisplayText.Length == zipCodeViewModel.RequiredLength;
		}
	}
}
