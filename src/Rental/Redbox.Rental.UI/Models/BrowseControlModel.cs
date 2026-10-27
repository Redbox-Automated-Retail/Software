using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public class BrowseControlModel : DependencyObject
{
	public static readonly DependencyProperty BottomPageNumberVisibilityProperty = DependencyProperty.Register("BottomPageNumberVisibility", typeof(Visibility), typeof(BrowseControlModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty ShowPaddlesProperty = DependencyProperty.Register("ShowPaddles", typeof(bool), typeof(TitleDetailModel), new FrameworkPropertyMetadata(true));

	public static readonly DependencyProperty SelectedDisplayProductUserControlSizeAndPositionProperty = DependencyProperty.Register("SelectedDisplayProductUserControlSizeAndPosition", typeof(SizeAndPosition), typeof(BrowseControlModel), new FrameworkPropertyMetadata(new SizeAndPosition(), OnSelectedDisplayProductUserControlSizeAndPositionPropertyChanged));

	public static readonly DependencyProperty TitleRollupOverlayModelProperty = DependencyProperty.Register("TitleRollupOverlayModel", typeof(TitleRollupOverlayModel), typeof(BrowseControlModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty NoResultsTextProperty = DependencyProperty.Register("NoResultsText", typeof(string), typeof(BrowseControlModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PageNumberProperty = DependencyProperty.Register("PageNumber", typeof(int), typeof(BrowseControlModel), new FrameworkPropertyMetadata(1));

	public static readonly DependencyProperty BrowseItemsProperty = DependencyProperty.Register("BrowseItems", typeof(List<IBrowseItemModel>), typeof(BrowseControlModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RedboxPlusMiniCartVisiblityProperty = DependencyProperty.Register("RedboxPlusMiniCartVisiblity", typeof(Visibility), typeof(BrowseControlModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	private Color _backgroundColor = Color.FromRgb(229, 229, 229);

	private List<Control> _browseControls = new List<Control>();

	public Visibility BorderShadowVisibility { get; set; }

	public BrowseLayoutDirection LayoutDirection { get; set; }

	public int RowCount { get; set; }

	public int ColumnCount { get; set; }

	public int PageNumber => base.Dispatcher.Invoke(() => (int)GetValue(PageNumberProperty));

	public bool UseFullPageWidth { get; set; }

	public bool ShowNoResultsMessage { get; set; }

	public Color BackgroundColor
	{
		get
		{
			return _backgroundColor;
		}
		set
		{
			_backgroundColor = value;
		}
	}

	public int VisibleItemCount => RowCount * ColumnCount;

	public bool ShowBorderItems { get; set; }

	public Visibility PageNumberVisibility { get; set; }

	public Visibility BottomPageNumberVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BottomPageNumberVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BottomPageNumberVisibilityProperty, value);
			});
		}
	}

	public double PaddleOverlapWidth { get; set; }

	public Thickness BrowseItemMargin { get; set; }

	public Thickness Margin { get; set; }

	public List<Control> BrowseControls
	{
		get
		{
			return _browseControls;
		}
		set
		{
			_browseControls = value;
		}
	}

	public List<IBrowseItemModel> BrowseItems
	{
		get
		{
			return base.Dispatcher.Invoke(() => (List<IBrowseItemModel>)GetValue(BrowseItemsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseItemsProperty, value);
			});
		}
	}

	public SizeAndPosition SelectedBrowseItemSizeAndPosition
	{
		get
		{
			return base.Dispatcher.Invoke(() => (SizeAndPosition)GetValue(SelectedDisplayProductUserControlSizeAndPositionProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SelectedDisplayProductUserControlSizeAndPositionProperty, value);
			});
		}
	}

	public string NoResultsText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(NoResultsTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NoResultsTextProperty, value);
			});
		}
	}

	public int ItemsPerPage => base.Dispatcher.Invoke(() => RowCount * ColumnCount);

	public int ItemsOnCurrentPage => base.Dispatcher.Invoke(() => (PageNumber >= TotalPages) ? ((BrowseItems?.Count ?? 0) % ItemsPerPage) : ItemsPerPage);

	public int TotalPages => base.Dispatcher.Invoke(() => (BrowseItems?.Count ?? 0) / ItemsPerPage + (((BrowseItems?.Count ?? 0) % ItemsPerPage > 0) ? 1 : 0));

	public bool ShowPaddles
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(ShowPaddlesProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ShowPaddlesProperty, value);
			});
		}
	}

	public string PageNumberText { get; set; }

	public event BrowseItemModelEvent OnBrowseItemModelSelected;

	public event BrowseItemModelEvent OnAddBrowseItemModel;

	public event BrowseItemModelEvent OnCancelBrowseItemModel;

	public event PageChangedEvent OnPageChanged;

	public event BrowseItemsModelEvent OnNotifyBrowseItemsShown;

	public event Action<BrowseControlModel> OnSelectedDisplayProductUserControlSizeAndPositionChanged;

	public void SetPageNumber(int value, PageChangeSource pageChangeSource)
	{
		int pageNumber = PageNumber;
		base.Dispatcher.Invoke(delegate
		{
			SetValue(PageNumberProperty, value);
		});
		ProcessPageChanged(pageNumber, value, pageChangeSource);
	}

	public int GetPageOfBrowseItemIndex(int browseItemIndex)
	{
		return Convert.ToInt32(Math.Ceiling(Convert.ToDouble(browseItemIndex + 1) / (double)VisibleItemCount));
	}

	public void ProcessAddBrowseItemModel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnAddBrowseItemModel != null)
		{
			OnAddBrowseItemModel(browseItemModel, parameter);
		}
	}

	public void ProcessCancelBrowseItemModel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnCancelBrowseItemModel != null)
		{
			OnCancelBrowseItemModel(browseItemModel, parameter);
		}
	}

	public void ProcessBrowseItemModelSelected(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnBrowseItemModelSelected != null)
		{
			OnBrowseItemModelSelected(browseItemModel, parameter);
		}
	}

	public void ProcessPageChanged(int prevPageNumber, int newPageNumber, PageChangeSource pageChangeSource)
	{
		OnPageChanged?.Invoke(prevPageNumber, newPageNumber, pageChangeSource);
	}

	public void ProcessNotifyBrowseItemsShown(List<IBrowseItemModel> browseitemModels, object parameter)
	{
		if (OnNotifyBrowseItemsShown != null)
		{
			OnNotifyBrowseItemsShown(browseitemModels, parameter);
		}
	}

	private static void OnSelectedDisplayProductUserControlSizeAndPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is BrowseControlModel { OnSelectedDisplayProductUserControlSizeAndPositionChanged: not null } browseControlModel)
		{
			browseControlModel.OnSelectedDisplayProductUserControlSizeAndPositionChanged(browseControlModel);
		}
	}
}
