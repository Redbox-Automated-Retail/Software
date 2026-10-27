using System;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services;

public class CardFunctions
{
	private static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

	private static void UpdateCardReadAttempt(CardReadAttemptStatus cardReadAttemptStatus, ITrackData trackData)
	{
		ICardReadAttempt cardReadAttempt = Redbox.Rental.Services.Session.Session.Instance?.CardReadAttemptCollection?.GetMostRecent();
		if (cardReadAttempt != null)
		{
			if (trackData != null)
			{
				cardReadAttempt.TrackData = trackData;
			}
			cardReadAttempt.EndDateTime = DateTime.Now;
			cardReadAttempt.CardReadAttemptStatus = cardReadAttemptStatus;
		}
	}

	private static void UpdateCardReadAttemptWithTrackDataResult(ITrackData trackData)
	{
		CardReadAttemptStatus cardReadAttemptStatus = CardReadAttemptStatus.Error;
		if (trackData != null && trackData.HasValidData())
		{
			cardReadAttemptStatus = CardReadAttemptStatus.Success;
		}
		UpdateCardReadAttempt(cardReadAttemptStatus, trackData);
	}

	private static bool QuickChipWasUsedAndCardNotRemovedFromReader(ITrackData trackData, bool chipCardRemovedFromReader)
	{
		bool result = false;
		if (trackData is IEMVTrackData { CardSourceType: CardSourceType.QuickChip } && !chipCardRemovedFromReader)
		{
			result = true;
		}
		return result;
	}

