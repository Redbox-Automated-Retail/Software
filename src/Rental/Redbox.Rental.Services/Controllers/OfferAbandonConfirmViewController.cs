using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class OfferAbandonConfirmViewController : BaseViewController
{
	private static object _lock = new object();

	private static Action _continueAction;

	private const string _offerAbandonConfirmViewTimerName = "OfferAbandonConfirmTimer";

	public static void ConfigureView(OfferAbandonConfirmInfo parameters)
	{
		_continueAction = parameters.ContinueAction;
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.SetCurrentUICulture();
			OfferAbandonConfirmModel offerAbandonConfirmModel = new OfferAbandonConfirmModel();
			offerAbandonConfirmModel.OkButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				OkayButtonClickedAction();
			}, (Func<bool>)null);
			PopulateModelText(offerAbandonConfirmModel, parameters);
			ConfigureTTS(offerAbandonConfirmModel);
			viewUserControl.DataContext = offerAbandonConfirmModel;
			ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			int value = ((service != null && service.TTSEnabled) ? 35000 : 15000);
			(ServiceLocator.Instance.GetService<ITimerService>()?.CreateTimer("OfferAbandonConfirmTimer", value, value, delegate
			{
				LogHelper.Instance.Log("offer_abandon_confirm_view_timer executed");
				ExecuteCallback(fromOkayButton: false);
			}))?.Start();
		}
	}

	private static void OkayButtonClickedAction()
	{
		ExecuteCallback(fromOkayButton: true);
	}

	private static void ExecuteCallback(bool fromOkayButton)
	{
		OfferAbandonConfirmModel viewModel = BaseViewController.GetViewModel<OfferAbandonConfirmModel>();
		if (viewModel == null)
		{
			return;
		}
		LockHelper.TryLockObject(_lock, delegate
		{
			if (!viewModel.CallbackExecuted)
			{
				viewModel.CallbackExecuted = true;
				if (fromOkayButton)
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("OfferAbandonConfirmOK");
				}
				OrchestrationService.ViewGoBackSafe("OfferAbandonConfirmView Continue", popOnly: false);
				_continueAction?.Invoke();
			}
			else
			{
				LogHelper.Instance.Log("ExecuteCallback was already executed.");
			}
		}, "ExecuteCallback.Execute");
	}

	private static void PopulateModelText(OfferAbandonConfirmModel viewModel, OfferAbandonConfirmInfo parameters)
	{
		viewModel.OkButtonText = Resources.got_it_button;
		viewModel.OkButtonVisibility = Visibility.Visible;
		viewModel.TitleVisibility = Visibility.Collapsed;
		if (parameters.IsInCart)
		{
			viewModel.SubMessageTextVisibility = Visibility.Collapsed;
			viewModel.MessageVisibility = Visibility.Visible;
			viewModel.MessageText = Resources.continue_with_current_cart_message;
		}
		else
		{
			viewModel.SubMessageTextVisibility = Visibility.Visible;
			viewModel.MessageVisibility = Visibility.Visible;
			viewModel.MessageText = Resources.offer_skipped_message;
			viewModel.SubMessageText = Resources.offer_skipped_submessage;
		}
	}

	private static void ConfigureTTS(OfferAbandonConfirmModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled || viewModel == null)
		{
			return;
		}
		viewModel.OnGetSpeechControl = delegate
		{
			SpeechControl obj = new SpeechControl
			{
				Name = "tts_multi_disc_vend_view"
			};
			SpeechPart part = new SpeechPart
			{
				Name = "Intro",
				Loop = true,
				StartPause = 250,
				EndPause = 10000
			};
			part.Refresh = delegate
			{
				part.Clear();
				part.Texts.Add(new TextPart
				{
					Text = viewModel.MessageText
				});
				if (viewModel.SubMessageTextVisibility == Visibility.Visible)
				{
					part.Texts.Add(new TextPart
					{
						Text = viewModel.SubMessageText
					});
				}
				part.Texts.Add(new TextPart
				{
					Text = "To continue press ENTER"
				});
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						viewModel?.OkButtonCommand.Execute(null);
					}
				});
			};
			obj.SpeechParts.Add(part);
			int i = 0;
			obj.SpeechParts.ForEach(delegate(ISpeechPart x)
			{
				x.Sequence = i++;
			});
			return obj;
		};
	}

	public static void LeaveView()
	{
		OrchestrationService.RemoveTimer("OfferAbandonConfirmTimer");
	}
}
