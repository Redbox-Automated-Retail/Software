using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class PleaseWaitMessageViewController : BaseViewController
{
	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.SetCurrentUICulture();
			PleaseWaitMessageViewModel pleaseWaitMessageViewModel = new PleaseWaitMessageViewModel();
			pleaseWaitMessageViewModel.TitleMessage = Resources.please_wait_message;
			ConfigureTTS(pleaseWaitMessageViewModel);
			viewUserControl.DataContext = pleaseWaitMessageViewModel;
		}
	}

	private static void ConfigureTTS(PleaseWaitMessageViewModel pleaseWaitMessageViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled || pleaseWaitMessageViewModel == null)
		{
			return;
		}
		pleaseWaitMessageViewModel.OnGetSpeechControl = delegate
		{
			SpeechControl obj = new SpeechControl
			{
				Name = "tts_please_wait_message_view"
			};
			SpeechPart part = new SpeechPart
			{
				Name = "Main",
				Loop = true,
				StartPause = 250
			};
			part.Refresh = delegate
			{
				part.Clear();
				part.Texts.Add(new TextPart
				{
					Text = pleaseWaitMessageViewModel?.TitleMessage
				});
			};
			obj.SpeechParts.Add(part);
			return obj;
		};
	}
}
