using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class KeypadUserControl : UserControl, IComponentConnector
{
	private KeypadModel KeypadModel => base.DataContext as KeypadModel;

	public KeypadUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(OneButton);
		obj?.SetStyle(TwoButton);
		obj?.SetStyle(ThreeButton);
		obj?.SetStyle(FourButton);
		obj?.SetStyle(FiveButton);
		obj?.SetStyle(SixButton);
		obj?.SetStyle(SevenButton);
		obj?.SetStyle(EightButton);
		obj?.SetStyle(NineButton);
		obj?.SetStyle(ZeroButton);
		obj?.SetStyle(ClearAllButton);
		obj?.SetStyle(BackButton);
	}

	private void NumberCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		KeypadModel?.ProcessOnNumberButtonClicked((e.OriginalSource as RoundedButton)?.Tag?.ToString());
		Commands.ResetIdleTimerCommand.Execute(null, this);
	}

	private void ClearAllCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		KeypadModel?.ProcessOnClearAllButtonClicked();
		Commands.ResetIdleTimerCommand.Execute(null, this);
	}

	private void BackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		KeypadModel?.ProcessOnBackButtonClicked();
		Commands.ResetIdleTimerCommand.Execute(null, this);
	}
}
