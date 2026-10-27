using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Views;

public partial class MarketingScreenSaverUserControl : UserControl, IComponentConnector
{
	public static readonly DependencyProperty IsAnimationOnProperty = DependencyProperty.Register("IsAnimationOn", typeof(bool), typeof(MarketingScreenSaverUserControl), new PropertyMetadata(false));

	public bool IsAnimationOn
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsAnimationOnProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAnimationOnProperty, value);
			});
		}
	}

	public MarketingScreenSaverUserControl()
	{
		InitializeComponent();
	}
}
