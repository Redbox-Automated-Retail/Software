using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PerksConfirmView")]
public partial class PerksConfirmViewUserControl : ViewUserControl, IComponentConnector
{
	private PerksConfirmViewModel Model { get; set; }

	public PerksConfirmViewUserControl()
	{
		InitializeComponent();
		ApplyTheme<Control>(MainControl);
		base.SpeechViewName = "perks_confirm_view";
	}

	protected override void DataContextHasChanged()
	{
		if (base.DataContext != null && base.DataContext is PerksConfirmViewModel)
		{
			Model = base.DataContext as PerksConfirmViewModel;
		}
	}

	public override ISpeechControl GetSpeechControl()
	{
		return ((PerksConfirmViewModel)base.DataContext)?.ProcessGetSpeechControl?.Invoke();
	}
}
