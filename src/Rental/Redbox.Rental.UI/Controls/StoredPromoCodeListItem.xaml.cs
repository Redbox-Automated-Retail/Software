using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace Redbox.Rental.UI.Controls;

public partial class StoredPromoCodeListItem : BaseUserControl, IComponentConnector
{
	private static DependencyProperty PromoCodeTextProperty = DependencyProperty.Register("PromoCodeText", typeof(string), typeof(StoredPromoCodeListItem), new PropertyMetadata(null));

	private static DependencyProperty IsAddedTextProperty = DependencyProperty.Register("IsAddedText", typeof(string), typeof(StoredPromoCodeListItem), new PropertyMetadata(null));

	private static DependencyProperty PromoDescriptionTextProperty = DependencyProperty.Register("PromoDescriptionText", typeof(string), typeof(StoredPromoCodeListItem), new PropertyMetadata(null));

	private static DependencyProperty PromoExpirationTextProperty = DependencyProperty.Register("PromoExpirationText", typeof(string), typeof(StoredPromoCodeListItem), new PropertyMetadata(null));

	private static DependencyProperty AdaButtonNumberProperty = DependencyProperty.Register("AdaButtonNumber", typeof(int), typeof(StoredPromoCodeListItem), new PropertyMetadata(0));

	private static DependencyProperty IsAdaModeProperty = DependencyProperty.Register("IsAdaMode", typeof(bool), typeof(StoredPromoCodeListItem), new PropertyMetadata(false, IsAdaChanged));

	private static DependencyProperty AddRemoveButtonCommandProperty = DependencyProperty.Register("AddRemoveButtonCommand", typeof(DynamicRoutedCommand), typeof(StoredPromoCodeListItem), new PropertyMetadata(null));

	private static DependencyPropertyKey IsAddedTextVisibilityPropertyKey = DependencyProperty.RegisterReadOnly("IsAddedTextVisibility", typeof(Visibility), typeof(StoredPromoCodeListItem), new FrameworkPropertyMetadata(Visibility.Collapsed, FrameworkPropertyMetadataOptions.AffectsParentArrange));

	public static readonly DependencyProperty IsAddedTextVisibilityProperty = IsAddedTextVisibilityPropertyKey.DependencyProperty;

