using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class NewCartConfirmADAActionUpdateViewTextToSpeech
{
	private class TTSConstants
	{
		public const string ListCartItems = "ListCartItems";
	}

	private static int CartCount
	{
		get
		{
			int result = 0;
			if (RentalShoppingCartService?.CurrentRentalShoppingCart != null)
			{
				result = RentalShoppingCartService.CurrentRentalShoppingCart.Count;
			}
			return result;
		}
	}

	private static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	public static ISpeechControl GetSpeechControl(Func<NewCartConfirmADAActionUpdateViewModel> getBrowseViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = string.Format("tts_{0}", "new_cart_confirm_ada_action_update_view");
		AddSpeechPartListCartItems(result, getBrowseViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartListCartItems(ISpeechControl speechControl, Func<NewCartConfirmADAActionUpdateViewModel> getShoppingCartViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 2,
			Name = "ListCartItems",
			Loop = true,
			StartPause = 1000,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			NewCartConfirmADAActionUpdateViewModel viewModel = getShoppingCartViewModelFunction();
			if (viewModel != null)
			{
				AddNextAudioSnippetEvent(speechPart);
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						viewModel.OkButtonCommand.Execute(null);
					}
				});
				int num = 1;
				foreach (DisplayCheckoutProductModel item in viewModel.DisplayProductModels)
				{
					if (item.OfferActionModel != null && item.OfferActionModel.ButtonVisibility == Visibility.Visible)
					{
						speechPart.Events.Add(new MapKeyPressToAction
						{
							KeyCode = num.ToString(),
							Function = "TTSRepeatSpeechPart",
							Action = delegate
							{
								item.OfferActionModel.ButtonCommand.Execute(null);
							}
						});
					}
					num++;
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "Update your bag"
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = string.Format("Your bag has {0} title{1}.", CartCount, (CartCount != 1) ? "s" : null)
				});
				int num2 = 0;
				foreach (DisplayCheckoutProductModel displayProductModel in viewModel.DisplayProductModels)
				{
					num2++;
					if (displayProductModel.OfferActionModel != null && displayProductModel.OfferActionModel.ButtonVisibility != Visibility.Visible)
					{
						speechPart.Texts.Add(new TextPart
						{
							Text = "title " + displayProductModel.Name + " does not qualify for the discount."
						});
					}
					else
					{
						speechPart.Texts.Add(new TextPart
						{
							Text = $"title {displayProductModel.Name}, {displayProductModel.OfferActionModel.TextLine1}, to {displayProductModel.OfferActionModel.ButtonText}, press {num2}."
						});
					}
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "To go back to the previous screen, press the ENTER key."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddNextAudioSnippetEvent(ISpeechPart speechPart)
	{
		speechPart.Events.Add(new MapKeyPress
		{
			KeyCode = "TAB",
			Function = "TTSNextAudioSnippet"
		});
	}
}
