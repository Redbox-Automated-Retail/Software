using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.UI.Controls;

public partial class BrowseUserControl : UserControl, IComponentConnector
{
	private struct RowAndColumn
	{
		public int Row { get; set; }

		public int Column { get; set; }
	}

	private enum PageDirection
	{
		Left,
		Right
	}

	public static readonly DependencyProperty ShowPaddlesProperty = DependencyProperty.Register("ShowPaddles", typeof(bool), typeof(BrowseUserControl), new FrameworkPropertyMetadata(false, OnShowPaddlesPropertyChanged)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty LeftPaddleVisibilityProperty = DependencyProperty.Register("LeftPaddleVisibility", typeof(Visibility), typeof(BrowseUserControl), new FrameworkPropertyMetadata(Visibility.Visible)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty RightPaddleVisibilityProperty = DependencyProperty.Register("RightPaddleVisibility", typeof(Visibility), typeof(BrowseUserControl), new FrameworkPropertyMetadata(Visibility.Visible)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty BorderShadowVisibilityProperty = DependencyProperty.Register("BorderShadowVisibility", typeof(Visibility), typeof(BrowseUserControl), new FrameworkPropertyMetadata(Visibility.Visible)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty PageGridWidthProperty = DependencyProperty.Register("PageGridWidth", typeof(double), typeof(BrowseUserControl), new FrameworkPropertyMetadata(1024.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty PaddleOverlapWidthProperty = DependencyProperty.Register("PaddleOverlapWidth", typeof(double), typeof(BrowseUserControl), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty PageWrapGridUnderPaddlesProperty = DependencyProperty.Register("PageWrapPanelUnderPaddles", typeof(bool), typeof(BrowseUserControl), new FrameworkPropertyMetadata(false)
	{
		AffectsRender = true
	});

	private int _browseItemIndex;

	private BrowseControlModel _browseControlModel;

	private PageDirection _animationScrollDirection;

	private bool _animationScrollInProgress;

	private FrameworkElement _scrollControl;

	private List<Control> m_priorPageBrowseControls = new List<Control>();

	private List<Control> m_currentPageBrowseControls = new List<Control>();

	private List<Control> m_nextPageBrowseControls = new List<Control>();

	public int RowCount { get; set; }

	public int ColumnCount { get; set; }

	public int VisibleItemCount => RowCount * ColumnCount;

	public int PageCount => Convert.ToInt32(Math.Ceiling(Convert.ToDouble((_browseControlModel?.BrowseItems?.Count).GetValueOrDefault()) / (double)VisibleItemCount));

	public int CurrentPage
	{
		get
		{
			return GetPageOfBrowseItemIndex(_browseItemIndex);
		}
		set
		{
			_browseItemIndex = Math.Min(VisibleItemCount * (Math.Max(1, value) - 1), PageCount * VisibleItemCount);
		}
	}

	public bool ShowPaddles
	{
		get
		{
			return (bool)GetValue(ShowPaddlesProperty);
		}
		set
		{
			SetValue(ShowPaddlesProperty, value);
		}
	}

	public Visibility LeftPaddleVisibility
	{
		get
		{
			return (Visibility)GetValue(LeftPaddleVisibilityProperty);
		}
		set
		{
			SetValue(LeftPaddleVisibilityProperty, value);
		}
	}

	public Visibility RightPaddleVisibility
	{
		get
		{
			return (Visibility)GetValue(RightPaddleVisibilityProperty);
		}
		set
		{
			SetValue(RightPaddleVisibilityProperty, value);
		}
	}

	public Visibility BorderShadowVisibility
	{
		get
		{
			return (Visibility)GetValue(BorderShadowVisibilityProperty);
		}
		set
		{
			SetValue(BorderShadowVisibilityProperty, value);
		}
	}

	public double PageGridWidth
	{
		get
		{
			return (double)GetValue(PageGridWidthProperty);
		}
		set
		{
			SetValue(PageGridWidthProperty, value);
		}
	}

	public double PaddleOverlapWidth
	{
		get
		{
			return (double)GetValue(PaddleOverlapWidthProperty);
		}
		set
		{
			SetValue(PaddleOverlapWidthProperty, value);
		}
	}

	public bool PageWrapGridUnderPaddles
	{
		get
		{
			return (bool)GetValue(PageWrapGridUnderPaddlesProperty);
		}
		set
		{
			SetValue(PageWrapGridUnderPaddlesProperty, value);
		}
	}

	public event BrowsePageNumberChangedEvent OnBeforePageNumberChange;

	public BrowseUserControl()
	{
		InitializeComponent();
		ApplyStyle();
	}

	private void ApplyStyle()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(LeftPaddleButton);
		obj?.SetStyle(RightPaddleButton);
	}

	private static void OnShowPaddlesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as BrowseUserControl).RenderNavigationButtons();
	}

	public int GetPageOfBrowseItemIndex(int browseItemIndex)
	{
		return Convert.ToInt32(Math.Ceiling(Convert.ToDouble(browseItemIndex + 1) / (double)VisibleItemCount));
	}

	public void Render()
	{
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		list.AddRange(RenderPage(m_priorPageBrowseControls, VisibleItemCount * -1, isBorderPage: true));
		list.AddRange(RenderPage(m_currentPageBrowseControls, 0, isBorderPage: false));
		list.AddRange(RenderPage(m_nextPageBrowseControls, VisibleItemCount, isBorderPage: true));
		RenderNoResultsMessage();
		RenderNavigationButtons();
		RenderPageNumbers();
		_browseControlModel?.ProcessNotifyBrowseItemsShown(list, null);
	}

	private void RenderNoResultsMessage()
	{
		Grid noResultsGrid = NoResultsGrid;
		BrowseControlModel browseControlModel = _browseControlModel;
		noResultsGrid.Visibility = ((browseControlModel == null || !browseControlModel.ShowNoResultsMessage || (_browseControlModel?.BrowseItems?.Count).GetValueOrDefault() != 0) ? Visibility.Collapsed : Visibility.Visible);
	}

	private List<IBrowseItemModel> RenderPage(List<Control> pageControls, int indexOffset, bool isBorderPage)
	{
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		for (int i = 0; i < VisibleItemCount; i++)
		{
			int num = _browseItemIndex + i + indexOffset;
			IBrowseItemModel browseItemModel = ((num > -1 && _browseControlModel.BrowseItems != null && num < _browseControlModel.BrowseItems.Count) ? _browseControlModel.BrowseItems[num] : null);
			if (browseItemModel != null)
			{
				browseItemModel.IsBorderItem = isBorderPage;
				browseItemModel.VisibleItemIndex = i;
				RowAndColumn rowAndColumnFromIndex = GetRowAndColumnFromIndex(i);
				browseItemModel.CurrentGridRow = rowAndColumnFromIndex.Row + 1;
				browseItemModel.CurrentGridColumn = rowAndColumnFromIndex.Column + 1;
				browseItemModel.IsBottomRow = ((_browseControlModel != null && _browseControlModel.LayoutDirection == BrowseLayoutDirection.LeftToRightThenTopToBottom) ? (i >= (RowCount - 1) * ColumnCount) : ((i + 1) % RowCount == 0));
				list.Add(browseItemModel);
			}
			if (pageControls.Count > i)
			{
				Control control = pageControls[i];
				if (control != null)
				{
					control.DataContext = browseItemModel;
				}
			}
		}
		return list;
	}

	private void RenderNavigationButtons()
	{
		if (_browseControlModel != null)
		{
			if (ShowPaddles)
			{
				LeftPaddleVisibility = ((_browseItemIndex <= 0) ? Visibility.Collapsed : Visibility.Visible);
				RightPaddleVisibility = ((_browseItemIndex + VisibleItemCount >= (_browseControlModel?.BrowseItems?.Count).GetValueOrDefault()) ? Visibility.Collapsed : Visibility.Visible);
			}
			else
			{
				LeftPaddleVisibility = Visibility.Collapsed;
				RightPaddleVisibility = Visibility.Collapsed;
			}
		}
	}

	private void RenderPageNumbers()
	{
		string text = string.Format(_browseControlModel?.PageNumberText ?? Redbox.Rental.UI.Properties.Resources.browse_view_control_page_mask, CurrentPage, PageCount);
		PageNumberText.Text = text;
		BottomPageNumberText.Text = text;
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_browseControlModel = e.NewValue as BrowseControlModel;
		RowCount = _browseControlModel?.RowCount ?? 1;
		ColumnCount = _browseControlModel?.ColumnCount ?? 1;
		if (_browseControlModel != null)
		{
			BorderShadowVisibility = _browseControlModel.BorderShadowVisibility;
			ConfigureBrowseItemGrid();
			UpdatePageGridWidth();
			CurrentPage = _browseControlModel.PageNumber;
			Render();
			_browseControlModel.OnPageChanged += _browseControlModel_OnPageChanged;
		}
	}

	private void _browseControlModel_OnPageChanged(int prevPageNumber, int newPageNumber, PageChangeSource pageChangedSource)
	{
		if (newPageNumber != prevPageNumber)
		{
			CurrentPage = newPageNumber;
			Render();
		}
	}

	private void ConfigureBrowseItemGrid()
	{
		m_priorPageBrowseControls.Clear();
		m_currentPageBrowseControls.Clear();
		m_nextPageBrowseControls.Clear();
		PriorPageGrid.Children.Clear();
		CurrentPageGrid.Children.Clear();
		NextPageGrid.Children.Clear();
		CurrentPageGrid.Margin = _browseControlModel.Margin;
		PriorPageGrid.Margin = _browseControlModel.Margin;
		NextPageGrid.Margin = _browseControlModel.Margin;
		PriorPageGrid.RowDefinitions.Clear();
		PriorPageGrid.ColumnDefinitions.Clear();
		CurrentPageGrid.RowDefinitions.Clear();
		CurrentPageGrid.ColumnDefinitions.Clear();
		NextPageGrid.RowDefinitions.Clear();
		NextPageGrid.ColumnDefinitions.Clear();
		for (int i = 0; i < RowCount; i++)
		{
			PriorPageGrid.RowDefinitions.Add(new RowDefinition());
			CurrentPageGrid.RowDefinitions.Add(new RowDefinition());
			NextPageGrid.RowDefinitions.Add(new RowDefinition());
		}
		for (int j = 0; j < ColumnCount; j++)
		{
			PriorPageGrid.ColumnDefinitions.Add(new ColumnDefinition());
			CurrentPageGrid.ColumnDefinitions.Add(new ColumnDefinition());
			NextPageGrid.ColumnDefinitions.Add(new ColumnDefinition());
		}
		for (int k = 0; k < VisibleItemCount; k++)
		{
			if (_browseControlModel != null && _browseControlModel.ShowBorderItems)
			{
				Control control = _browseControlModel?.BrowseControls[k];
				ConfigureBrowseItemControlInGrid(PriorPageGrid, control, m_priorPageBrowseControls, k);
				Control control2 = _browseControlModel?.BrowseControls[k + VisibleItemCount * 2];
				ConfigureBrowseItemControlInGrid(NextPageGrid, control2, m_nextPageBrowseControls, k);
			}
			int num = ((_browseControlModel != null && _browseControlModel.ShowBorderItems) ? VisibleItemCount : 0);
			Control control3 = _browseControlModel?.BrowseControls[k + num];
			ConfigureBrowseItemControlInGrid(CurrentPageGrid, control3, m_currentPageBrowseControls, k);
		}
	}

	private void ConfigureBrowseItemControlInGrid(Grid grid, Control control, List<Control> controlList, int index)
	{
		control.Margin = _browseControlModel.BrowseItemMargin;
		controlList.Add(control);
		RowAndColumn rowAndColumnFromIndex = GetRowAndColumnFromIndex(index);
		Grid.SetColumn(control, rowAndColumnFromIndex.Column);
		Grid.SetRow(control, rowAndColumnFromIndex.Row);
		grid.Children.Add(control);
	}

	private RowAndColumn GetRowAndColumnFromIndex(int index)
	{
		RowAndColumn result = default(RowAndColumn);
		if (_browseControlModel.LayoutDirection == BrowseLayoutDirection.LeftToRightThenTopToBottom)
		{
			result.Row = index / ColumnCount;
			result.Column = index % ColumnCount;
		}
		else if (_browseControlModel.LayoutDirection == BrowseLayoutDirection.TopToBottomThenLeftToRight)
		{
			result.Row = index % RowCount;
			result.Column = index / RowCount;
		}
		return result;
	}

	private void UpdateBrowseItemIndex(int newBrowseItemIndex)
	{
		_browseItemIndex = newBrowseItemIndex;
		Render();
	}

	private void PageLeftCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Commands.ResetIdleTimerCommand.Execute(null, this);
		Scroll(PageDirection.Left);
	}

	private void PageRightCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Commands.ResetIdleTimerCommand.Execute(null, this);
		Scroll(PageDirection.Right);
	}

	private void Scroll(PageDirection direction)
	{
		UpdateSelectedBrowseItemSizeAndPosition(null);
		if (OnBeforePageNumberChange != null)
		{
			OnBeforePageNumberChange(CurrentPage);
		}
		ScrollProducts(direction);
	}

	private void ScrollProducts(PageDirection direction)
	{
		int newBrowseItemIndex = ((direction == PageDirection.Left) ? Math.Max(0, _browseItemIndex - VisibleItemCount) : Math.Min(_browseItemIndex + VisibleItemCount, _browseControlModel.BrowseItems.Count - 1));
		UpdateBrowseItemIndex(newBrowseItemIndex);
		if (_browseControlModel != null)
		{
			_browseControlModel.SetPageNumber(CurrentPage, PageChangeSource.PaddleButton);
		}
	}

	private void AnimateScroll(PageDirection direction)
	{
		if (!_animationScrollInProgress)
		{
			_animationScrollInProgress = true;
			_animationScrollDirection = direction;
			_scrollControl = BrowsePagesStackPanel;
			TimeSpan timeSpan = TimeSpan.FromSeconds(0.5);
			Storyboard obj = new Storyboard
			{
				Duration = timeSpan
			};
			double toValue = _scrollControl.ActualWidth / 3.0 * (double)((direction == PageDirection.Left) ? 1 : (-1));
			DoubleAnimation doubleAnimation = new DoubleAnimation(0.0, toValue, timeSpan);
			doubleAnimation.Completed += Animation_Completed;
			TranslateTransform renderTransform = new TranslateTransform();
			_scrollControl.RenderTransform = renderTransform;
			Storyboard.SetTarget(doubleAnimation, _scrollControl);
			Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
			obj.Children.Add(doubleAnimation);
			obj.Begin();
		}
	}

	private void Animation_Completed(object sender, EventArgs e)
	{
		_scrollControl.RenderTransform = null;
		ScrollProducts(_animationScrollDirection);
		_animationScrollInProgress = false;
		CommandManager.InvalidateRequerySuggested();
	}

	private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		UpdatePageGridWidth();
	}

	private void UpdatePageGridWidth()
	{
		double num = (ShowPaddles ? (Math.Max(LeftPaddleButton.ActualWidth, LeftPaddleButton.Width) * 2.0) : 0.0);
		double actualWidth = base.ActualWidth;
		double num2;
		if (!PageWrapGridUnderPaddles)
		{
			BrowseControlModel browseControlModel = _browseControlModel;
			if (browseControlModel == null || !browseControlModel.UseFullPageWidth)
			{
				num2 = Math.Max((num - _browseControlModel?.PaddleOverlapWidth).GetValueOrDefault(), 0.0);
				goto IL_00ca;
			}
		}
		num2 = 0.0;
		goto IL_00ca;
		IL_00ca:
		PageGridWidth = Math.Max(0.0, actualWidth - num2);
	}

	private void BrowseItemAddCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		UserControl browseItemUserControl = GetBrowseItemUserControl(e.OriginalSource);
		if (browseItemUserControl != null)
		{
			UpdateSelectedBrowseItemSizeAndPosition(browseItemUserControl);
		}
		Commands.ResetIdleTimerCommand.Execute(null, this);
		IBrowseItemModel browseItemModel = (e?.OriginalSource as FrameworkElement)?.DataContext as IBrowseItemModel;
		if (_browseControlModel != null)
		{
			_browseControlModel.ProcessAddBrowseItemModel(browseItemModel, e.Parameter);
		}
	}

