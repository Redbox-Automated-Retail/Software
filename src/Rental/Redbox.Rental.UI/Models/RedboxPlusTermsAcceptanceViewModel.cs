using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class RedboxPlusTermsAcceptanceViewModel : DependencyObject
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty AcceptButtonIsEnabledProperty = DependencyProperty.Register("AcceptButtonIsEnabled", typeof(bool), typeof(RedboxPlusTermsAcceptanceViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty CheckMarkVisibilityProperty = DependencyProperty.Register("CheckMarkVisibility", typeof(Visibility), typeof(RedboxPlusTermsAcceptanceViewModel), new FrameworkPropertyMetadata(Visibility.Hidden));

	public static readonly DependencyProperty AcceptButtonTextProperty = DependencyProperty.Register("AcceptButtonText", typeof(string), typeof(RedboxPlusTermsAcceptanceViewModel), new FrameworkPropertyMetadata(null));

	public DynamicRoutedCommand CheckMarkCommand { get; set; }

	public DynamicRoutedCommand AcceptButtonCommand { get; set; }

	public DynamicRoutedCommand BackButtonCommand { get; set; }

	public DynamicRoutedCommand TermsAndPrivacyButtonCommand { get; set; }

	public string TitleText { get; set; }

	public string SubtitleText { get; set; }

	public string EmailText { get; set; }

	public string TermsHeader { get; set; }

	public string TermsText { get; set; }

	public IRedboxPlusSubscriptionProduct RedboxPlusSubscriptionProduct { get; set; }

	public string AcceptButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AcceptButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AcceptButtonTextProperty, value);
			});
		}
	}

	public bool AcceptButtonIsEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(AcceptButtonIsEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AcceptButtonIsEnabledProperty, value);
			});
		}
	}

	public Visibility CheckMarkVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(CheckMarkVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckMarkVisibilityProperty, value);
			});
		}
	}

	public ISpeechControl ProcessGetSpeechControl()
	{
		return OnGetSpeechControl?.Invoke();
	}
}
