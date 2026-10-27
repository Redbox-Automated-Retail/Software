using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public partial class WelcomeViewUserControl : UserControl, IComponentConnector
{
	private List<UserControl> _controlList = new List<UserControl>();

	private int _nextLocation = 1;

	public WelcomeViewUserControl()
	{
		InitializeComponent();
	}

	private void OnAnimationFinish(object sender, EventArgs e)
	{
		_nextLocation = (_nextLocation + 1) % _controlList.Count();
		grid1.Children.Clear();
		grid1.Children.Add(grid2.Children[0]);
		grid2.Children.Clear();
		grid2.Children.Add(_controlList[_nextLocation]);
		base.Dispatcher.Invoke(delegate
		{
			sb.Begin();
		});
	}

	private void HelpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		(base.DataContext as WelcomeViewModel)?.ProcessNavigateHelp();
	}

	private void ToggleLanguageModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		NagivateStartView(inSpanish: true);
	}

	private void ToggleADAModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		NagivateStartView(inSpanish: false, inAda: true);
	}

	private void GeneralClickCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		NagivateStartView();
	}

	private void NagivateStartView(bool inSpanish = false, bool inAda = false)
	{
		(base.DataContext as WelcomeViewModel)?.ProcessNavigateStart(inSpanish, inAda);
	}
}
