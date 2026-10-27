using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Controls;

public partial class TitleDetailsDevLabelValueUserControl : UserControl, IComponentConnector
{
	public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(TitleDetailsDevLabelValueUserControl), new PropertyMetadata(string.Empty));

	public static readonly DependencyProperty ValueTextProperty = DependencyProperty.Register("ValueText", typeof(string), typeof(TitleDetailsDevLabelValueUserControl), new PropertyMetadata(string.Empty));

	public string LabelText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(LabelTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(LabelTextProperty, value);
			});
		}
	}

	public string ValueText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ValueTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ValueTextProperty, value);
			});
		}
	}

	public TitleDetailsDevLabelValueUserControl()
	{
		InitializeComponent();
	}
}
