using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Models;

public class SimpleModel : BaseModel<SimpleModel>
{
	public static readonly DependencyProperty BannerImageProperty = BaseModel<SimpleModel>.CreateDependencyProperty("BannerImage", TYPES.BITMAPIMAGE);

	public static readonly DependencyProperty BannerImageVisibilityProperty = BaseModel<SimpleModel>.CreateDependencyProperty("BannerImageVisibility", TYPES.VISIBILITY, Visibility.Visible);

	public static readonly DependencyProperty DisplayImageProperty = BaseModel<SimpleModel>.CreateDependencyProperty("DisplayImage", TYPES.BITMAPIMAGE);

	public static readonly DependencyProperty TitleTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("TitleText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty MessageTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("MessageText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty CommentTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("CommentText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty FooterTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("FooterText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty CancelButtonTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("CancelButtonText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty ContinueButtonTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("ContinueButtonText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty Circle1TextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("Circle1Text", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty Circle2TextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("Circle2Text", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty Circle3TextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("Circle3Text", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty OtherButtonTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("OtherButtonText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty CheckboxTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("CheckboxText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty CheckboxCheckedProperty = DependencyProperty.Register("CheckboxChecked", TYPES.BOOL, typeof(SimpleModel), new FrameworkPropertyMetadata(false, CheckboxPropertyChanged));

	public Action OtherAction { get; set; }

	public Action CancelAction { get; set; }

	public Action ContinueAction { get; set; }

	public Action<bool> CheckBoxChangedAction { get; set; }

	public SimpleInfo Info { get; set; }

	public DynamicRoutedCommand OtherButtonCommand { get; set; }

	public DynamicRoutedCommand CancelButtonCommand { get; set; }

	public DynamicRoutedCommand ContinueButtonCommand { get; set; }

	public BitmapImage BannerImage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BitmapImage)GetValue(BannerImageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BannerImageProperty, value);
			});
		}
	}

	public Visibility BannerImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BannerImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BannerImageVisibilityProperty, value);
			});
		}
	}

	public BitmapImage DisplayImage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BitmapImage)GetValue(DisplayImageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayImageProperty, value);
			});
		}
	}

	public string TitleText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TitleTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleTextProperty, value);
			});
		}
	}

	public string MessageText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MessageTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MessageTextProperty, value);
			});
		}
	}

	public string CommentText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CommentTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CommentTextProperty, value);
			});
		}
	}

	public string FooterText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(FooterTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FooterTextProperty, value);
			});
		}
	}

	public string CancelButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CancelButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CancelButtonTextProperty, value);
			});
		}
	}

	public string ContinueButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ContinueButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ContinueButtonTextProperty, value);
			});
		}
	}

	public string Circle1Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(Circle1TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(Circle1TextProperty, value);
			});
		}
	}

	public string Circle2Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(Circle2TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(Circle2TextProperty, value);
			});
		}
	}

	public string Circle3Text
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(Circle3TextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(Circle3TextProperty, value);
			});
		}
	}

	public string OtherButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(OtherButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OtherButtonTextProperty, value);
			});
		}
	}

	public string CheckboxText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CheckboxTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckboxTextProperty, value);
			});
		}
	}

	public bool CheckboxChecked
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(CheckboxCheckedProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CheckboxCheckedProperty, value);
			});
		}
	}

	public IAdImpression AdImpression { get; set; }

	private static void CheckboxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SimpleModel simpleModel)
		{
			simpleModel.CheckBoxChangedAction(simpleModel.CheckboxChecked);
		}
	}
}
