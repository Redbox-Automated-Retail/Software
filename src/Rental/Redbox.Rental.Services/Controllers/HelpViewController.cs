using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Xps.Packaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class HelpViewController : BaseViewController
{
	private const string LoyaltyTimeoutTimerName = "loyalty_help_view_timer";

	public static void ShowFullHelpView(bool allowFMA, bool restartTimer, string analyticsContext = null)
	{
		HelpViewControllerParameters helpViewControllerParameters = new HelpViewControllerParameters
		{
			AnalysisContext = analyticsContext
		};
		helpViewControllerParameters.GoBackAction = delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("HelpBack");
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			string obj = service?.Peek(1);
			if (restartTimer)
			{
				OrchestrationService.ResetApplicationIdleTimerTimeout();
			}
			if (obj == service?.GetStartOverViewName() && !OrchestrationService.IsPersonalizationSessionSignedIn())
			{
				ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession(null);
			}
			else
			{
				OrchestrationService.ViewGoBackSafe("go_back", popOnly: false);
			}
		};
		if (allowFMA)
		{
			helpViewControllerParameters.ClickRedboxLogoAction = delegate
			{
				OrchestrationService.ExecuteScript("activate_fa");
			};
		}
		if (restartTimer)
		{
			OrchestrationService.ResetApplicationIdleTimerTimeout();
			OrchestrationService.StartIdleTimer("idle_timer");
		}
		OrchestrationService.ViewShow("help_document_view", helpViewControllerParameters);
	}

	public static void ShowTermsAndConditions(string analysisContext = null)
	{
		HelpViewControllerParameters helpViewControllerParameters = new HelpViewControllerParameters
		{
			AnalysisContext = analysisContext
		};
		helpViewControllerParameters.GoBackAction = delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("HelpBack");
			OrchestrationService.ResetApplicationIdleTimerTimeout();
			OrchestrationService.ViewGoBackSafe("go_back", popOnly: false);
		};
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = HelpDocuments.Terms;
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = 1;
		}
		OrchestrationService.ResetApplicationIdleTimerTimeout();
		OrchestrationService.StartIdleTimer("idle_timer");
		OrchestrationService.ViewShow("help_document_view", helpViewControllerParameters);
	}

	public static void ShowTermsAndConditions(string analysisContext, string idleTimerName)
	{
		ShowHelpDocument(analysisContext, HelpDocuments.Terms, idleTimerName);
	}

	public static void ShowSubscriptions(string analysisContext = null)
	{
		HelpViewControllerParameters helpViewControllerParameters = new HelpViewControllerParameters
		{
			AnalysisContext = analysisContext
		};
		helpViewControllerParameters.GoBackAction = delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("HelpBack");
			OrchestrationService.ResetApplicationIdleTimerTimeout();
			OrchestrationService.ViewGoBackSafe("go_back", popOnly: false);
		};
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = HelpDocuments.Subscriptions;
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = 1;
		}
		OrchestrationService.ResetApplicationIdleTimerTimeout();
		OrchestrationService.StartIdleTimer("idle_timer");
		OrchestrationService.ViewShow("help_document_view", helpViewControllerParameters);
	}

	public static void ShowRedboxPlus(string analysisContext = null)
	{
		HelpViewControllerParameters helpViewControllerParameters = new HelpViewControllerParameters
		{
			AnalysisContext = analysisContext
		};
		helpViewControllerParameters.GoBackAction = delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("HelpBack");
			OrchestrationService.ResetApplicationIdleTimerTimeout();
			OrchestrationService.ViewGoBackSafe("go_back", popOnly: false);
		};
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = HelpDocuments.RedboxPlus;
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = 1;
		}
		OrchestrationService.ResetApplicationIdleTimerTimeout();
		OrchestrationService.StartIdleTimer("idle_timer");
		OrchestrationService.ViewShow("help_document_view", helpViewControllerParameters);
	}

	private static void ShowHelpDocument(string analysisContext, HelpDocuments helpDocuments, string idleTimerName = "idle_timer")
	{
		HelpViewControllerParameters helpViewControllerParameters = new HelpViewControllerParameters
		{
			AnalysisContext = analysisContext
		};
		helpViewControllerParameters.GoBackAction = delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("HelpBack");
			OrchestrationService.StopIdleTimer("loyalty_help_view_timer");
			OrchestrationService.RemoveIdleTimer("loyalty_help_view_timer");
			OrchestrationService.ResumeIdleTimer(idleTimerName);
			OrchestrationService.ViewGoBackSafe("go_back", popOnly: false);
		};
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = helpDocuments;
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = 1;
		}
		OrchestrationService.SuspendIdleTimer(idleTimerName);
		OrchestrationService.CreateIdleTimer("loyalty_help_view_timer");
		OrchestrationService.StartIdleTimer(40000, helpViewControllerParameters.GoBackAction, "loyalty_help_view_timer");
		OrchestrationService.ViewShow("help_document_view", helpViewControllerParameters);
	}

	public static void ShowMobileFaqAndPrivacy(string analysisContext, string idleTimerName = "idle_timer")
	{
		HelpViewControllerParameters helpViewControllerParameters = new HelpViewControllerParameters
		{
			AnalysisContext = analysisContext
		};
		helpViewControllerParameters.GoBackAction = delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("HelpBack");
			OrchestrationService.StopIdleTimer("loyalty_help_view_timer");
			OrchestrationService.RemoveIdleTimer("loyalty_help_view_timer");
			OrchestrationService.ResumeIdleTimer(idleTimerName);
			OrchestrationService.ViewGoBackSafe("go_back", popOnly: false);
		};
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			if (ServiceLocator.Instance.GetService<IResourceBundleService>().GetResource("document-mobile-faqs") != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = HelpDocuments.MobileFaq;
			}
			else
			{
				Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = HelpDocuments.Privacy;
			}
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = 1;
		}
		OrchestrationService.SuspendIdleTimer(idleTimerName);
		OrchestrationService.CreateIdleTimer("loyalty_help_view_timer");
		OrchestrationService.StartIdleTimer(40000, helpViewControllerParameters.GoBackAction, "loyalty_help_view_timer");
		OrchestrationService.ViewShow("help_document_view", helpViewControllerParameters);
	}

	public static void ShowJoinPerks(Action goBackAction, string idleTimerName = "idle_timer")
	{
		HelpViewControllerParameters helpViewControllerParameters = new HelpViewControllerParameters
		{
			AnalysisContext = "perks_terms",
			GoBackAction = delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("HelpBack", "perks_terms");
				OrchestrationService.StopIdleTimer("loyalty_help_view_timer");
				OrchestrationService.RemoveIdleTimer("loyalty_help_view_timer");
				OrchestrationService.ResumeIdleTimer(idleTimerName);
				goBackAction?.Invoke();
			}
		};
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = HelpDocuments.PlayPass;
			Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = 1;
		}
		OrchestrationService.CreateIdleTimer("loyalty_help_view_timer");
		OrchestrationService.StartIdleTimer(40000, helpViewControllerParameters.GoBackAction, "loyalty_help_view_timer");
		OrchestrationService.ViewShow("help_document_view", helpViewControllerParameters);
	}

	public static void ConfigureHelpView(HelpViewControllerParameters parameters)
	{
		OrchestrationService.SetCurrentUICulture();
		HelpViewUserControl helpViewUserControl = (HelpViewUserControl)BaseViewController.GetViewUserControl();
		if (helpViewUserControl == null)
		{
			return;
		}
		string.IsNullOrWhiteSpace(parameters.AnalysisContext);
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		HelpViewModel viewModel = (HelpViewModel)helpViewUserControl.DataContext;
		if (viewModel == null)
		{
			viewModel = new HelpViewModel();
		}
		viewModel.OnHelpDocumentChanged += delegate(HelpDocuments helpDocument)
		{
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = helpDocument;
			}
		};
		viewModel.OnHelpDocumentPageChanged += delegate(int pageNumber)
		{
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = pageNumber;
			}
		};
		viewModel.OnHelpDocumentButtonPress += delegate(HelpDocuments helpDocument)
		{
			HelpViewModel.HelpDocumentInfo value = null;
			viewModel.HelpDocumentInfoList.TryGetValue(helpDocument, out value);
			if (value != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocument = helpDocument;
				Redbox.Rental.Services.Session.Session.Instance.HelpState.CurrentHelpDocumentPage = 1;
				viewModel.CurrentHelpDocument = helpDocument;
				viewModel.CurrentHelpDocumentPage = 1;
				viewModel.XpsDocument = LoadXpsDocument(value);
				SetTitle(viewModel);
				viewModel.ShowContactText = viewModel.CurrentHelpDocument == HelpDocuments.Faq || viewModel.CurrentHelpDocument == HelpDocuments.MobileFaq;
				BaseViewController.AnalyticsService?.AddButtonPressEvent("HelpSelectDocument", value.AnalyticsButtonName);
			}
		};
		viewModel.OnGoBack += parameters.GoBackAction;
		viewModel.OnRedboxLogoCommand += parameters.ClickRedboxLogoAction;
		viewModel.OnHelpPaddleButtonPressed += delegate(string source)
		{
			string arg = ((source == "Left") ? "help-page-back," : "help-page-next,");
			arg = $"{arg}{viewModel.TitleText}";
			BaseViewController.AnalyticsService?.AddButtonPressEvent("HelpPageChange", arg);
		};
		if (service != null)
		{
			viewModel.StoreNumberText = service["StoreNumber"];
		}
		viewModel.BackButtonText = Resources.back_button.ToUpper();
		viewModel.ContactText = Resources.contact_redbox_label;
		viewModel.PageCounterText = Resources.help_page_counter;
		viewModel.StoreNumberLabel = Resources.kiosk_id_label;
		viewModel.CloseButtonText = Resources.close_button;
		ServiceLocator.Instance.GetService<IConfigurationService>();
		BuildHelpDocInfoList(giftCardEnabled: false, viewModel);
		HelpDocuments valueOrDefault = (Redbox.Rental.Services.Session.Session.Instance?.HelpState?.CurrentHelpDocument).GetValueOrDefault();
		BuildHelpDocButtons(parameters, viewModel, valueOrDefault);
		viewModel.CurrentHelpDocument = valueOrDefault;
		viewModel.XpsDocument = LoadXpsDocument(viewModel.HelpDocumentInfoList[valueOrDefault]);
		SetTitle(viewModel);
		viewModel.ShowContactText = valueOrDefault == HelpDocuments.Faq || viewModel.CurrentHelpDocument == HelpDocuments.MobileFaq;
		ConfigureTTS(viewModel);
		helpViewUserControl.DataContext = viewModel;
		viewModel.CurrentHelpDocumentPage = Redbox.Rental.Services.Session.Session.Instance?.HelpState?.CurrentHelpDocumentPage ?? 1;
	}

	private static void BuildHelpDocButtons(HelpViewControllerParameters parameters, HelpViewModel viewModel, HelpDocuments currentHelpDoc)
	{
		viewModel.ButtonList = new ObservableCollection<HelpButtonData>();
		viewModel.ButtonList.Add(new HelpButtonData
		{
			ButtonText = Resources.play_pass_button,
			DocumentToShow = HelpDocuments.PlayPass,
			IsEnabled = (currentHelpDoc != HelpDocuments.PlayPass)
		});
		viewModel.ButtonList.Add(new HelpButtonData
		{
			ButtonText = Resources.faq_button,
			DocumentToShow = HelpDocuments.Faq,
			IsEnabled = (currentHelpDoc != HelpDocuments.Faq)
		});
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service.GetResource(Redbox.Rental.Services.Configuration.Configuration.Instance.MobileFaqDocument) != null)
		{
			viewModel.ButtonList.Add(new HelpButtonData
			{
				ButtonText = Resources.mobile_faq_button,
				DocumentToShow = HelpDocuments.MobileFaq,
				IsEnabled = (currentHelpDoc != HelpDocuments.MobileFaq)
			});
		}
		viewModel.ButtonList.Add(new HelpButtonData
		{
			ButtonText = Resources.transaction_terms_button,
			DocumentToShow = HelpDocuments.Terms,
			IsEnabled = (currentHelpDoc != HelpDocuments.Terms)
		});
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.GiftCardsEnabled)
		{
			viewModel.ButtonList.Add(new HelpButtonData
			{
				ButtonText = Resources.terms_redbox_card_button,
				DocumentToShow = HelpDocuments.TermsReboxCard,
				IsEnabled = (currentHelpDoc != HelpDocuments.TermsReboxCard)
			});
		}
		if (service.GetResource(Redbox.Rental.Services.Configuration.Configuration.Instance.SubscriptionsDocument) != null)
		{
			viewModel.ButtonList.Add(new HelpButtonData
			{
				ButtonText = Resources.subscriptions_button,
				DocumentToShow = HelpDocuments.Subscriptions,
				IsEnabled = (currentHelpDoc != HelpDocuments.Subscriptions)
			});
		}
		if (service.GetResource(Redbox.Rental.Services.Configuration.Configuration.Instance.RedboxPlusDocument) != null)
		{
			viewModel.ButtonList.Add(new HelpButtonData
			{
				ButtonText = Resources.redbox_plus_button,
				DocumentToShow = HelpDocuments.RedboxPlus,
				IsEnabled = (currentHelpDoc != HelpDocuments.RedboxPlus)
			});
		}
		viewModel.ButtonList.Add(new HelpButtonData
		{
			ButtonText = Resources.privacy_button,
			DocumentToShow = HelpDocuments.Privacy,
			IsEnabled = (currentHelpDoc != HelpDocuments.Privacy)
		});
		if (service.GetResource(Redbox.Rental.Services.Configuration.Configuration.Instance.CAPrivacyNoticeDocument) != null)
		{
			viewModel.ButtonList.Add(new HelpButtonData
			{
				ButtonText = Resources.ca_privacy_notice_button,
				DocumentToShow = HelpDocuments.CAPrivacyNotice,
				IsEnabled = (currentHelpDoc != HelpDocuments.CAPrivacyNotice)
			});
		}
		if (service.GetResource(Redbox.Rental.Services.Configuration.Configuration.Instance.DoNotSellDocument) != null)
		{
			viewModel.ButtonList.Add(new HelpButtonData
			{
				ButtonText = Resources.do_not_sell_button,
				DocumentToShow = HelpDocuments.DoNotSell,
				IsEnabled = (currentHelpDoc != HelpDocuments.DoNotSell)
			});
		}
	}

	private static void BuildHelpDocInfoList(bool giftCardEnabled, HelpViewModel viewModel)
	{
		string perksDocument = Redbox.Rental.Services.Configuration.Configuration.Instance.PerksDocument;
		string fAQDocument = Redbox.Rental.Services.Configuration.Configuration.Instance.FAQDocument;
		string mobileFaqDocument = Redbox.Rental.Services.Configuration.Configuration.Instance.MobileFaqDocument;
		string termsDocument = Redbox.Rental.Services.Configuration.Configuration.Instance.TermsDocument;
		string redboxCardDocument = Redbox.Rental.Services.Configuration.Configuration.Instance.RedboxCardDocument;
		string subscriptionsDocument = Redbox.Rental.Services.Configuration.Configuration.Instance.SubscriptionsDocument;
		string redboxPlusDocument = Redbox.Rental.Services.Configuration.Configuration.Instance.RedboxPlusDocument;
		string privacyDocument = Redbox.Rental.Services.Configuration.Configuration.Instance.PrivacyDocument;
		string cAPrivacyNoticeDocument = Redbox.Rental.Services.Configuration.Configuration.Instance.CAPrivacyNoticeDocument;
		string doNotSellDocument = Redbox.Rental.Services.Configuration.Configuration.Instance.DoNotSellDocument;
		viewModel.HelpDocumentInfoList = new Dictionary<HelpDocuments, HelpViewModel.HelpDocumentInfo>();
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.PlayPass, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = perksDocument,
			DefaultDocumentName = "document-perks",
			Title = Resources.play_pass_title,
			Visible = true,
			AnalyticsButtonName = "show_play_pass",
			AbeTitle = Resources.play_pass_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.Faq, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = fAQDocument,
			DefaultDocumentName = "document-faqs",
			Title = Resources.faq_title,
			Visible = true,
			AnalyticsButtonName = "show_faq",
			AbeTitle = Resources.faq_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.MobileFaq, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = mobileFaqDocument,
			DefaultDocumentName = "document-mobile-faqs",
			Title = Resources.mobile_faq_title,
			Visible = true,
			AnalyticsButtonName = "show_mobile_faq",
			AbeTitle = Resources.mobile_faq_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.Terms, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = termsDocument,
			DefaultDocumentName = "document-terms",
			Title = Resources.terms_of_use_title,
			Visible = true,
			AnalyticsButtonName = "show_terms",
			AbeTitle = Resources.terms_of_use_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.TermsReboxCard, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = redboxCardDocument,
			DefaultDocumentName = "document-terms-redbox-card",
			Title = Resources.terms_of_use_title,
			Visible = giftCardEnabled,
			AnalyticsButtonName = "show_terms_redbox_card",
			AbeTitle = Resources.redbox_card_terms_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.Subscriptions, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = subscriptionsDocument,
			DefaultDocumentName = "document-subscriptions",
			Title = Resources.subscriptions_title,
			Visible = true,
			AnalyticsButtonName = "show_subscriptions",
			AbeTitle = Resources.subscriptions_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.RedboxPlus, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = redboxPlusDocument,
			DefaultDocumentName = "document-redbox-plus",
			Title = Resources.redbox_plus_title,
			Visible = true,
			AnalyticsButtonName = "show_redbox_plus",
			AbeTitle = Resources.redbox_plus_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.Privacy, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = privacyDocument,
			DefaultDocumentName = "document-privacy-policy",
			Title = Resources.privacy_policy_title,
			Visible = true,
			AnalyticsButtonName = "show_privacy",
			AbeTitle = Resources.privacy_policy_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.CAPrivacyNotice, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = cAPrivacyNoticeDocument,
			DefaultDocumentName = "document-ca-privacy-notice",
			Title = Resources.ca_privacy_notice_title,
			Visible = true,
			AnalyticsButtonName = "show_ca_privacy_notice",
			AbeTitle = Resources.ca_privacy_notice_title_abe
		});
		viewModel.HelpDocumentInfoList.Add(HelpDocuments.DoNotSell, new HelpViewModel.HelpDocumentInfo
		{
			DocumentName = doNotSellDocument,
			DefaultDocumentName = "document-do-not-sell",
			Title = Resources.do_not_sell_title,
			Visible = true,
			AnalyticsButtonName = "show_do_not_sell",
			AbeTitle = Resources.do_not_sell_title_abe
		});
	}

	private static void SetTitle(HelpViewModel viewModel)
	{
		HelpViewModel.HelpDocumentInfo value = null;
		viewModel.HelpDocumentInfoList.TryGetValue(viewModel.CurrentHelpDocument, out value);
		if (value != null)
		{
			viewModel.TitleText = value.Title;
		}
	}

	private static XpsDocument LoadXpsDocument(HelpViewModel.HelpDocumentInfo helpDocumentInfo)
	{
		XpsDocument xpsDocument = null;
		if (helpDocumentInfo != null)
		{
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			if (service != null)
			{
				xpsDocument = service.GetXpsDocument(helpDocumentInfo.DocumentName);
				if (xpsDocument == null)
				{
					LogHelper.Instance.Log("Unable to get Xps document {0}", helpDocumentInfo.DocumentName);
					if (!helpDocumentInfo.DocumentName.Equals(helpDocumentInfo.DefaultDocumentName, StringComparison.CurrentCultureIgnoreCase))
					{
						xpsDocument = service.GetXpsDocument(helpDocumentInfo.DefaultDocumentName);
						if (xpsDocument == null)
						{
							LogHelper.Instance.Log("Unable to get default Xps document {0}", helpDocumentInfo.DefaultDocumentName);
						}
					}
				}
			}
		}
		else
		{
			LogHelper.Instance.Log("Help document info missing in LoadXpsDocument");
		}
		return xpsDocument;
	}

	private static HelpViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as HelpViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(HelpViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			viewModel.OnGetSpeechControl = () => HelpViewTextToSpeech.GetSpeechControl(GetViewModel);
		}
	}

	internal static void LeaveHelpView()
	{
	}
}
