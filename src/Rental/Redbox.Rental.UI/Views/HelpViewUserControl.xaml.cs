using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "HelpViewUserControl")]
public partial class HelpViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private HelpViewModel Model => (HelpViewModel)base.DataContext;

	public HelpViewUserControl()
	{
		InitializeComponent();
		base.Loaded += HelpViewUserControl_Loaded;
		RegisterRoutedCommand("ShowDocument", HelpViewCommands.ShowDocument);
		RegisterRoutedCommand("PageLeft", HelpViewCommands.PageLeft);
		RegisterRoutedCommand("PageRight", HelpViewCommands.PageRight);
		RegisterRoutedCommand("GoBack", HelpViewCommands.GoBack);
	}

	private void HelpViewUserControl_Loaded(object sender, RoutedEventArgs e)
	{
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		theme?.SetStyle(back_button);
		for (int i = 0; i < buttons_items_control.Items.Count; i++)
		{
			FrameworkElement frameworkElement = (FrameworkElement)buttons_items_control.ItemContainerGenerator.ContainerFromIndex(i);
			if (frameworkElement != null)
			{
				theme?.SetStyle(frameworkElement);
			}
		}
	}

	private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessGoBack();
		HandleWPFHit();
	}

	private void ShowDocumentCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		bool flag = false;
		HelpDocuments helpDocument = HelpDocuments.Faq;
		if (e.OriginalSource is RoundedButton roundedButton)
		{
			helpDocument = (HelpDocuments)roundedButton.Tag;
			flag = true;
		}
		else if (e.Parameter != null)
		{
			try
			{
				helpDocument = (HelpDocuments)Enum.Parse(typeof(HelpDocuments), e.Parameter.ToString());
				flag = true;
			}
			catch
			{
			}
		}
		if (flag)
		{
			Model?.ProcessHelpDocumentButtonPress(helpDocument);
		}
		HandleWPFHit();
	}

	private void RedboxLogoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessRedboxLogoCommand();
	}

	private void HandleHelpPaddleButtonPresed(string source)
	{
		Model?.ProcessHelpPaddleButtonPressed(source);
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.OnGetSpeechControl();
	}
}
