using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class SubscriptionSignUpConfirmationViewModel : DependencyObject
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public ISubscriptionProduct SubscriptionProduct { get; set; }

	public string RegisteredTrademarkSymbol { get; set; }

	public string RegisteredTrademarkSymbolMessage { get; set; }

	public string TitleText1 { get; set; }

	public string TitleText2 { get; set; }

	public string MessageText1 { get; set; }

	public string MessageText2 { get; set; }

	public string SubMessageText { get; set; }

	public string SubMessageText2 { get; set; }

	public string ContinueButtonText { get; set; }

	public Action ContinueAction { get; set; }

	public bool HasActionExecuted { get; set; }

	public Visibility RedboxPlusExistingCustomerTextVisibility { get; set; }

	public Visibility RedboxPlusNewCustomerTextVisibility { get; set; }

	public DynamicRoutedCommand GotItButtonCommand { get; set; }

	public ISpeechControl ProcessGetSpeechControl()
	{
		return OnGetSpeechControl?.Invoke();
	}
}
