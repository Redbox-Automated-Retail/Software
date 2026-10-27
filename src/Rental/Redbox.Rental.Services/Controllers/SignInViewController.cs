using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Personalization;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class SignInViewController : BaseViewController
{
	private static IReadCardJob _currentReadCardJob = null;

	private static readonly ManualResetEvent _cardReadComplete = new ManualResetEvent(initialState: true);

	private static bool PerksPassEnabled
	{
		get
		{
			IDeviceServiceClientHelper service = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
			if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.DeviceService.ContactlessEnabled && Redbox.Rental.Services.Configuration.Configuration.Instance.PerksPassEnabled)
			{
				return service?.SupportsVas ?? false;
			}
			return false;
		}
	}

	public static void ConfigureSignInView(SignInViewParameters signInViewParameters)
	{
		BaseViewController.StartIdleTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.SetCurrentUICulture();
			SignInViewModel signInViewModel = new SignInViewModel();
			string text = ((signInViewParameters.SignUpType == SignUpType.Perks) ? "perks-white-logo.png" : "logoWhite2_icon.png");
			signInViewModel.HeaderImage = new BitmapImage(new Uri("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/" + text));
			string styleName = ((signInViewParameters.SignUpType == SignUpType.Perks) ? "sign_in_up_perks_header_image" : "sign_in_up_redbox_header_image");
			signInViewModel.HeaderImageStyle = WPFFunctions.GetStyle(viewUserControl, styleName);
			signInViewModel.HeaderLabelText = ((signInViewParameters.SignUpType == SignUpType.Perks) ? Resources.sign_in_header_text : Resources.sign_in_redbox_plus_header_text);
			signInViewModel.AlreadyMemberText = Resources.sign_in_already_member_text;
			signInViewModel.SignInLabelText = Resources.sign_in;
			signInViewModel.EmailAndPasswordButtonText = Resources.sign_in_email_and_password_button_text;
			signInViewModel.PhoneAndPinButtonText = Resources.sign_in_phone_and_pin_button_text;
			signInViewModel.MessageTextVisibility = ((signInViewParameters.SignUpType == SignUpType.Perks) ? Visibility.Collapsed : Visibility.Visible);
			signInViewModel.MessageText = Resources.sign_in_redbox_plus_message_text;
			signInViewModel.RegisteredTrademarkSymbol = ((signInViewParameters.SignUpType == SignUpType.RedboxPlus) ? string.Empty : Resources.registered_trademark_symbol);
			signInViewModel.BulletVisibility = ((signInViewParameters.SignUpType != SignUpType.Perks) ? Visibility.Collapsed : Visibility.Visible);
			bool displayUpdatedPerksPoints = Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.DisplayUpdatedPerksPoints;
			signInViewModel.Bullet1Text = (displayUpdatedPerksPoints ? Resources.sign_in_point_one_text_updated : Resources.sign_in_point_one_text);
			signInViewModel.Bullet2Text = Resources.sign_in_point_two_text;
			signInViewModel.Bullet3Text = Resources.sign_in_point_three_text;
			signInViewModel.CancelButtonText = Resources.sign_in_cancel_button_text;
			signInViewModel.BottomMessageText = Resources.sign_in_bottom_message_text;
			signInViewModel.LegalText = ((signInViewParameters.SignUpType != SignUpType.Perks) ? null : (displayUpdatedPerksPoints ? Resources.sign_in_legal_text_updated : Resources.sign_in_legal_text));
			signInViewModel.SignUpButtonText = Resources.button_sign_up;
			signInViewModel.InfoButtonText = Resources.sign_up_info_button_text;
			signInViewModel.InfoButtonVisibility = ((signInViewParameters.SignUpType != SignUpType.Perks) ? Visibility.Collapsed : Visibility.Visible);
			signInViewModel.TermsButtonText = Resources.terms_link_text;
			signInViewModel.PrivacyText = (string.IsNullOrEmpty(Redbox.Rental.Services.Configuration.Configuration.Instance.CAPrivacyNoticeDocument) ? null : Resources.privacy_notice_text);
			signInViewModel.MobileSignInButtonText = Resources.sign_in_mobile_button_text;
			signInViewModel.MobileMessageText = Resources.sign_in_mobile_message_text;
			signInViewModel.MobileSignInEnabled = PerksPassEnabled;
			signInViewModel.ButtonTextStyle = WPFFunctions.GetStyle(viewUserControl, "font_montserrat_extrabold_20");
			signInViewModel.EmailAndPasswordButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				EmailAndPasswordButtonClicked(signInViewParameters);
			}, (Func<bool>)null);
			signInViewModel.PhoneAndPinButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				PhoneAndPinButtonClicked(signInViewParameters);
			}, (Func<bool>)null);
			signInViewModel.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				CancelButtonClicked();
			}, (Func<bool>)null);
			signInViewModel.SignUpButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				SignUpButtonAction(signInViewParameters);
			}, (Func<bool>)null);
			signInViewModel.InfoButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				InfoButtonAction();
			}, (Func<bool>)null);
			signInViewModel.TermsButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				TermsButtonAction();
			}, (Func<bool>)null);
			signInViewModel.MobileSignInButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				MobileSignInButtonAction();
			}, (Func<bool>)null);
			ConfigureTTS(signInViewModel);
			viewUserControl.DataContext = signInViewModel;
			if (!PersonalizationFunctions.IsSignedIn)
			{
				ReadVas();
			}
		}
	}

	public static void LeaveView()
	{
		CancelVas(CancelReason.LeavingSignInView);
	}

	public static void MobileSignInZeroTouchSwipeAction()
	{
		BaseViewController.StopIdleTimer();
		SwipeViewController.SetSwipeViewParameters2(ZeroTouchSwipeViewCancel, SwipeViewContinue, SwipeViewType.EMVTapToLogin, VASOnlyCardReadImmediateAction);
		DisplayView("swipe_view");
	}

	private static void TermsButtonAction()
	{
		HelpViewController.ShowTermsAndConditions();
	}

	private static SignInViewModel GetSignInViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as SignInViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(SignInViewModel signInViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			signInViewModel.OnGetSpeechControl = () => SignInViewTextToSpeech.GetSpeechControl(GetSignInViewModel);
		}
	}

	private static void EmailAndPasswordButtonClicked(SignInViewParameters signInViewParameters)
	{
		BaseViewController.StopIdleTimer();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SignInEmail");
		if (signInViewParameters.SignUpType == SignUpType.Perks)
		{
			OrchestrationService.ViewGoBackSafe("SignInViewController email sign in", popOnly: true);
		}
		PersonalizationFunctions.ShowEmailLogin(SignInCancelButtonTypes.Back, signInViewParameters?.AfterSignInAction);
	}

	private static void PhoneAndPinButtonClicked(SignInViewParameters signInViewParameters)
	{
		BaseViewController.StopIdleTimer();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SignInPhone");
		if (signInViewParameters.SignUpType == SignUpType.Perks)
		{
			OrchestrationService.ViewGoBackSafe("SignInViewController phone sign in", popOnly: true);
		}
		PersonalizationFunctions.ShowPhoneNumberLogin(SignInCancelButtonTypes.Back, signInViewParameters?.AfterSignInAction);
	}

	private static void CancelButtonClicked()
	{
		BaseViewController.StopIdleTimer();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SignInCancel");
		OrchestrationService.ViewGoBackSafe("SignInViewController cancel", popOnly: false);
	}

	private static void SignUpButtonAction(SignInViewParameters signInViewParameters)
	{
		BaseViewController.StopIdleTimer();
		OrchestrationService.SetCurrentUICulture();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SignUp");
		signInViewParameters?.SignUpAction();
	}

	private static void InfoButtonAction()
	{
		BaseViewController.StopIdleTimer();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SignUpInfo");
		OrchestrationService.ViewShow("perks_signup_info_popup_view");
	}

	private static void MobileSignInButtonAction()
	{
		BaseViewController.StopIdleTimer();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("MobilePerksPass");
		SwipeViewController.SetSwipeViewParameters2(SwipeViewCancel, SwipeViewContinue, SwipeViewType.EMVTapToLogin, VASOnlyCardReadImmediateAction);
		DisplayView("swipe_view");
	}

	private static void SwipeViewContinue()
	{
	}

	private static void SwipeViewCancel()
	{
		OrchestrationService.UnwindToSafe("sign_in_view", discard: false, "perks_pass_login:swipe_view_cancel_func");
	}

	private static void ZeroTouchSwipeViewCancel()
	{
		OrchestrationService.UnwindToSafe("browse_view", discard: false, "perks_pass_login:zero_touch_swipe_view_cancel_func");
	}

	private static void ReadVas()
	{
		if (PerksPassEnabled && ServiceLocator.Instance.GetService<ITrackDataService>() is IDeviceServiceTrackDataService deviceServiceTrackDataService)
		{
			_cardReadComplete.Reset();
			CardReadRequestInfo request = new CardReadRequestInfo
			{
				Timeout = 60000,
				IsZeroTouchRead = false,
				InputType = DeviceInputType.C,
				VasMode = VasMode.VasOnly,
				SessionId = (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ShoppingSessionId ?? default(Guid))
			};
			deviceServiceTrackDataService.StartCardRead(request, OnCardReadStarting, OnCardReadCustomerInputStart, OnVasReadComplete);
		}
	}

	private static void OnCardReadStarting(IReadCardJob rcj)
	{
		_currentReadCardJob = rcj;
		LogHelper.Instance.Log($"Vas Read Starting RequestId: {rcj?.RequestId}");
	}

	private static void OnCardReadCustomerInputStart(Guid requestId)
	{
		LogHelper.Instance.Log($"Vas Read Input Starting - RequestId {requestId}");
	}

	private static void OnVasReadComplete(CardReadRequestInfo request, CardReadCompleteDetails details)
	{
		_cardReadComplete.Set();
		if (_currentReadCardJob?.RequestId != details?.RequestId && details != null && details.RequestId.HasValue)
		{
			BaseViewController.Log($"OnVasReadComplete has mismatch in current request ids skip processing read complete - Current: {_currentReadCardJob?.RequestId}, Received: {details?.RequestId}");
		}
		else
		{
			AfterVASCardReadAction(request, details);
		}
	}

	private static void VASOnlyCardReadImmediateAction(CardReadRequestInfo request, CardReadCompleteDetails details, Action swipeContinueAction)
	{
		AfterVASCardReadAction(request, details);
	}

	private static void AfterVASCardReadAction(CardReadRequestInfo request, CardReadCompleteDetails details)
	{
		CardReadEvent cardReadEvent = new CardReadEvent(details?.TrackData);
		bool flag = details?.Success ?? false;
		cardReadEvent.CardReadValid = flag && details?.TrackData?.HasVas == true;
		bool flag2 = details?.TrackData != null && details.TrackData.ReadStatus == ResponseStatus.Cancelled;
		cardReadEvent.Action = (cardReadEvent.CardReadValid ? "continue" : (flag2 ? "read_cancelled" : "bad_swipe_action"));
		ServiceLocator.Instance?.GetService<IAnalyticsService>()?.AddViewEvent(cardReadEvent);
		if (flag && details?.TrackData?.HasVas == true)
		{
			Task.Factory.StartNew(delegate
			{
				PersonalizationFunctions.StartLoginProcess(null, null, null, null, details.VasData, details.TrackData.WalletType, delegate(LoginType loginType, ILoginResult loginResult)
				{
					afterMobilePassLoginAction(loginType, loginResult);
				});
			}, CancellationToken.None, TaskCreationOptions.None, BaseViewController.GetViewService()?.UITaskScheduler);
			return;
		}
		if (details?.TrackData == null || (details.TrackData.ReadStatus != ResponseStatus.Disconnected && details.TrackData.ReadStatus != ResponseStatus.Cancelled && details.TrackData.ReadStatus != ResponseStatus.Timeout))
		{
			ITrackData trackData = details.TrackData;
			if (trackData == null || !trackData.Errors.Any((Redbox.KioskEngine.ComponentModel.Error x) => string.Compare(x.Description, "READERDISCONNECT") == 0))
			{
				KioskLoginResponseErrors kioskLoginResponseError = KioskLoginResponseErrors.MobilePassReadError;
				if (details?.TrackData != null && details.TrackData.WalletTypeIsGoogleAndNotVas())
				{
					kioskLoginResponseError = KioskLoginResponseErrors.MobilePassGoogleNoPassFoundError;
				}
				PersonalizationFunctions.ShowPerksPassLoginError(new LoginResult
				{
					KioskLoginResponseError = kioskLoginResponseError
				});
				return;
			}
		}
		LogHelper.Instance.Log($"Card Read Not successful.  ReadStatus = {details?.TrackData?.ReadStatus}");
	}

	private static void afterMobilePassLoginAction(LoginType loginType, ILoginResult loginResult)
	{
		if (loginResult != null && loginResult.Success)
		{
			OrchestrationService.ViewShow("sign_in_thanks_view", new SignInThanksViewParameters
			{
				DelaySeconds = 3,
				EmailAddress = loginResult.PersonalizationSession.LoginEmailAddress,
				ContinueAction = delegate
				{
					if ((BaseViewController.GetViewService()?.PeekViewFrameByName("sign_in_view", out var _))?.Parameter is SignInViewParameters { SignUpType: SignUpType.RedboxPlus })
					{
						PersonalizationFunctions.AfterRedboxPlusSignInAction(loginType, loginResult);
					}
					else
					{
						PersonalizationFunctions.AfterRegularSignInAction(loginType, loginResult);
					}
				}
			});
		}
		else
		{
			PersonalizationFunctions.ShowPerksPassLoginError(loginResult);
		}
	}

	private static void DisplayView(string viewName, object parameters = null)
	{
		ServiceLocator.Instance.GetService<IEngineApplication>().ThreadSafeHostUpdate(delegate
		{
			OrchestrationService.ViewShow(viewName, parameters);
		});
	}

	private static void CancelVas(CancelReason cancelReason)
	{
		ServiceLocator.Instance.GetService<ITrackDataService>()?.StopCardRead(cancelReason);
		_cardReadComplete.WaitOne(3000);
	}
}
