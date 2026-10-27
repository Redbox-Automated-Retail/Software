using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Controls;

public partial class PagingButtonsControl : UserControl, IComponentConnector
{
	private static DependencyProperty NumberOfPagesProperty = DependencyProperty.Register("NumberOfPages", typeof(int), typeof(PagingButtonsControl), new PropertyMetadata(1, HandleNumberOfPagesChanged));

	private static DependencyProperty ButtonSizeProperty = DependencyProperty.Register("ButtonSize", typeof(int), typeof(PagingButtonsControl), new PropertyMetadata(44));

	private static DependencyProperty CurrentPageNumberProperty = DependencyProperty.Register("CurrentPageNumber", typeof(int), typeof(PagingButtonsControl), new PropertyMetadata(1, HandleCurrentPageChanged));

	private static DependencyPropertyKey ButtonInfoListPropertyKey = DependencyProperty.RegisterReadOnly("ButtonInfoList", typeof(ObservableCollection<object>), typeof(PagingButtonsControl), new PropertyMetadata(null));

	public static readonly DependencyProperty ButtonInfoListProperty = ButtonInfoListPropertyKey.DependencyProperty;

	private static DependencyPropertyKey LeftArrowVisibilityPropertyKey = DependencyProperty.RegisterReadOnly("LeftArrowVisibility", typeof(Visibility), typeof(PagingButtonsControl), new PropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty LeftArrowVisibilityProperty = LeftArrowVisibilityPropertyKey.DependencyProperty;

	private static DependencyPropertyKey RightArrowVisibilityPropertyKey = DependencyProperty.RegisterReadOnly("RightArrowVisibility", typeof(Visibility), typeof(PagingButtonsControl), new PropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty RightArrowVisibilityProperty = RightArrowVisibilityPropertyKey.DependencyProperty;

	public int NumberOfPages
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(NumberOfPagesProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NumberOfPagesProperty, value);
			});
		}
	}

	public int ButtonSize
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(ButtonSizeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ButtonSizeProperty, value);
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

	public ObservableCollection<object> ButtonInfoList
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ObservableCollection<object>)GetValue(ButtonInfoListProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ButtonInfoListPropertyKey, value);
			});
		}
	}

	public Visibility LeftArrowVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(LeftArrowVisibilityProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(LeftArrowVisibilityPropertyKey, value);
			});
		}
	}

	public Visibility RightArrowVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RightArrowVisibilityProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RightArrowVisibilityPropertyKey, value);
			});
		}
	}

	public event EventHandler<PageButtonPressedArgs> ButtonPressed;

	public PagingButtonsControl()
	{
		InitializeComponent();
	}

	private static void HandleNumberOfPagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is PagingButtonsControl pagingButtonsControl)
		{
			pagingButtonsControl.ConfigurePageButtons();
		}
	}

	private static void HandleCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is PagingButtonsControl pagingButtonsControl)
		{
			pagingButtonsControl.ConfigurePageButtons();
		}
	}

	private void DecrementPage()
	{
		if (CurrentPageNumber > 1)
		{
			SetPage(CurrentPageNumber - 1);
		}
	}

	private void IncrementPage()
	{
		if (CurrentPageNumber < NumberOfPages)
		{
			SetPage(CurrentPageNumber + 1);
		}
	}

	private void SetPage(int newPage)
	{
		if (newPage > 0 && newPage <= NumberOfPages)
		{
			CurrentPageNumber = newPage;
			ConfigurePageButtons();
		}
	}

	private void ConfigurePageButtons()
	{
		ObservableCollection<object> observableCollection = new ObservableCollection<object>();
		if (NumberOfPages < 6)
		{
			for (int i = 1; i <= NumberOfPages; i++)
			{
				observableCollection.Add(new PageButtonInfo
				{
					ButtonText = i.ToString(),
					PageNumber = i,
					IsCurrentPage = (i == CurrentPageNumber)
				});
			}
		}
		else
		{
			observableCollection.Add(new PageButtonInfo
			{
				ButtonText = "1",
				PageNumber = 1,
				IsCurrentPage = (CurrentPageNumber == 1)
			});
			if (CurrentPageNumber > 1)
			{
				if (CurrentPageNumber > 2)
				{
					observableCollection.Add(new EllipsesInfo());
				}
				observableCollection.Add(new PageButtonInfo
				{
					ButtonText = CurrentPageNumber.ToString(),
					PageNumber = CurrentPageNumber,
					IsCurrentPage = true
				});
			}
			if (CurrentPageNumber < NumberOfPages)
			{
				if (CurrentPageNumber < NumberOfPages - 1)
				{
					observableCollection.Add(new EllipsesInfo());
				}
				observableCollection.Add(new PageButtonInfo
				{
					ButtonText = NumberOfPages.ToString(),
					PageNumber = NumberOfPages,
					IsCurrentPage = false
				});
			}
		}
		ButtonInfoList = observableCollection;
		if (CurrentPageNumber == 1)
		{
			LeftArrowVisibility = Visibility.Hidden;
		}
		else
		{
			LeftArrowVisibility = Visibility.Visible;
		}
		if (CurrentPageNumber < NumberOfPages)
		{
			RightArrowVisibility = Visibility.Visible;
		}
		else
		{
			RightArrowVisibility = Visibility.Hidden;
		}
	}

	private void LeftArrowCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		DecrementPage();
		ButtonPressed?.Invoke(this, new PageButtonPressedArgs
		{
			ButtonType = PageButtonType.LeftArrow,
			NewPageNumber = CurrentPageNumber
		});
	}

	private void RightArrowCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		IncrementPage();
		ButtonPressed?.Invoke(this, new PageButtonPressedArgs
		{
			ButtonType = PageButtonType.RightArrow,
			NewPageNumber = CurrentPageNumber
		});
	}

	private void NumberCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		SetPage((int)((Control)e.OriginalSource).Tag);
		ButtonPressed?.Invoke(this, new PageButtonPressedArgs
		{
			ButtonType = PageButtonType.Number,
			NewPageNumber = CurrentPageNumber
		});
	}
}