	public static void CardReadComplete(SwipeViewType swipeType, ITrackData trackData, bool chipCardRemovedFromReader, Guid? requestId, Action onContinue = null, Action onError = null)
	{
		try
		{
			LogHelper.Instance.Log("Starting CardFunctions.CardReadComplete");
			ICardReadAttempt cardReadAttempt = Redbox.Rental.Services.Session.Session.Instance?.CardReadAttemptCollection?.GetMostRecent();
			if (requestId.HasValue)
			{
				bool num;
				if (cardReadAttempt == null)
				{
					num = !requestId.HasValue;
				}
				else
				{
					Guid cardReadJobCommandRequestId = cardReadAttempt.CardReadJobCommandRequestId;
					Guid? guid = requestId;
					if (!guid.HasValue)
					{
						goto IL_010f;
					}
					num = cardReadJobCommandRequestId == guid.GetValueOrDefault();
				}
				if (!num)
				{
					goto IL_010f;
				}
			}
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.TrackData = trackData;
			}
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			if (service?.UITaskScheduler != null)
			{
				Task.Factory.StartNew(delegate
				{
					LogHelper.Instance.Log("Starting UI update in CardFunctions.CardReadComplete");
					try
					{
						UpdateCardReadAttemptWithTrackDataResult(trackData);
						if (IsCurrentViewTheCardReadView(swipeType, out var cardReadViewName))
						{
							OrchestrationService.SetCurrentUICulture();
							CardReadEvent cardRead = new CardReadEvent(trackData);
							if (trackData == null || !trackData.HasValidData())
							{
								cardRead.CardReadValid = false;
								cardRead.Action = "bad_swipe_action";
								Action arg = delegate
								{
									ErrorMessageAction(cardReadViewName, cardRead.Action, onError);
								};
								Action arg2 = delegate
								{
									ErrorMessageAction(cardReadViewName, cardRead.Action, onError);
								};
								Action<Action, Action> action = MessagePopupViewController.ShowMessagePopupViewCardNotRecognized;
								ITrackData trackData2 = trackData;
								if (trackData2 != null && trackData2.NextCardReadIsInTechnicalFallback)
								{
									cardRead.Action = "technical_fallback";
									action = MessagePopupViewController.ShowMessagePopupViewCardNotRecognizedTechnicalFallback;
								}
								else if (trackData.HasErrors())
								{
									if (trackData.HasSwipedCardIsChipEnabledError())
									{
										cardRead.Action = "swiped_card_is_chip_enabled";
										action = MessagePopupViewController.ShowMessagePopupViewSwipedCardIsChipEnabled;
									}
									else if (swipeType.IsReservation() && trackData.HasReservationMobileDeviceError())
									{
										cardRead.Action = "reservation_with_mobile_wallet";
										action = MessagePopupViewController.ShowMessagePopupViewReservationMobileWalletError;
									}
									else if (trackData.HasTamperedError())
									{
										cardRead.Action = "tampered_card_reader";
										arg = delegate
										{
											ErrorMessageAction(cardReadViewName, cardRead.Action, onError);
											ServiceLocator.Instance.GetService<IRedboxRental>()?.EnterMaintenanceModeBecauseOfTamperedCardReader();
										};
										arg2 = delegate
										{
											ErrorMessageAction(cardReadViewName, cardRead.Action, onError);
											ServiceLocator.Instance.GetService<IRedboxRental>()?.EnterMaintenanceModeBecauseOfTamperedCardReader();
										};
										action = ((!swipeType.IsReservation()) ? new Action<Action, Action>(MessagePopupViewController.ShowMessagePopupViewCheckoutCardReaderTampered) : new Action<Action, Action>(MessagePopupViewController.ShowMessagePopupViewReservationCardReaderTampered));
									}
									else if (trackData.CardSourceTypeIsEMVContactOrQuickChip())
									{
										cardRead.Action = "bad_chip_read_action";
										action = MessagePopupViewController.ShowMessagePopupViewBadChipReadError;
									}
									else if (trackData.CardSourceTypeIsEMVContactlessOrMobileOrMSDContactless())
									{
										cardRead.Action = "bad_contactless_read_action";
										action = MessagePopupViewController.ShowMessagePopupViewBadContactlessReadError;
									}
									else if (trackData.WalletTypeIsGoogleAndNotVas())
									{
										cardRead.Action = "google_not_vas_action";
										action = MessagePopupViewController.ShowMessagePopupViewGoogleNoPassFoundError;
									}
								}
								AnalyticsService?.AddViewEvent(cardRead);
								action(arg, arg2);
							}
							else
							{
								cardRead.CardReadValid = true;
								cardRead.Action = "continue";
								AnalyticsService?.AddViewEvent(cardRead);
								if (QuickChipWasUsedAndCardNotRemovedFromReader(trackData, chipCardRemovedFromReader))
								{
									OrchestrationService.ViewShow("remove_card_view", new RemoveCardViewParameters
									{
										IsQuickChip = true,
										ContinueAction = onContinue
									});
								}
								else
								{
									onContinue?.Invoke();
								}
							}
						}
						else
						{
							LogHelper.Instance.Log("CardFunctions.CardReadComplete: current view is not " + cardReadViewName + ".  Card read data will not be processed.");
						}
					}
					catch (Exception e2)
					{
						LogHelper.Instance.Log("An unhandled exception was raised in CardFunctions.CardReadComplete.", e2);
					}
				}, CancellationToken.None, TaskCreationOptions.None, service?.UITaskScheduler);
			}
			else
			{
				LogHelper.Instance.Log("ViewService UITaskScheduler is null in CardFunctions.CardReadComplete");
			}
			return;
			IL_010f:
			LogHelper.Instance.Log($"ViewService Received a response for request Id {requestId} which is not the current read attempt.");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in CardFunctions.CardReadComplete.", e);
		}
	}

	private static bool IsCurrentViewTheCardReadView(SwipeViewType swipeType, out string cardReadViewName)
	{
		cardReadViewName = ((swipeType == SwipeViewType.ZeroTouchReservationPickup) ? "start_view" : "swipe_view");
		bool result = false;
		string text = OrchestrationService.PeekView();
		if (swipeType != SwipeViewType.ZeroTouchReservationPickup && text == "swipe_view")
		{
			result = true;
		}
		else if (swipeType == SwipeViewType.ZeroTouchReservationPickup)
		{
			if (text == "start_view")
			{
				result = true;
			}
			else if (text == "please_wait_view" && OrchestrationService.PeekView(1) == "start_view")
			{
				result = true;
			}
		}
		return result;
	}

	private static void ErrorMessageAction(string cardReadViewName, string cardReadAction, Action onError)
	{
		OrchestrationService.UnwindToSafe(cardReadViewName, discard: true, cardReadAction);
		onError?.Invoke();
	}
}
