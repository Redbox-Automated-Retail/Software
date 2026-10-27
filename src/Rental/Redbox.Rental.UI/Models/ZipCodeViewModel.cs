using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.UI.Models;

public class ZipCodeViewModel : DependencyObject
{
	public Action<string> OnNextButtonClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty KeypadModelProperty = DependencyProperty.Register("KeypadModel", typeof(KeypadModel), typeof(ZipCodeViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdImageSourceProperty = DependencyProperty.Register("AdImageSource", typeof(BitmapImage), typeof(ZipCodeViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdImageVisibilityProperty = DependencyProperty.Register("AdImageVisibility", typeof(Visibility), typeof(ZipCodeViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty StackPanelMarginProperty = DependencyProperty.Register("StackPanelMargin", typeof(Thickness), typeof(ZipCodeViewModel), new FrameworkPropertyMetadata(new Thickness(0.0, 49.0, 0.0, 0.0)));

	public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText", typeof(string), typeof(ZipCodeViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty IsNextButtonEnabledProperty = DependencyProperty.Register("IsNextButtonEnabled", typeof(bool), typeof(ZipCodeViewModel), new FrameworkPropertyMetadata(false)
	{
		AffectsRender = true
	});

	public int RequiredLength { get; set; }

	public string HeaderLabelText { get; set; }

	public string Header2LabelText { get; set; }

	public string Note1LabelText { get; set; }

	public string Note2LabelText { get; set; }

	public string NextButtonText { get; set; }

	public KeypadModel KeypadModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (KeypadModel)GetValue(KeypadModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KeypadModelProperty, value);
			});
		}
	}

	public BitmapImage AdImageSource
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BitmapImage)GetValue(AdImageSourceProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdImageSourceProperty, value);
			});
		}
	}

	public IAdImpression AdImpression { get; set; }

	public Visibility AdImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AdImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdImageVisibilityProperty, value);
			});
		}
	}

	public Thickness StackPanelMargin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(StackPanelMarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StackPanelMarginProperty, value);
			});
		}
	}

	public string DisplayText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DisplayTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayTextProperty, value);
			});
		}
	}

	public string TTSDisplayText
	{
		get
		{
			string text = "";
			string displayText = DisplayText;
			for (int i = 0; i < displayText.Length; i++)
			{
				text = text + displayText[i] + " ";
			}
			return text;
		}
	}

	public bool IsNextButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsNextButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsNextButtonEnabledProperty, value);
			});
		}
	}

	public void ProcessOnNextButtonClicked(string displayText)
	{
		if (OnNextButtonClicked != null)
		{
			OnNextButtonClicked(displayText);
		}
	}

	public ISpeechControl ProcessGetSpeechControl()
	{
		ISpeechControl result = null;
		if (OnGetSpeechControl != null)
		{
			result = OnGetSpeechControl();
		}
		return result;
	}
}
