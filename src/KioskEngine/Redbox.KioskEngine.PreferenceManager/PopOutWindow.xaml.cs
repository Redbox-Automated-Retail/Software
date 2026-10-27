using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using Redbox.KioskEngine.PreferenceManager.Preferences;

namespace Redbox.KioskEngine.PreferenceManager;

public partial class PopOutWindow : Window, IComponentConnector
{
	private IPreferencePageHost _preferencePageHost;

	private IPreferencePage _preferencePage;

	public PopOutWindow()
	{
		InitializeComponent();
	}

	public void SetPreferencePageHost(IPreferencePage preferencePage, FrameworkElement preferencePageHostFrameworkElement)
	{
		_preferencePage = preferencePage;
		_preferencePageHost = preferencePageHostFrameworkElement as IPreferencePageHost;
		MainGrid.Children.Add(preferencePageHostFrameworkElement);
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		_preferencePageHost.SaveValues(_preferencePage);
	}
}
