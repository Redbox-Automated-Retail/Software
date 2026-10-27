using System.Windows;
using System.Windows.Markup;

namespace Redbox.KioskEngine.PreferenceManager;

public partial class SimpleMessageWindow : Window, IComponentConnector
{
	public SimpleMessageWindow()
	{
		InitializeComponent();
	}

	public static void Show(Window owner, string title, string message)
	{
		SimpleMessageWindow simpleMessageWindow = new SimpleMessageWindow();
		simpleMessageWindow.Owner = owner;
		simpleMessageWindow.DataContext = new SimpleMessageWindowModel
		{
			Title = title,
			Message = message
		};
		simpleMessageWindow.ShowDialog();
	}

	private void OkButton_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
