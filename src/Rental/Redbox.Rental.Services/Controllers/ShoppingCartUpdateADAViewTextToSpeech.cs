using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class ShoppingCartUpdateADAViewTextToSpeech
{
	private class TTSConstants
	{
		public const string CheckForCartEmpty = "CheckForCartEmpty";

		public const string CartIsEmpty = "CartIsEmpty";

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

	public static ISpeechControl GetSpeechControl(Func<ShoppingCartUpdateADAViewModel> getBrowseViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = string.Format("tts_{0}", "shopping_cart_update_ada_view");
		AddSpeechPartCheckForCartEmpty(result);
		AddSpeechPartCartIsEmpty(result, getBrowseViewModelFunction);
		AddSpeechPartListCartItems(result, getBrowseViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartCheckForCartEmpty(ISpeechControl speechControl)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 0,
			Name = "CheckForCartEmpty",
			StartPause = 250
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			string text = ((CartCount == 0) ? "CartIsEmpty" : "ListCartItems");
			speechPart.RegularExpression = new RegularExpression
			{
				Expression = "Jump",
				Value = "Jump",
				Success = text,
				Failure = text
			};
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartCartIsEmpty(ISpeechControl speechControl, Func<ShoppingCartUpdateADAViewModel> getShoppingCartViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "CartIsEmpty",
			Loop = true,
			StartPause = 1000,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			ShoppingCartUpdateADAViewModel shoppingCartUpdateADAViewModel = getShoppingCartViewModelFunction();
			if (shoppingCartUpdateADAViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						shoppingCartUpdateADAViewModel.ProcessOkButtonPress();
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "3",
					Action = delegate
					{
						shoppingCartUpdateADAViewModel.ProcessAddMovieButtonPress();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Your Cart is Empty."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To go back to the previous screen, Press the ENTER key."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To add a movie, press 3."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartListCartItems(ISpeechControl speechControl, Func<ShoppingCartUpdateADAViewModel> getShoppingCartViewModelFunction)
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
			ShoppingCartUpdateADAViewModel shoppingCartUpdateADAViewModel = getShoppingCartViewModelFunction();
			if (shoppingCartUpdateADAViewModel != null)
			{
				AddNextAudioSnippetEvent(speechPart);
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						shoppingCartUpdateADAViewModel.ProcessOkButtonPress();
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "3",
					Action = delegate
					{
						shoppingCartUpdateADAViewModel.ProcessAddMovieButtonPress();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = Resources.this_is_your_chance
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = string.Format("Your cart has {0} title{1}.", CartCount, (CartCount != 1) ? "s" : null)
				});
				int num = 0;
				foreach (IRentalShoppingCartTitleItem titleItem in RentalShoppingCartService.CurrentRentalShoppingCart.TitleItems)
				{
					DisplayProductModel displayModel = shoppingCartUpdateADAViewModel.DisplayProductModels[num];
					num++;
					speechPart.Texts.Add(new TextPart
					{
						Text = $"To remove the {titleItem.Action} title {displayModel.Name} from your cart, press clear and number {num}."
					});
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = $"CLEAR{num}",
						Action = delegate
						{
							shoppingCartUpdateADAViewModel.ProcessOnCancelBrowseItemModel(displayModel, null);
						},
						Text = $"Removed {displayModel.Name}"
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "To go back to the previous screen, press the ENTER key."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To add a movie, press 3."
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
