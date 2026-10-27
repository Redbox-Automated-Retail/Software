using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class StoredPromoCodeListControl : BaseUserControl, IComponentConnector
{
	private static DependencyProperty PromoCodeModelsProperty = DependencyProperty.Register("PromoCodeModels", typeof(List<IPerksOfferListItem>), typeof(StoredPromoCodeListControl), new PropertyMetadata(null, PromoCodeModelsChanged));

	private static DependencyProperty IsAdaModeProperty = DependencyProperty.Register("IsAdaMode", typeof(bool), typeof(StoredPromoCodeListControl), new PropertyMetadata(false, IsAdaModeChanged));

	private static DependencyProperty ShowAdaLeftArrowProperty = DependencyProperty.Register("ShowAdaLeftArrow", typeof(bool), typeof(StoredPromoCodeListControl), new PropertyMetadata(false));

	private static DependencyProperty ShowAdaRightArrowProperty = DependencyProperty.Register("ShowAdaRightArrow", typeof(bool), typeof(StoredPromoCodeListControl), new PropertyMetadata(false));

	private static DependencyProperty CurrentPageNumberProperty = DependencyProperty.Register("CurrentPageNumber", typeof(int), typeof(StoredPromoCodeListControl), new PropertyMetadata(1, CurrentPageNumberChanged));

	private static DependencyProperty CurrentPageCountProperty = DependencyProperty.Register("CurrentPageCount", typeof(int), typeof(StoredPromoCodeListControl), new PropertyMetadata(1, CurrentPageNumberChanged));

	private static DependencyProperty ItemsPerPageProperty = DependencyProperty.Register("ItemsPerPage", typeof(int), typeof(StoredPromoCodeListControl), new PropertyMetadata(3, ItemsPerPageChanged));

	private static DependencyProperty NextPageClickedCommandProperty = DependencyProperty.Register("NextPageClickedCommand", typeof(DynamicRoutedCommand), typeof(StoredPromoCodeListControl), new PropertyMetadata(null));

	private static DependencyProperty PreviousPageClickedCommandProperty = DependencyProperty.Register("PreviousPageClickedCommand", typeof(DynamicRoutedCommand), typeof(StoredPromoCodeListControl), new PropertyMetadata(null));

	private static DependencyProperty PageNumberClickedCommandProperty = DependencyProperty.Register("PageNumberClickedCommand", typeof(DynamicRoutedCommand), typeof(StoredPromoCodeListControl), new PropertyMetadata(null));

	private static DependencyProperty PageShownCommandProperty = DependencyProperty.Register("PageShownCommand", typeof(DynamicRoutedCommand), typeof(StoredPromoCodeListControl), new PropertyMetadata(null));

	private static DependencyPropertyKey NumberOfPagesPropertyKey = DependencyProperty.RegisterReadOnly("NumberOfPages", typeof(int), typeof(StoredPromoCodeListControl), new PropertyMetadata(0, NumberOfPagesChanged));

	public static readonly DependencyProperty NumberOfPagesProperty = NumberOfPagesPropertyKey.DependencyProperty;

	private static DependencyPropertyKey PageControlVisibilityPropertyKey = DependencyProperty.RegisterReadOnly("PageControlVisibility", typeof(Visibility), typeof(StoredPromoCodeListControl), new PropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty PageControlVisibilityProperty = PageControlVisibilityPropertyKey.DependencyProperty;

	private static DependencyPropertyKey AdaArrowColumnWidthPropertyKey = DependencyProperty.RegisterReadOnly("AdaArrowColumnWidth", typeof(double), typeof(StoredPromoCodeListControl), new PropertyMetadata(0.0));

	public static readonly DependencyProperty AdaArrowColumnWidthProperty = PageControlVisibilityPropertyKey.DependencyProperty;

	private static readonly DependencyProperty CurrentPagePromosProperty = DependencyProperty.Register("CurrentPagePromos", typeof(ObservableCollection<IPerksOfferListItem>), typeof(StoredPromoCodeListControl), new PropertyMetadata(new ObservableCollection<IPerksOfferListItem>()));

	private static readonly DependencyProperty AdaButtonDataProperty = DependencyProperty.Register("AdaButtonData", typeof(ObservableCollection<AdaButtonInfo>), typeof(StoredPromoCodeListControl), new PropertyMetadata(new ObservableCollection<AdaButtonInfo>()));

	private static DependencyProperty AdaLeftArrowPressedCommandProperty = DependencyProperty.Register("AdaLeftArrowPressedCommand", typeof(DynamicRoutedCommand), typeof(StoredPromoCodeListControl), new PropertyMetadata(null));

	private static DependencyProperty AdaRightArrowPressedCommandProperty = DependencyProperty.Register("AdaRightArrowPressedCommand", typeof(DynamicRoutedCommand), typeof(StoredPromoCodeListControl), new PropertyMetadata(null));

	private int _isUpdatingPage;

	public List<IPerksOfferListItem> PromoCodeModels
	{
		get
		{
			return base.Dispatcher.Invoke(() => (List<IPerksOfferListItem>)GetValue(PromoCodeModelsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromoCodeModelsProperty, value);
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

	public bool ShowAdaLeftArrow
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(ShowAdaLeftArrowProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ShowAdaLeftArrowProperty, value);
			});
		}
	}

	public bool ShowAdaRightArrow
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(ShowAdaRightArrowProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ShowAdaRightArrowProperty, value);
			});
		}
	}

	public int CurrentPageNumber
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(CurrentPageNumberProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CurrentPageNumberProperty, value);
			});
		}
	}

	public int CurrentPageCount
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(CurrentPageCountProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CurrentPageCountProperty, value);
			});
		}
	}

	public int ItemsPerPage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(ItemsPerPageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ItemsPerPageProperty, value);
			});
		}
	}

	public DynamicRoutedCommand NextPageClickedCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(NextPageClickedCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NextPageClickedCommandProperty, value);
			});
		}
	}

	public DynamicRoutedCommand PreviousPageClickedCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(PreviousPageClickedCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PreviousPageClickedCommandProperty, value);
			});
		}
	}

	public DynamicRoutedCommand PageNumberClickedCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(PageNumberClickedCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PageNumberClickedCommandProperty, value);
			});
		}
	}

	public DynamicRoutedCommand PageShownCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(PageShownCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PageShownCommandProperty, value);
			});
		}
	}

	public int NumberOfPages
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(NumberOfPagesProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NumberOfPagesPropertyKey, value);
			});
		}
	}

	public Visibility PageControlVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PageControlVisibilityProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PageControlVisibilityPropertyKey, value);
			});
		}
	}

	public double AdaArrowColumnWidth
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(AdaArrowColumnWidthProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaArrowColumnWidthPropertyKey, value);
			});
		}
	}

	public ObservableCollection<IPerksOfferListItem> CurrentPagePromos
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ObservableCollection<IPerksOfferListItem>)GetValue(CurrentPagePromosProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CurrentPagePromosProperty, value);
			});
		}
	}

	public ObservableCollection<AdaButtonInfo> AdaButtonData
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ObservableCollection<AdaButtonInfo>)GetValue(AdaButtonDataProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaButtonDataProperty, value);
			});
		}
	}

	public DynamicRoutedCommand AdaLeftArrowPressedCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(AdaLeftArrowPressedCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaLeftArrowPressedCommandProperty, value);
			});
		}
	}

	public DynamicRoutedCommand AdaRightArrowPressedCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(AdaRightArrowPressedCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaRightArrowPressedCommandProperty, value);
			});
		}
	}

	public StoredPromoCodeListControl()
	{
		InitializeComponent();
		PageControl.ButtonPressed += PageControl_ButtonPressed;
		AdaLeftArrowPressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), AdaLeftArrowPressed);
		AdaRightArrowPressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), AdaRightArrowPressed);
	}

	private static void PromoCodeModelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is StoredPromoCodeListControl storedPromoCodeListControl)
		{
			storedPromoCodeListControl.UpdatePageInfo();
		}
	}

	private static void IsAdaModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is StoredPromoCodeListControl storedPromoCodeListControl)
		{
			storedPromoCodeListControl.UpdatePageInfo();
		}
	}

	private void UpdatePageInfo()
	{
		if (Interlocked.CompareExchange(ref _isUpdatingPage, 1, 0) == 1)
		{
			return;
		}
		try
		{
			CurrentPagePromos = new ObservableCollection<IPerksOfferListItem>();
			AdaButtonData = new ObservableCollection<AdaButtonInfo>();
			if ((PromoCodeModels?.Count ?? 0) > 0)
			{
				NumberOfPages = (int)Math.Round((double)PromoCodeModels.Sum((IPerksOfferListItem m) => m.NumberOfSpaces) / (double)ItemsPerPage + 0.4);
				CurrentPageNumber = Math.Min(CurrentPageNumber, NumberOfPages);
				CurrentPageNumber = Math.Max(CurrentPageNumber, 1);
				int num = 0;
				int num2 = 1;
				foreach (IPerksOfferListItem promoCodeModel in PromoCodeModels)
				{
					num += promoCodeModel.NumberOfSpaces;
					if (num > (CurrentPageNumber - 1) * ItemsPerPage && num <= CurrentPageNumber * ItemsPerPage)
					{
						CurrentPagePromos.Add(promoCodeModel);
						promoCodeModel.AdaButtonNumber = num2;
						bool flag = true;
						DynamicRoutedCommand buttonCommand;
						if (promoCodeModel is StoredPromoCodeModel)
						{
							buttonCommand = ((StoredPromoCodeModel)promoCodeModel).AddRemoveButtonCommand;
						}
						else
						{
							flag = ((PerksOfferListItemModel)promoCodeModel).OfferStatus != "Completed";
							buttonCommand = ((PerksOfferListItemModel)promoCodeModel).DetailsButtonCommand;
						}
						if (flag)
						{
							AdaButtonData.Add(new AdaButtonInfo
							{
								AdaButtonNumber = num2,
								ButtonCommand = buttonCommand,
								Item = promoCodeModel
							});
							num2++;
						}
					}
				}
			}
			else
			{
				NumberOfPages = 0;
				CurrentPageNumber = 0;
			}
			PageControlVisibility = ((IsAdaMode || NumberOfPages <= 1) ? Visibility.Collapsed : Visibility.Visible);
			ShowAdaLeftArrow = IsAdaMode && CurrentPageNumber > 1;
			ShowAdaRightArrow = IsAdaMode && CurrentPageNumber < NumberOfPages;
			AdaArrowColumnWidth = (IsAdaMode ? 60 : 0);
		}
		finally
		{
			_isUpdatingPage = 0;
		}
	}

	private static void CurrentPageNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is StoredPromoCodeListControl storedPromoCodeListControl)
		{
			storedPromoCodeListControl.UpdatePageInfo();
		}
	}

	private static void ItemsPerPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is StoredPromoCodeListControl storedPromoCodeListControl)
		{
			if (storedPromoCodeListControl.ItemsPerPage < 1)
			{
				storedPromoCodeListControl.ItemsPerPage = 1;
			}
			storedPromoCodeListControl.UpdatePageInfo();
		}
	}

	private static void NumberOfPagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is StoredPromoCodeListControl storedPromoCodeListControl)
		{
			storedPromoCodeListControl.CurrentPageCount = storedPromoCodeListControl.NumberOfPages;
		}
	}

	private void PageControl_ButtonPressed(object sender, PageButtonPressedArgs e)
	{
		switch (e.ButtonType)
		{
		case PageButtonType.LeftArrow:
			PreviousPageClickedCommand?.Execute(null);
			break;
		case PageButtonType.RightArrow:
			NextPageClickedCommand?.Execute(null);
			break;
		default:
			PageNumberClickedCommand?.Execute(e.NewPageNumber);
			break;
		}
		HandleWPFHit();
	}

	private void AdaRightArrowPressed()
	{
		CurrentPageNumber++;
		NextPageClickedCommand?.Execute(null);
	}

	private void AdaLeftArrowPressed()
	{
		CurrentPageNumber--;
		PreviousPageClickedCommand?.Execute(null);
	}
}
