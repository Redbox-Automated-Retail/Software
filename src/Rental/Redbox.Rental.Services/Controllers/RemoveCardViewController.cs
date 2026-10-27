using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Responses;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class RemoveCardViewController : BaseViewController
{
	private static object _lockObject = new object();

	public static void ConfigureView(RemoveCardViewParameters removeCardViewParameters)
	{
		if (!(BaseViewController.GetViewUserControl() is RemoveCardViewUserControl removeCardViewUserControl))
		{
			return;
		}
		OrchestrationService.StopIdleTimer("idle_timer");
		RemoveCardViewModel removeCardViewModel = new RemoveCardViewModel();
		ConfigureText(removeCardViewModel, removeCardViewParameters);
		removeCardViewModel.Timeout = 120000;
		removeCardViewModel.StartCardRemovedYetCheck = StartCardRemovedYetCheck;
		removeCardViewModel.CardNeverRemoved = CardNeverRemovedAction;
		removeCardViewModel.ContinueAction = removeCardViewParameters.ContinueAction;
		removeCardViewUserControl.DataContext = removeCardViewModel;
		removeCardViewModel.OnGetSpeechControl = delegate
		{
			ISpeechControl result = new SpeechControl
			{
				Name = "tts_remove_card_view"
			};
			AddSpeechPartTitle(result, removeCardViewModel);
			result.SpeechParts.ForEach(delegate(ISpeechPart x)
			{
				x.SpeechControl = result;
			});
			return result;
		};
	}

	private static void CardNeverRemovedAction()
	{
		lock (_lockObject)
		{
			if (OrchestrationService.PeekView() == "remove_card_view")
			{
				LogHelper.Instance.Log("RemoveCardView timeout timer expired.");
				ServiceLocator.Instance.GetService<IRedboxRental>()?.HandleUnremovedChipInCardReader();
			}
		}
	}

	private static void StartCardRemovedYetCheck(RemoveCardViewModel removeCardViewModel)
	{
		lock (_lockObject)
		{
			if (OrchestrationService.PeekView() == "remove_card_view")
			{
				IDeviceServiceClientHelper service = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
				if (removeCardViewModel != null && !removeCardViewModel.CardHasBeenRemoved && service != null)
				{
					service.GetCardInsertedStatus(delegate(GetCardInsertedStatusResponseEvent getCardInsertedStatusResponseEvent)
					{
						bool obj = false;
						lock (_lockObject)
						{
							if (OrchestrationService.PeekView() == "remove_card_view" && getCardInsertedStatusResponseEvent != null && getCardInsertedStatusResponseEvent.CardInsertedStatus == InsertedStatus.Removed)
							{
								removeCardViewModel.CardHasBeenRemoved = true;
								obj = true;
								LogHelper.Instance.Log("Chip card has been removed from the card reader.  Proceeding with post card read functionality...");
								IViewService service2 = ServiceLocator.Instance.GetService<IViewService>();
								Task.Factory.StartNew(delegate
								{
									removeCardViewModel?.StopTimeOutTimerAction?.Invoke();
									OrchestrationService.ViewGoBackSafe("chip_card_removed", popOnly: true);
									removeCardViewModel.ContinueAction();
								}, CancellationToken.None, TaskCreationOptions.None, service2?.UITaskScheduler);
							}
						}
						removeCardViewModel?.ProcessCardRemovedCheckResult(obj);
					});
				}
				else
				{
					removeCardViewModel?.ProcessCardRemovedCheckResult(obj: false);
				}
			}
			else
			{
				removeCardViewModel?.ProcessCardRemovedCheckResult(obj: true);
			}
		}
	}

	private static void AddSpeechPartTitle(ISpeechControl result, RemoveCardViewModel removeCardViewModel)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 0,
			Name = "Title",
			Loop = false,
			StartPause = 500,
			EndPause = 1000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			BaseViewController.GetViewUserControl();
			speechPart.Texts.Add(new TextPart
			{
				Text = removeCardViewModel?.TitleText
			});
		};
		result.SpeechParts.Add(speechPart);
	}

	public static void LeaveView()
	{
	}

	private static void ConfigureText(RemoveCardViewModel removeCardViewModel, RemoveCardViewParameters removeCardViewParameters)
	{
		OrchestrationService.SetCurrentUICulture();
		if (removeCardViewModel != null && removeCardViewParameters != null)
		{
			removeCardViewModel.TitleText = (removeCardViewParameters.IsQuickChip ? Resources.remove_card_view_quick_chip_title : Resources.remove_card_view_title);
		}
	}
}