	private void UpdateSelectedBrowseItemSizeAndPosition(UserControl selectedUserControl)
	{
		SizeAndPosition sizeAndPosition = new SizeAndPosition();
		if (selectedUserControl != null)
		{
			Point point = selectedUserControl.TransformToAncestor(this).Transform(new Point(0.0, 0.0));
			sizeAndPosition.Left = point.X;
			sizeAndPosition.Top = point.Y;
			sizeAndPosition.Width = selectedUserControl.ActualWidth;
			sizeAndPosition.Height = selectedUserControl.ActualHeight;
		}
		if (_browseControlModel != null)
		{
			_browseControlModel.SelectedBrowseItemSizeAndPosition = sizeAndPosition;
		}
	}

	private UserControl GetBrowseItemUserControl(object source)
	{
		FrameworkElement frameworkElement = source as FrameworkElement;
		while (frameworkElement != null && !(frameworkElement is UserControl) && frameworkElement.Parent != CurrentPageGrid)
		{
			frameworkElement = frameworkElement.Parent as FrameworkElement;
		}
		return frameworkElement as UserControl;
	}

	private void BrowseItemCancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		UserControl browseItemUserControl = GetBrowseItemUserControl(e.OriginalSource);
		if (browseItemUserControl != null)
		{
			Commands.ResetIdleTimerCommand.Execute(null, this);
			IBrowseItemModel browseItemModel = browseItemUserControl.DataContext as IBrowseItemModel;
			if (_browseControlModel != null)
			{
				_browseControlModel.ProcessCancelBrowseItemModel(browseItemModel, e.Parameter);
			}
		}
	}

	private void BrowseItemSelectedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		UserControl browseItemUserControl = GetBrowseItemUserControl(e.OriginalSource);
		if (browseItemUserControl != null)
		{
			UpdateSelectedBrowseItemSizeAndPosition(browseItemUserControl);
		}
		Commands.ResetIdleTimerCommand.Execute(null, this);
		IBrowseItemModel browseItemModel = (e?.OriginalSource as FrameworkElement)?.DataContext as IBrowseItemModel;
		if (_browseControlModel != null && browseItemModel != null && !browseItemModel.IsBorderItem)
		{
			_browseControlModel.ProcessBrowseItemModelSelected(browseItemModel, e.Parameter);
		}
	}

	private void BrowseItemAddCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = ((e?.OriginalSource as FrameworkElement)?.DataContext as IBrowseItemModel)?.CanAdd ?? false;
	}
}
