using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class ReservationDetailsViewController : BaseViewController
{
	public class Constants
	{
		public const string ReservationDetailsViewTimer = "reservation_details_view_timer";

		public const string Intro = "Intro";
	}

	private static object _lock = new object();

	private static Action _continueAction;

	public static void ConfigureView(Action continueAction)
	{
		_continueAction = continueAction;
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.SetCurrentUICulture();
			ReservationDetailsViewModel reservationDetailsViewModel = new ReservationDetailsViewModel();
			reservationDetailsViewModel.OkayButtonAction += OkayButtonClickedAction;
			PopulateModelText(reservationDetailsViewModel);
			ConfigureTTS(reservationDetailsViewModel);
			viewUserControl.DataContext = reservationDetailsViewModel;
			reservationDetailsViewModel.OkayButtonVisibility = (Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode ? Visibility.Hidden : Visibility.Visible);
			ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			int value = ((service != null && service.TTSEnabled) ? 25000 : 15000);
			if (Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode)
			{
				value = 5000;
			}
			(ServiceLocator.Instance.GetService<ITimerService>()?.CreateTimer("reservation_details_view_timer", value, value, delegate
			{
				LogHelper.Instance.Log("reservation_details_view_timer executed");
				ExecuteCallback(fromOkayButton: false);
			}))?.Start();
		}
	}

	private static void PopulateModelText(ReservationDetailsViewModel reservationDetailsViewModel)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		List<IReservation> accountReservations = ReservationFunctions.GetAccountReservations();
		if (accountReservations != null)
		{
			num = accountReservations.Sum((IReservation x) => x.ReservationDetails.ReservedTitles.Count);
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			if (service != null)
			{
				foreach (IReservation item in accountReservations)
				{
					foreach (IReservedTitle reservedTitle in item.ReservationDetails.ReservedTitles)
					{
						IKioskProduct kioskProduct = null;
						service?.GetProductByBarcode(reservedTitle.Barcode, out kioskProduct);
						if (kioskProduct is MDVProduct { DiscNumber: var discNumber } mDVProduct && discNumber == 1)
						{
							num3++;
							num2 = mDVProduct.TitleProducts.Count;
						}
					}
				}
			}
		}
		reservationDetailsViewModel.MDVVisibility = ((num3 <= 0) ? Visibility.Collapsed : Visibility.Visible);
		reservationDetailsViewModel.HeaderText = Resources.reservation_pickup_header;
		string text = Resources.discs_text?.ToLower();
		if (num == 1)
		{
			text = text.Substring(0, text.Length - 1);
		}
		string arg = string.Format(text, num);
		reservationDetailsViewModel.BodyText = string.Format(Resources.reservation_pickup_body, arg);
		reservationDetailsViewModel.DiscVendText = $"{num2} {Resources.disc_vend}";
		reservationDetailsViewModel.NoteHeaderText = Resources.reservation_pickup_note_header;
		reservationDetailsViewModel.OkayButtonText = Resources.reservation_details_okay;
		string format;
		string format2;
		if (num3 == 1)
		{
			format = Resources.reservation_pickup_note_body_single_mdv;
			format2 = Resources.reservation_pickup_return_warning_single_mdv;
		}
		else
		{
			format = Resources.reservation_pickup_note_body_multiple_mdv;
			format2 = Resources.reservation_pickup_return_warning_multiple_mdv;
		}
		reservationDetailsViewModel.NoteBodyText = string.Format(format, num3, num2);
		reservationDetailsViewModel.ReturnWarningText = string.Format(format2, num3, num2);
	}

	private static void ConfigureTTS(ReservationDetailsViewModel multiDiscVendViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled || multiDiscVendViewModel == null)
		{
			return;
		}
		multiDiscVendViewModel.OnGetSpeechControl = delegate
		{
			SpeechControl obj = new SpeechControl
			{
				Name = "tts_reservation_details_view"
			};
			SpeechPart part = new SpeechPart
			{
				Name = "Intro",
				Loop = false,
				StartPause = 250,
				EndPause = 5000
			};
			part.Refresh = delegate
			{
				part.Clear();
				part.Texts.Add(new TextPart
				{
					Text = multiDiscVendViewModel?.HeaderText
				});
				part.Texts.Add(new TextPart
				{
					Text = multiDiscVendViewModel?.BodyText
				});
				part.Texts.Add(new TextPart
				{
					Text = multiDiscVendViewModel?.NoteHeaderText
				});
				part.Texts.Add(new TextPart
				{
					Text = multiDiscVendViewModel?.NoteBodyText
				});
				part.Texts.Add(new TextPart
				{
					Text = multiDiscVendViewModel?.ReturnWarningText
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

	private static string GetNumberSpelledOut(int number)
	{
		string result = number.ToString();
		if (number >= 1 && number <= 5)
		{
			string name = $"number_{number}";
			result = Capitalize(Resources.ResourceManager.GetString(name).ToLower());
		}
		return result;
	}

	private static string Capitalize(string textToCapitalize)
	{
		return new CultureInfo(ServiceLocator.Instance.GetService<IResourceBundleService>()?.Filter["culture"] ?? null, useUserOverride: false).TextInfo.ToTitleCase(textToCapitalize);
	}

	private static void OkayButtonClickedAction()
	{
		ExecuteCallback(fromOkayButton: true);
	}

	private static void ExecuteCallback(bool fromOkayButton)
	{
		ReservationDetailsViewModel reservationDetailsViewModel = BaseViewController.GetViewModel<ReservationDetailsViewModel>();
		if (reservationDetailsViewModel == null)
		{
			return;
		}
		LockHelper.TryLockObject(_lock, delegate
		{
			if (!reservationDetailsViewModel.CallbackExecuted)
			{
				reservationDetailsViewModel.CallbackExecuted = true;
				if (fromOkayButton)
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("ReservationDetailsOkay");
				}
				_continueAction?.Invoke();
			}
			else
			{
				LogHelper.Instance.Log("ReservationDetailsView.ExecuteCallback was already executed.");
			}
		}, "ReservationDetailsView.ExecuteCallback");
	}

	public static void LeaveView()
	{
		OrchestrationService.RemoveTimer("reservation_details_view_timer");
	}
}
