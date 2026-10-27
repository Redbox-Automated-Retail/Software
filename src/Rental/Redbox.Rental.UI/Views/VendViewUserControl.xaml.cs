using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "VendView")]
public partial class VendViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	public bool IsPickup { get; set; }

	private VendViewModel Model => base.DataContext as VendViewModel;

	public VendViewType VendType
	{
		get
		{
			return VendDiscControl.VendType;
		}
		set
		{
			VendDiscControl.VendType = value;
			VendDiscControl.IsPickupDisc = IsPickup;
			VendDiscControl.IsAnimated = true;
		}
	}

	public BitmapImage BannerImage
	{
		set
		{
			AdImageElem.Source = value;
			VendBannerGrid.Visibility = ((AdImageElem.Source == null) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	public override ISpeechControl GetSpeechControl()
	{
		ISpeechControl result = new SpeechControl
		{
			Name = "tts_vend_view"
		};
		AddSpeechPartMessage(result);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartMessage(ISpeechControl result)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 0,
			Name = "Message",
			Loop = true,
			StartPause = 500,
			EndPause = 10000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = "Your rentals are on their way."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "Thank you for using Redbox!"
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "Please retrieve your disc from the vending slot."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "The vending slot is located on the far right."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "Thanks!"
			});
		};
		result.SpeechParts.Add(speechPart);
	}

	public VendViewUserControl()
	{
		InitializeComponent();
	}

	private void VendDiscControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (Model != null && VendDiscControl != null)
		{
			IsPickup = Model.IsPickup;
			VendDiscControl.IsSignedIn = Model.IsSignedIn;
			VendDiscControl.TitleText1 = Model.TitleText1;
			VendDiscControl.MessageText1 = Model.MessageText1;
			VendDiscControl.MessageText2 = Model.MessageText2;
			VendDiscControl.MessageText3 = Model.MessageText3;
			VendType = Model.VendType;
			BannerImage = Model.BannerImage;
		}
	}
}
