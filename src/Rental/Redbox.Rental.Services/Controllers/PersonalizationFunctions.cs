using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Model.Transaction;
using Redbox.Rental.Services.RedboxPlus;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class PersonalizationFunctions
{
	private static IRedboxPlusSubscriptionProduct _redboxPlusSubscriptionProduct;

	private static DateTime _loginStartTime;

	public const string EARLY_ID_INDICATOR = "EarlyIdIndicator";

	public const string EARLY_ID_APPLE = "EarlyIdApple";

	public const string EARLY_ID_GOOGLE = "EarlyIdGoogle";

	public static bool IsSignedIn => Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.IsLoggedIn == true;

	public static bool IsSignInEnabled
	{
		get
		{
			IPersonalizationService service = ServiceLocator.Instance.GetService<IPersonalizationService>();
			if (!IsSignedIn)
			{
				return service?.IsEarlyIdEnabled ?? false;
			}
			return false;
		}
	}

	public static bool IsLoyaltyEnabled => ServiceLocator.Instance.GetService<IPersonalizationService>()?.IsLoyaltyEnabled ?? false;

	public static bool IsLoyaltySignedIn
	{
		get
		{
			if (IsSignedIn && IsLoyaltyEnabled && Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession != null)
			{
				return !Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.LoyaltySession.SignInError;
			}
			return false;
		}
	}

	public static bool IsLoyaltySignInError
	{
		get
		{
			if (IsSignedIn && IsLoyaltyEnabled)
			{
				return Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession?.SignInError == true;
			}
			return false;
		}
	}

	public static int? LoyaltyPointsBalance
	{
		get
		{
			if (!IsSignedIn)
			{
				return null;
			}
			return Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession?.PointsBalance;
		}
	}

	private static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

	public static bool CanAddRedboxPlusSubscription()
	{
		bool flag = false;
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		List<ISubscriptionProduct> subscriptionProducts = null;
		ServiceLocator.Instance.GetService<IDataService>()?.GetSubscriptionProducts(out subscriptionProducts);
		if (personalizationSession?.SubscriptionData?.EligibleSubscriptionPlanIds != null && subscriptionProducts != null)
		{
			foreach (string eachEligibleSubscriptionPlanId in personalizationSession.SubscriptionData.EligibleSubscriptionPlanIds)
			{
				if (subscriptionProducts.Any((ISubscriptionProduct x) => x.SubscriptionType == SubscriptionType.RedboxPlus && x.SubscriptionId == eachEligibleSubscriptionPlanId))
				{
					flag = true;
					break;
				}
			}
		}
		return (personalizationSession?.RedboxPlusSession?.HasActiveSubscription != true) & flag;
	}

	public static void ShowRedboxPlusSignInSignUpDialog(IRedboxPlusSubscriptionProduct redboxPlusSubscriptionProduct)
	{
		_redboxPlusSubscriptionProduct = redboxPlusSubscriptionProduct;
		SignInViewParameters parameter = new SignInViewParameters
		{
			AfterSignInAction = delegate(LoginType loginType, ILoginResult loginResult)
			{
				AfterEmailOrPhoneAndPinSignInAction(loginType, loginResult, AfterRedboxPlusSignInAction);
			},
			SignUpAction = RedboxPlusSignUpAction,
			SignUpType = SignUpType.RedboxPlus
		};
		OrchestrationService.ViewShow("sign_in_view", parameter);
	}

	public static void ShowRegularSignInSignUpDialog()
	{
		SignInViewParameters parameter = new SignInViewParameters
		{
			AfterSignInAction = delegate(LoginType loginType, ILoginResult loginResult)
			{
				AfterEmailOrPhoneAndPinSignInAction(loginType, loginResult, AfterRegularSignInAction);
			},
			SignUpAction = RegularSignUpAction,
			SignUpType = SignUpType.Perks
		};
		OrchestrationService.ViewShow("sign_in_view", parameter);
	}

	private static void RegularSignUpAction()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		string parentView = service.Peek(1);
		ServiceLocator.Instance.GetService<IPerksSignupService>()?.StartSignupFlow(delegate
		{
			OrchestrationService.UnwindToSafe(parentView, discard: true, "SignInViewController sign up");
		}, SignUpFlowContext.SignUp, delegate
		{
			OrchestrationService.InvokeIdleTimerExpireSessionCallback();
		});
	}

	private static void AfterEmailOrPhoneAndPinSignInAction(LoginType loginType, ILoginResult loginResult, Action<LoginType, ILoginResult> afterLoginAction)
	{
		if (loginResult != null && (loginResult.Success || loginResult.KioskLoginResponseError == KioskLoginResponseErrors.LoyaltyAccountNotFound))
		{
			OrchestrationService.ViewGoBackSafe("Login process finished - pop keyboard / phone view", popOnly: false);
			OrchestrationService.StopIdleTimer("idle_timer");
			if (IsSignedIn)
			{
				OrchestrationService.ViewShow("sign_in_thanks_view", new SignInThanksViewParameters
				{
					DelaySeconds = 3,
					EmailAddress = loginResult.PersonalizationSession.LoginEmailAddress,
					ContinueAction = delegate
					{
						afterLoginAction?.Invoke(loginType, loginResult);
					}
				});
			}
			else
			{
				afterLoginAction?.Invoke(loginType, loginResult);
			}
			return;
		}
		if (loginResult.KioskLoginResponseError == KioskLoginResponseErrors.LoyaltyIsDown)
		{
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession = loginResult.PersonalizationSession;
			}
			OrchestrationService.ViewGoBackSafe("Login process finished - pop Keyboard view / phone and pin view", popOnly: true);
			ShowLoyaltyIsDownOnLoginDialog(delegate
			{
				afterLoginAction?.Invoke(loginType, loginResult);
			});
			return;
		}
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		IViewFrameInstance viewFrameInstance = service?.PeekViewFrame();
		if (service == null || viewFrameInstance == null)
		{
			return;
		}
		if (viewFrameInstance?.ViewFrame.ViewName == "phone_and_pin_view")
		{
			LogHelper.Instance.Log("returning to phone and pin view to display error");
			PhoneAndPinInfo phoneAndPinInfo = viewFrameInstance.Parameter as PhoneAndPinInfo;
			if (phoneAndPinInfo?.PhoneAndPinViewModel != null)
			{
				phoneAndPinInfo.PhoneAndPinViewModel.ErrorMessageText = loginResult.ErrorMessage;
				phoneAndPinInfo.PhoneAndPinViewModel.SetDefaultDisplays();
			}
		}
		else
		{
			LogHelper.Instance.Log("returning to keyboard view to display error");
			if (viewFrameInstance.Parameter is KeyboardInfo keyboardInfo)
			{
				keyboardInfo.ErrorMessage = loginResult.ErrorMessage;
				keyboardInfo.KeyboardText = string.Empty;
				keyboardInfo.KeyboardCode = string.Empty;
			}
		}
		service.Show();
	}

	public static void AfterRegularSignInAction(LoginType loginType, ILoginResult loginResult)
	{
		ILoginResult loginResult2 = loginResult;
		if ((loginResult2 == null || !loginResult2.Success) && loginResult.KioskLoginResponseError != KioskLoginResponseErrors.LoyaltyAccountNotFound)
		{
			return;
		}
		if (IsSignedIn && loginType == LoginType.Email)
		{
			Action<ISignupFlowResult> action = delegate
			{
				IViewFrameInstance afterSignInUnwindToView2 = GetAfterSignInUnwindToView();
				if (afterSignInUnwindToView2 != null)
				{
					OrchestrationService.UnwindToSafe(afterSignInUnwindToView2?.ViewFrame?.ViewName, discard: true, "sign_up_flow_return");
				}
				ShowRecommendedTitlesIfNeeded(loginResult);
			};
			ServiceLocator.Instance.GetService<IPerksSignupService>()?.StartSignupFlow(action, SignUpFlowContext.AfterSignIn, action);
		}
		else if (IsSignedIn && loginType == LoginType.Mobile)
		{
			IViewFrameInstance afterSignInUnwindToView = GetAfterSignInUnwindToView();
			if (afterSignInUnwindToView != null)
			{
				OrchestrationService.UnwindToSafe(afterSignInUnwindToView?.ViewFrame?.ViewName, discard: true, "sign_up_flow_return");
				ShowRecommendedTitlesIfNeeded(loginResult);
			}
		}
		else if (IsSignedIn)
		{
			ShowRecommendedTitlesIfNeeded(loginResult);
		}
	}

	private static IViewFrameInstance GetAfterSignInUnwindToView()
	{
		IViewFrameInstance viewFrameInstance = OrchestrationService.PeekViewFrameByName("shopping_cart_view", out var index);
		if (viewFrameInstance == null)
		{
			viewFrameInstance = OrchestrationService.PeekViewFrameByName("browse_view", out index);
		}
		return viewFrameInstance;
	}

	public static void AfterRedboxPlusSignInAction(LoginType loginType, ILoginResult loginResult)
	{
		RedboxPlusSignupService.StartRedboxPlusSignup(_redboxPlusSubscriptionProduct);
	}

	private static void RedboxPlusSignUpAction()
	{
		RedboxPlusSignupService.StartRedboxPlusSignup(_redboxPlusSubscriptionProduct);
	}

	public static void ShowEmailLogin(SignInCancelButtonTypes signInCancelButtonType, Action<LoginType, ILoginResult> afterLoginAction)
	{
		LogHelper.Instance.Log("Starting login with email.");
		OrchestrationService.ViewShow("keyboard_sign_in_view", new KeyboardInfo
		{
			MaxLength = 64,
			KeyboardText = "",
			KeepKeyboardTextOnInitialErrorMessage = true,
			ForgotPasswordAction = delegate
			{
				MessagePopupViewController.ShowForgotPasswordModal(delegate
				{
					OrchestrationService.ViewGoBackSafe("Forgot Password modal time out", popOnly: false);
				}, delegate
				{
					OrchestrationService.ViewGoBackSafe("Forgot Password modal continue", popOnly: false);
				});
			},
			CancelAction = delegate
			{
				OrchestrationService.ViewGoBackSafe("Email login cancel", popOnly: false);
			},
			IsValidEntries = ValidationServices.IsValidEmailAndCode,
			SignInCancelButtonType = signInCancelButtonType,
			ContinueCodeAction = delegate(string emailAddress, SecureString password)
			{
				StartLoginProcess(emailAddress, password, null, null, null, WalletType.None, afterLoginAction);
			}
		});
	}

	public static void ShowPhoneNumberLogin(SignInCancelButtonTypes signInCancelButtonType, Action<LoginType, ILoginResult> afterLoginAction)
	{
		LogHelper.Instance.Log("Starting login with phone number.");
		OrchestrationService.ViewShow("phone_and_pin_view", new PhoneAndPinInfo
		{
			SignInCancelButtonType = signInCancelButtonType,
			SignInAction = delegate(string phone, SecureString pin, bool textClub)
			{
				StartLoginProcess(null, null, phone, pin, null, WalletType.None, afterLoginAction);
			},
			IsSignUp = false
		});
	}

	public static void ShowLogoutDialog()
	{
		MessagePopupViewController.ShowLogOutWarning(delegate
		{
			OrchestrationService.ViewGoBackSafe("Message view Timed out.", popOnly: false);
		}, delegate
		{
			AnalyticsService?.AddButtonPressEvent("PersonalizationFunctionsSignOutBack");
			OrchestrationService.ViewGoBackSafe("Sign Out view:hide()", popOnly: false);
		}, delegate
		{
			AnalyticsService?.AddButtonPressEvent("PersonalizationFunctionsSignOut");
			Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LogOut();
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.EmailConfirmResult = null;
			}
			ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart.ClearAllLoyaltyPointsRecords();
			LogHelper.Instance.Log("User has signed out.  Closing session and returning to welcome screen.");
			ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession(null);
		});
	}

	public static void ShowLoyaltyIsDownDialog()
	{
		MessagePopupViewController.ShowLoyaltyIsDownMessage(delegate
		{
			OrchestrationService.ViewGoBackSafe("Loyalty is down message popup timeout", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("Loyalty is down message popup continue", popOnly: false);
		});
	}

	public static void ShowLoyaltyIsDownOnLoginDialog(Action continueAction)
	{
		MessagePopupViewController.ShowLoyaltyIsDownOnLoginMessage(delegate
		{
			OrchestrationService.ViewGoBackSafe("Loyalty is down on login message popup timeout", popOnly: false);
			continueAction?.Invoke();
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("Loyalty is down on login message popup continue", popOnly: false);
			continueAction?.Invoke();
		});
	}

	public static void StartLoginProcess(string emailAddress, SecureString password, string phoneNumber, SecureString pin, string customerProfileNumber, WalletType walletType, Action<LoginType, ILoginResult> afterLoginAction)
	{
		LoginType loginType = ((!string.IsNullOrEmpty(customerProfileNumber)) ? LoginType.Mobile : (string.IsNullOrEmpty(emailAddress) ? LoginType.Phone : LoginType.Email));
		PleaseWaitParameters parameter = ((loginType == LoginType.Mobile) ? new PleaseWaitParameters
		{
			MessageText = Resources.please_wait_processing_perks_pass
		} : null);
		OrchestrationService.ViewShow("please_wait_view", parameter);
		ServiceLocator.Instance.GetService<IPersonalizationService>();
		IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
		_loginStartTime = DateTime.Now;
		Action<ILoginResult> loginResultCallback = delegate(ILoginResult loginResult)
		{
			ProcessLoginResult(loginResult, loginType, walletType, afterLoginAction);
		};
		if (loginType == LoginType.Mobile)
		{
			service?.CustomerProfile?.LoginWithCustomerProfileNumber(customerProfileNumber, loginResultCallback);
		}
		else if (loginType == LoginType.Email)
		{
			service?.CustomerProfile?.LoginWithEmail(emailAddress, password, loginResultCallback);
		}
		else
		{
			service?.CustomerProfile?.LoginWithPhoneNumber(phoneNumber, pin, loginResultCallback);
		}
	}

	private static void ProcessLoginResult(ILoginResult loginResult, LoginType loginType, WalletType walletType, Action<LoginType, ILoginResult> afterLoginAction)
	{
		TimeSpan timeRemaining = _loginStartTime.AddMilliseconds(4500.0).Subtract(DateTime.Now);
		Task.Run(async delegate
		{
			if (timeRemaining > new TimeSpan(0L))
			{
				await Task.Delay(timeRemaining);
			}
		}).ContinueWith(delegate
		{
			LogHelper.Instance.Log("Processing login result: success={0} responseError={1}", loginResult?.Success, loginResult?.KioskLoginResponseError);
			LoginEvent evt = new LoginEvent
			{
				Success = (loginResult?.Success ?? false),
				LoginType = loginType.ToString(),
				WalletType = walletType.ToString(),
				Error = (loginResult?.KioskLoginResponseError.ToString() ?? KioskLoginResponseErrors.None.ToString())
			};
			ServiceLocator.Instance?.GetService<IAnalyticsService>()?.AddViewEvent(evt);
			ServiceLocator.Instance.GetService<IViewService>();
			OrchestrationService.ViewGoBackSafe("Login process finished - pop PleaseWaitView", popOnly: true);
			if (loginResult != null && (loginResult.Success || loginResult.KioskLoginResponseError == KioskLoginResponseErrors.LoyaltyAccountNotFound))
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					loginResult.Success = true;
					Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession = loginResult.PersonalizationSession;
					Redbox.Rental.Services.Session.Session.Instance.BrowseState.ShowRedboxPlusTooltip = RedboxPlusFunctions.IsFreeOneNightRentalAvailable;
					Redbox.Rental.Services.Session.Session.Instance.UserInfo = new UserInfo
					{
						Email = loginResult.PersonalizationSession.LoginEmailAddress
					};
					if (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.RedboxPlusSession?.HasActiveSubscription == true)
					{
						Redbox.Rental.Services.Session.Session.Instance.RentalReturnTime = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.RedboxPlusSession.RentalReturnTime;
					}
					CheckCartForOfferCodes();
					CheckCartForTivoQueryId();
					CheckCartForPersonalizationTitleTags();
					OrchestrationService.AnalyticsAddLoggedInCustomerProfileNumber(loginResult.PersonalizationSession?.CustomerProfileNumber);
				}
				IShoppingSession obj = ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetCurrentSession();
				string key = "EarlyIdIndicator";
				if (walletType == WalletType.Apple)
				{
					key = "EarlyIdApple";
				}
				else if (walletType == WalletType.Google)
				{
					key = "EarlyIdGoogle";
				}
				obj?.ShoppingCart?.SetPropertyBagEntry(key, "true");
				OrchestrationService.ResetApplicationIdleTimerTimeout();
			}
			afterLoginAction?.Invoke(loginType, loginResult);
		}, ServiceLocator.Instance.GetService<IViewService>()?.UITaskScheduler);
	}

	public static void ShowRecommendedTitlesIfNeeded(ILoginResult loginResult)
	{
		bool flag = false;
		if (!(ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.IsFull ?? true) && loginResult.PersonalizationSession.RecommendedTitles != null && loginResult.PersonalizationSession.RecommendedTitles.Count > 0 && loginResult.PersonalizationSession.RecommendedTitles[0] != null && loginResult.PersonalizationSession.RecommendedTitles[0].Count > 0)
		{
			IRecommendedTitles recommendedTitles = (ApplicationState.Instance.IsADAAccessible ? RecommendedTitlesPopupViewController.UnrollRecommendedTitles(loginResult.PersonalizationSession.RecommendedTitles[0]) : loginResult.PersonalizationSession.RecommendedTitles[0]);
			int numberOfTitlesToShow = 20;
			List<IBrowseItemModel> list = RecommendedTitlesPopupViewController.CreateBrowseItemsFromRecommendedTitles(recommendedTitles, numberOfTitlesToShow);
			if (list != null && list.Count > 0)
			{
				RecommendedTitlesPopupInfo recommendedTitlesPopupInfo = new RecommendedTitlesPopupInfo();
				recommendedTitlesPopupInfo.RecommendedTitles = recommendedTitles;
				recommendedTitlesPopupInfo.NumberOfTitlesToShow = numberOfTitlesToShow;
				recommendedTitlesPopupInfo.ShownFromView = "perks_confirm_view";
				flag = true;
				OrchestrationService.ViewShow("recommended_titles_popup_view", recommendedTitlesPopupInfo);
			}
		}
		if (!flag && OrchestrationService.PeekView() == "browse_view")
		{
			OrchestrationService.StartIdleTimer("idle_timer");
		}
	}

	private static void CheckCartForOfferCodes()
	{
		IRentalShoppingCart currentRentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().CurrentRentalShoppingCart;
		if (currentRentalShoppingCart == null || currentRentalShoppingCart.TitleItems.Count <= 0)
		{
			return;
		}
		foreach (IRentalShoppingCartTitleItem titleItem in currentRentalShoppingCart.TitleItems)
		{
			titleItem.OfferCode = GetLoyaltyOfferCode(titleItem.TitleProduct);
		}
	}

	private static void CheckCartForTivoQueryId()
	{
		IRentalShoppingCart currentRentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().CurrentRentalShoppingCart;
		if (currentRentalShoppingCart == null || currentRentalShoppingCart.TitleItems.Count <= 0)
		{
			return;
		}
		foreach (IRentalShoppingCartTitleItem titleItem in currentRentalShoppingCart.TitleItems)
		{
			titleItem.TivoQueryId = GetTivoQueryId(titleItem.TitleProduct);
		}
	}

	private static void CheckCartForPersonalizationTitleTags()
	{
		IRentalShoppingCart currentRentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().CurrentRentalShoppingCart;
		if (currentRentalShoppingCart == null || currentRentalShoppingCart.TitleItems.Count <= 0)
		{
			return;
		}
		foreach (IRentalShoppingCartTitleItem titleItem in currentRentalShoppingCart.TitleItems)
		{
			titleItem.PersonalizationTitleTags = GetPersonalizationTitleTags(titleItem.TitleProduct);
		}
	}

	public static void ShowMyPointsError()
	{
		MessagePopupViewController.ShowMyPointsError(delegate
		{
			AnalyticsService?.AddButtonPressEvent("PersonalizationFunctionsNotEnoughPointsGotit");
			OrchestrationService.ViewGoBackSafe("Message button GotIt pressed", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("Message view Timed out.", popOnly: false);
		});
	}

	public static void ShowNotEnoughPointsError()
	{
		MessagePopupViewController.ShowNotEnoughPointsError(delegate
		{
			OrchestrationService.ViewGoBackSafe("Message button Okay pressed", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("Message view Timed out.", popOnly: false);
		});
	}

	public static void ShowPerksPassLoginError(ILoginResult loginResult, Action continueAction = null, bool popOnlyOnViewGoback = false)
	{
		OrchestrationService.SetCurrentUICulture();
		string header = Resources.perks_pass_login_generic_error_header;
		string message = Resources.perks_pass_login_generic_error_message;
		if (loginResult != null)
		{
			switch (loginResult.KioskLoginResponseError)
			{
			case KioskLoginResponseErrors.AccountLocked:
			case KioskLoginResponseErrors.AccountNotFound:
				header = Resources.perks_pass_login_data_mismatch_header;
				message = Resources.perks_pass_login_data_mismatch_message;
				break;
			case KioskLoginResponseErrors.CommunicationError:
			case KioskLoginResponseErrors.LoyaltyIsDown:
				header = Resources.perks_pass_login_server_failure_header;
				message = Resources.perks_pass_login_server_failure_message;
				break;
			case KioskLoginResponseErrors.MobilePassReadError:
				header = Resources.perks_pass_login_read_error_header;
				message = Resources.perks_pass_login_read_error_message;
				break;
			case KioskLoginResponseErrors.MobilePassGoogleNoPassFoundError:
				header = Resources.mobile_pass_login_google_no_pass_found_error_header;
				message = Resources.mobile_pass_login_google_no_pass_found_error_message;
				break;
			default:
				header = Resources.perks_pass_login_generic_error_header;
				message = Resources.perks_pass_login_generic_error_message;
				break;
			}
		}
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service?.UITaskScheduler == null)
		{
			return;
		}
		Task.Factory.StartNew(delegate
		{
			if (loginResult != null && loginResult.KioskLoginResponseError == KioskLoginResponseErrors.MobilePassGoogleNoPassFoundError)
			{
				MessagePopupViewController.ShowMessagePopupViewGoogleNoPassFoundError(delegate
				{
					OrchestrationService.ViewGoBackSafe("Message view Timed out.", popOnlyOnViewGoback);
					continueAction?.Invoke();
				}, delegate
				{
					AnalyticsService?.AddButtonPressEvent("PersonalizationFunctionsPerksPassErrorGotIt");
					OrchestrationService.ViewGoBackSafe("Message button GotIt pressed", popOnlyOnViewGoback);
					continueAction?.Invoke();
				});
			}
			else
			{
				MessagePopupViewController.ShowPerksPassLoginErrorMessage(header, message, delegate
				{
					AnalyticsService?.AddButtonPressEvent("PersonalizationFunctionsPerksPassErrorGotIt");
					OrchestrationService.ViewGoBackSafe("Message button GotIt pressed", popOnlyOnViewGoback);
					continueAction?.Invoke();
				}, delegate
				{
					OrchestrationService.ViewGoBackSafe("Message view Timed out.", popOnlyOnViewGoback);
					continueAction?.Invoke();
				});
			}
		}, CancellationToken.None, TaskCreationOptions.None, service?.UITaskScheduler);
	}

	public static string GetTivoQueryIdFromTransactionEntities(ITitleProduct titleProduct)
	{
		ITransactionData obj = Redbox.Rental.Services.Session.Session.Instance?.TransactionData;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		long productGroupId = service.GetProductGroupId(titleProduct);
		string result = null;
		List<Redbox.Rental.Model.Transaction.ITransactionEntity> list = obj?.TransactionEntities.Where((Redbox.Rental.Model.Transaction.ITransactionEntity x) => x.Name == "TivoQueryId").ToList();
		if (list != null)
		{
			foreach (Redbox.Rental.Model.Transaction.ITransactionEntity item in list)
			{
				service.GetProduct(item.TitleId, excludeTitleRollup: false, out var kioskProduct);
				long productGroupId2 = service.GetProductGroupId(kioskProduct);
				if ((productGroupId > 0 && productGroupId2 > 0 && productGroupId == productGroupId2) || item.TitleId == titleProduct.ProductId)
				{
					result = item.Value;
					break;
				}
			}
		}
		return result;
	}

	public static string GetLoyaltyOfferCode(ITitleProduct titleProduct)
	{
		return GetDataFromRecommendedTitlesForGivenTitleProduct(titleProduct, (IRecommendedTitles x, IRecommendedTitleId y) => x.OfferCode);
	}

	public static string GetTivoQueryId(ITitleProduct titleProduct)
	{
		return GetDataFromRecommendedTitlesForGivenTitleProduct(titleProduct, (IRecommendedTitles x, IRecommendedTitleId y) => x.TivoQueryId);
	}

	public static List<string> GetPersonalizationTitleTags(ITitleProduct titleProduct)
	{
		return GetDataFromRecommendedTitlesForGivenTitleProduct(titleProduct, (IRecommendedTitles x, IRecommendedTitleId y) => y.PersonalizationTitleTags);
	}

	private static T GetDataFromRecommendedTitlesForGivenTitleProduct<T>(ITitleProduct titleProduct, Func<IRecommendedTitles, IRecommendedTitleId, T> dataToGet)
	{
		T result = default(T);
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		if (personalizationSession != null && personalizationSession.RecommendedTitles != null)
		{
			foreach (IRecommendedTitles recommendedTitle in personalizationSession.RecommendedTitles)
			{
				foreach (IRecommendedTitleId item in recommendedTitle)
				{
					if (item.IdType == RecommendedTitlesIdType.ProductGroupID)
					{
						if (item.Id == titleProduct.TitleRollupProductGroupId)
						{
							result = dataToGet(recommendedTitle, item);
							break;
						}
					}
					else if (item.Id == titleProduct.ProductId)
					{
						result = dataToGet(recommendedTitle, item);
						break;
					}
				}
			}
		}
		return result;
	}

	public static void ShowEmailReceiptAddress(string emailAddress)
	{
		LogHelper.Instance.Log("Email Receipt starting.");
		int num = 8000;
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null)
		{
			num = service.GetTimeout("shorter", num);
		}
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		object lockObject = new object();
		bool hasActionExecuted = false;
		ITimer emailNextTimer = null;
		Action continueAction = (continueAction = delegate
		{
			lock (lockObject)
			{
				if (!hasActionExecuted)
				{
					hasActionExecuted = true;
					OrchestrationService.ViewGoBackSafe("email view continue", popOnly: false);
					emailNextTimer.Stop();
					timerService?.RemoveTimer("email_next_timer");
					CheckoutFunctions.PromptUserForSignups();
				}
			}
		});
		emailNextTimer = timerService?.CreateTimer("email_next_timer", num, num, delegate
		{
			continueAction();
		});
		emailNextTimer?.Start();
		OrchestrationService.ViewShow("email_receipt_view", new EmailReceiptInfo
		{
			ContinueAction = continueAction,
			EmailAddress = emailAddress
		});
	}

	public static void ShowSonySweepstakesConfirmationView(IAuthorizeResult authorizeResult, string emailAddress, bool showRedboxSweepstakes)
	{
		int num = 10000;
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null)
		{
			num = service.GetTimeout("shorter", num);
		}
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		object lockObject = new object();
		bool hasActionExecuted = false;
		ITimer sonySweepstakesConfirmationViewNextTimer = null;
		Action continueAction = (continueAction = delegate
		{
			lock (lockObject)
			{
				if (!hasActionExecuted)
				{
					hasActionExecuted = true;
					OrchestrationService.ViewGoBackSafe("sony sweepstakes confirmation view continue", popOnly: false);
					sonySweepstakesConfirmationViewNextTimer.Stop();
					timerService?.RemoveTimer("sony_sweepstakes_confirmation_view_next_timer");
					if (authorizeResult != null && authorizeResult.HasProfile && !string.IsNullOrEmpty(emailAddress))
					{
						CheckoutFunctions.ShowEmailReceiptIfNeeded(authorizeResult);
					}
					else
					{
						CheckoutFunctions.PromptForEmailAddress(authorizeResult, emailAddress, null);
					}
				}
			}
		});
		sonySweepstakesConfirmationViewNextTimer = timerService?.CreateTimer("sony_sweepstakes_confirmation_view_next_timer", num, num, delegate
		{
			continueAction();
		});
		sonySweepstakesConfirmationViewNextTimer?.Start();
		OrchestrationService.ViewShow("sony_sweepstakes_confirmation_view", new SonySweepstakesConfirmationViewInfo
		{
			ContinueAction = continueAction,
			EmailAddress = emailAddress,
			ShowRedboxSweepstakes = showRedboxSweepstakes
		});
	}

	public static void ShowMemberPerks()
	{
		if (!IsLoyaltySignedIn)
		{
			return;
		}
		LogHelper.Instance.Log("Member Perks starting.");
		ILoyaltySession loyaltySession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession;
		if (loyaltySession != null)
		{
			OrchestrationService.ViewShow("member_perks_view", new PerksMemberInfo
			{
				CancelAction = delegate
				{
					OrchestrationService.ViewGoBackSafe("Member Perks cancel", popOnly: false);
				},
				ExpireDate = loyaltySession.PointsExpirationDate,
				ExpirePoints = (uint)loyaltySession.PointsExpiring,
				Points = (uint)loyaltySession.PointsBalance,
				YearRentals = (uint)loyaltySession.LoyaltyTierCounter,
				TierName = loyaltySession.LoyaltyTierName
			});
		}
	}

	public static void ShowUseMemberPoints()
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (!IsLoyaltySignedIn || service?.CurrentRentalShoppingCart?.TitleItems?.Any() != true)
		{
			return;
		}
		LogHelper.Instance.Log("Member Perks starting.");
		ILoyaltySession loyaltySession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession;
		if (loyaltySession == null)
		{
			return;
		}
		PointsMemberInfo pointsMemberInfo = new PointsMemberInfo
		{
			Points = (uint)loyaltySession.PointsBalance,
			CancelAction = delegate
			{
				OrchestrationService.ViewGoBackSafe("Member Points canceled", popOnly: false);
			},
			ContinueAction = delegate
			{
				OrchestrationService.ViewGoBackSafe("Member Points redeemed", popOnly: false);
			}
		};
		MemberPointsViewController.GetRedemptionPoints(delegate(IEstimateRedemptionPointsResult estimatePointsAccruedResult)
		{
			if (estimatePointsAccruedResult != null)
			{
				bool success = estimatePointsAccruedResult.Success;
				OrchestrationService.ViewGoBackSafe("Removed PleaseWaitView", success);
				if (success)
				{
					pointsMemberInfo.EstimateRedemptionPoints = estimatePointsAccruedResult;
					OrchestrationService.ViewShow("member_points_view", pointsMemberInfo);
				}
				else
				{
					ShowLoyaltyIsDownDialog();
				}
			}
			else
			{
				OrchestrationService.ViewGoBackSafe("MemberPoints problem: fail to get EstimateRedemptionPoints", popOnly: true);
			}
		});
		OrchestrationService.ViewShow("please_wait_view", null);
	}
}
