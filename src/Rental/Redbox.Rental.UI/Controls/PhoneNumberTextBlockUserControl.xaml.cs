using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Controls;

public partial class PhoneNumberTextBlockUserControl : TextBlock, IComponentConnector
{
	public static readonly DependencyProperty FormatMaskProperty = DependencyProperty.Register("FormatMask", typeof(string), typeof(PhoneNumberTextBlockUserControl), new FrameworkPropertyMetadata(null, OnFormatMaskChanged)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DigitsProperty = DependencyProperty.Register("Digits", typeof(string), typeof(PhoneNumberTextBlockUserControl), new FrameworkPropertyMetadata(null, OnDigitsChanged)
	{
		AffectsRender = true
	});

	private const char DigitPlaceholder = '#';

	public string FormatMask
	{
		get
		{
			return (string)GetValue(FormatMaskProperty);
		}
		set
		{
			SetValue(FormatMaskProperty, value);
		}
	}

	public string Digits
	{
		get
		{
			return (string)GetValue(DigitsProperty);
		}
		set
		{
			SetValue(DigitsProperty, value);
		}
	}

	public new string Text
	{
		get
		{
			return (string)GetValue(TextBlock.TextProperty);
		}
		private set
		{
			SetValue(TextBlock.TextProperty, value);
		}
	}

	public PhoneNumberTextBlockUserControl()
	{
		InitializeComponent();
	}

	private static void OnFormatMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as PhoneNumberTextBlockUserControl).FormatText();
	}

	private static void OnDigitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as PhoneNumberTextBlockUserControl).FormatText();
	}

	private void FormatText()
	{
		string text = string.Empty;
		FormatMask = FormatMask ?? string.Empty;
		Digits = Digits ?? string.Empty;
		int num = 0;
		for (int i = 0; i < FormatMask.Length; i++)
		{
			if (FormatMask[i] == '#')
			{
				if (num < Digits.Length)
				{
					text += Digits[num];
					num++;
				}
				else
				{
					text += " ";
				}
			}
			else
			{
				text += FormatMask[i];
			}
		}
		while (num < Digits.Length)
		{
			text += Digits[num++];
		}
		Text = text;
	}
}
