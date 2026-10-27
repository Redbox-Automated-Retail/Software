using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "IdleTimeoutMessageView")]
public partial class IdleTimeoutMessageViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private object m_lockObject = new object();

	private IdleTimeoutMessageViewModel _idleTimeoutMessageViewModel;

	public IdleTimeoutMessageViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
		RegisterRoutedCommand("Yes", Commands.Yes);
		RegisterRoutedCommand("No", Commands.No);
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(YesButton);
		obj?.SetStyle(NoButton);
	}

	public override ISpeechControl GetSpeechControl()
	{
		return _idleTimeoutMessageViewModel.OnGetSpeechControl();
	}

	private void YesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		_idleTimeoutMessageViewModel?.ProcessOnButtonYesClicked();
	}

	private void NoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		_idleTimeoutMessageViewModel?.ProcessOnButtonNoClicked();
	}

	private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_idleTimeoutMessageViewModel = base.DataContext as IdleTimeoutMessageViewModel;
	}
}
