using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "TitleDetailsView")]
public partial class TitleDetailsUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	protected static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

	private TitleDetailModel Model => base.DataContext as TitleDetailModel;

	public TitleDetailsUserControl()
	{
		InitializeComponent();
		base.Loaded += delegate
		{
			Keyboard.Focus(this);
		};
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.ProcessGetSpeechControl();
	}

	private UserControl GetBrowseItemUserControl(object source)
	{
		FrameworkElement frameworkElement = source as FrameworkElement;
		while (frameworkElement != null && !(frameworkElement is UserControl))
		{
			frameworkElement = VisualTreeHelper.GetParent(frameworkElement) as FrameworkElement;
		}
		return frameworkElement as UserControl;
	}

	private void GoBack(string reason, bool popOnly = false)
	{
		ServiceLocator.Instance.GetService<IViewService>()?.GoBack("go back", popOnly);
	}

	private void OnGoBack_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		AnalyticsService?.AddButtonPressEvent("TitleDetailsBack");
		GoBack("go_back");
		HandleWPFHit();
	}

	private void Buy_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		IBrowseItemModel model = GetBrowseItemUserControl(e.OriginalSource)?.DataContext as IBrowseItemModel;
		Model.ProcessBuyButtonHit(model, e.Parameter);
		HandleWPFHit();
	}

	private void RentProduct_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		IBrowseItemModel model = GetBrowseItemUserControl(e.OriginalSource)?.DataContext as IBrowseItemModel;
		Model.ProcessAddProduct(model, e.Parameter, (TitleType)((Control)e.OriginalSource).Tag);
		HandleWPFHit();
	}

	private void ResetIdleTimerCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		HandleWPFHit();
	}

	private void MoreInfo_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		IBrowseItemModel model = GetBrowseItemUserControl(e.OriginalSource)?.DataContext as IBrowseItemModel;
		Model.ProcessMoreInfoHit(model);
		HandleWPFHit();
	}

	private void SeeFullDetails_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		IBrowseItemModel model = GetBrowseItemUserControl(e.OriginalSource)?.DataContext as IBrowseItemModel;
		Model.ProcessSeeFullDetails(model);
		HandleWPFHit();
	}

	private void RedboxPlusInfo_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model.ProcessRedboxPlusInfo();
		HandleWPFHit();
	}

	private void DualInStockLearnMore_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model.ProcessDualInStockLearnMore();
		HandleWPFHit();
	}

	private void StatisticsPopup_Executed(object sender, ExecutedRoutedEventArgs e)
	{
	}
}