	private static DependencyProperty IsAddedProperty = DependencyProperty.Register("IsAdded", typeof(bool), typeof(StoredPromoCodeListItem), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange, IsAddedChanged));

	private static DependencyPropertyKey MainButtonStylePropertyKey = DependencyProperty.RegisterReadOnly("MainButtonStyle", typeof(Style), typeof(StoredPromoCodeListItem), new PropertyMetadata(null));

	public static readonly DependencyProperty MainButtonStyleProperty = MainButtonStylePropertyKey.DependencyProperty;

	private static DependencyPropertyKey AddRemoveButtonStylePropertyKey = DependencyProperty.RegisterReadOnly("AddRemoveButtonStyle", typeof(Style), typeof(StoredPromoCodeListItem), new PropertyMetadata(null));

	public static readonly DependencyProperty AddRemoveButtonStyleProperty = AddRemoveButtonStylePropertyKey.DependencyProperty;

	private static DependencyPropertyKey AddRemoveButtonXVisibilityPropertyKey = DependencyProperty.RegisterReadOnly("AddRemoveButtonXVisibility", typeof(Visibility), typeof(StoredPromoCodeListItem), new PropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty AddRemoveButtonXVisibilityProperty = AddRemoveButtonXVisibilityPropertyKey.DependencyProperty;

	private static DependencyPropertyKey AddRemoveButtonPlusVisibilityPropertyKey = DependencyProperty.RegisterReadOnly("AddRemoveButtonPlusVisibility", typeof(Visibility), typeof(StoredPromoCodeListItem), new PropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty AddRemoveButtonPlusVisibilityProperty = AddRemoveButtonPlusVisibilityPropertyKey.DependencyProperty;

	private static DependencyPropertyKey PromoTextBrushPropertyKey = DependencyProperty.RegisterReadOnly("PromoTextBrush", typeof(Brush), typeof(StoredPromoCodeListItem), new PropertyMetadata(Brushes.Black));

	public static readonly DependencyProperty PromoTextBrushProperty = PromoTextBrushPropertyKey.DependencyProperty;

	private static DependencyPropertyKey ExpirationTextBrushPropertyKey = DependencyProperty.RegisterReadOnly("ExpirationTextBrush", typeof(Brush), typeof(StoredPromoCodeListItem), new PropertyMetadata(Brushes.Red));

	public static readonly DependencyProperty ExpirationTextBrushProperty = ExpirationTextBrushPropertyKey.DependencyProperty;

	public string PromoCodeText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PromoCodeTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromoCodeTextProperty, value);
			});
		}
	}

	public string IsAddedText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(IsAddedTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAddedTextProperty, value);
			});
		}
	}

	public string PromoDescriptionText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PromoDescriptionTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromoDescriptionTextProperty, value);
			});
		}
	}

	public string PromoExpirationText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PromoExpirationTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromoExpirationTextProperty, value);
			});
		}
	}

	public int AdaButtonNumber
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(AdaButtonNumberProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaButtonNumberProperty, value);
			});
		}
	}

	public bool IsAdaMode
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsAdaModeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAdaModeProperty, value);
			});
		}
	}

	public DynamicRoutedCommand AddRemoveButtonCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(AddRemoveButtonCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddRemoveButtonCommandProperty, value);
			});
		}
	}

	public Visibility IsAddedTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(IsAddedTextVisibilityProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAddedTextVisibilityPropertyKey, value);
			});
		}
	}

	public bool IsAdded
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsAddedProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAddedProperty, value);
			});
		}
	}

	public Style MainButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(MainButtonStyleProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MainButtonStylePropertyKey, value);
			});
		}
	}

	public Style AddRemoveButtonStyle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(AddRemoveButtonStyleProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddRemoveButtonStylePropertyKey, value);
			});
		}
	}

	public Visibility AddRemoveButtonXVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddRemoveButtonXVisibilityProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddRemoveButtonXVisibilityPropertyKey, value);
			});
		}
	}

	public Visibility AddRemoveButtonPlusVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddRemoveButtonPlusVisibilityProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddRemoveButtonPlusVisibilityPropertyKey, value);
			});
		}
	}

	public Brush PromoTextBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(PromoTextBrushProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromoTextBrushPropertyKey, value);
			});
		}
	}

	public Brush ExpirationTextBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(ExpirationTextBrushProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ExpirationTextBrushPropertyKey, value);
			});
		}
	}

	public StoredPromoCodeListItem()
	{
		InitializeComponent();
		SetStylesForAddedStatus(isAdded: false);
	}

	private static void IsAdaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is StoredPromoCodeListItem storedPromoCodeListItem)
		{
			storedPromoCodeListItem.SetStylesForAddedStatus(storedPromoCodeListItem.IsAdded);
		}
	}

	private static void IsAddedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is StoredPromoCodeListItem storedPromoCodeListItem)
		{
			storedPromoCodeListItem.SetStylesForAddedStatus((bool)e.NewValue);
		}
	}

	private void SetStylesForAddedStatus(bool isAdded)
	{
		if (isAdded)
		{
			MainButtonStyle = FindResource("style_flat_selectedpurple_button") as Style;
			AddRemoveButtonXVisibility = Visibility.Visible;
			IsAddedTextVisibility = Visibility.Visible;
			AddRemoveButtonPlusVisibility = Visibility.Collapsed;
			PromoTextBrush = Brushes.White;
			ExpirationTextBrush = Brushes.White;
		}
		else
		{
			MainButtonStyle = FindResource("style_flat_warmpurple_button") as Style;
			AddRemoveButtonXVisibility = Visibility.Collapsed;
			IsAddedTextVisibility = Visibility.Collapsed;
			AddRemoveButtonPlusVisibility = Visibility.Visible;
		}
		if (IsAdaMode)
		{
			AddRemoveButtonPlusVisibility = Visibility.Collapsed;
			AddRemoveButtonXVisibility = Visibility.Collapsed;
			AddRemoveButtonStyle = FindResource("style_rb_rubine_white_border_with_outlineshadow_button") as Style;
		}
		else
		{
			AddRemoveButtonStyle = FindResource("style_white_no_border_button") as Style;
		}
	}
}
