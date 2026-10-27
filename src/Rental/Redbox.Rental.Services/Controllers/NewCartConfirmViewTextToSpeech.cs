using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class NewCartConfirmViewTextToSpeech
{
	private class TTSConstants
	{
		public const string ListCartItemsStart = "ListCartItemsStart";

		public const string CartItem = "CartItem";

		public const string CartTotals = "CartTotals";

		public const string CartButtons = "CartButtons";

		public const string DailyRentalCharge = "DailyRentalCharges";
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

	public static ISpeechControl GetSpeechControl(Func<NewCartConfirmViewModel> getBrowseViewModelFunction, NewCartConfirmviewParameters parameters)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_confirm_cart_view";
		AddSpeechPartListCartItems(result, getBrowseViewModelFunction, parameters);
		AddSpeechPartDailyRentalCharge(result, getBrowseViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartListCartItems(ISpeechControl speechControl, Func<NewCartConfirmViewModel> getShoppingCartViewModelFunction, NewCartConfirmviewParameters parameters)
	{
		int num = 3;
		NewCartConfirmViewModel viewModel = getShoppingCartViewModelFunction();
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 2,
			Name = "ListCartItemsStart",
			Loop = false,
			StartPause = 1000,
			EndPause = 0
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			if (viewModel != null)
			{
				AddListCartItemsEvents(speechPart, viewModel, parameters);
				speechPart.Texts.Add(new TextPart
				{
					Text = string.Format("Your cart has {0} title{1}.", CartCount, (CartCount != 1) ? "s" : null)
				});
				if (CartCount > 0)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "The titles in your cart are as follows:"
					});
				}
			}
		};
		speechControl.SpeechParts.Add(speechPart);
		if (viewModel != null && CartCount > 0)
		{
			int num2 = 0;
			List<SpeechPart> speechParts = new List<SpeechPart>();
			foreach (DisplayCheckoutProductModel displayModel in viewModel.DisplayProductModels)
			{
				int speechPartIndex = num2;
				IRentalShoppingCartTitleItem eachCartItem = (IRentalShoppingCartTitleItem)displayModel.Data;
				speechParts.Add(new SpeechPart
				{
					Sequence = num++,
					Name = "CartItem" + num2,
					Loop = false,
					StartPause = 500,
					EndPause = 0
				});
				speechParts[speechPartIndex].Refresh = delegate
				{
					speechParts[speechPartIndex].Clear();
					if (viewModel != null)
					{
						AddListCartItemsEvents(speechParts[speechPartIndex], viewModel, parameters);
						decimal num3 = ServiceLocator.Instance.GetService<IPromotionService>().GetPromotion().PromotionItems.FirstOrDefault((IPromotionItem i) => i.RentalShoppingCartItem.ProductId == eachCartItem.ProductId)?.PromotionAmount ?? 0m;
						string text = null;
						IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = eachCartItem;
						if (rentalShoppingCartTitleItem != null && rentalShoppingCartTitleItem.Action == RentalShoppingCartItemAction.purchase)
						{
							text = $"{eachCartItem.Price:C} {eachCartItem.Action}";
						}
						else
						{
							string productDescriptionLine2Text = displayModel.ProductDescriptionLine2Text;
							IRentalShoppingCartTitleItem rentalShoppingCartTitleItem2 = eachCartItem;
							text = productDescriptionLine2Text.ReplaceFirst("|", $"{((rentalShoppingCartTitleItem2 != null) ? new decimal?(rentalShoppingCartTitleItem2.Price - num3) : ((decimal?)null)):C}. ");
						}
						speechParts[speechPartIndex].Texts.Add(new TextPart
						{
							Text = $"{eachCartItem?.Action} title {displayModel.Name} {text}."
						});
					}
				};
				speechControl.SpeechParts.Add(speechParts[num2]);
				num2++;
			}
		}
		SpeechPart speechPart2 = new SpeechPart
		{
			Sequence = num++,
			Name = "CartTotals",
			Loop = false,
			StartPause = 500,
			EndPause = 0
		};
		speechPart2.Refresh = delegate
		{
			speechPart2.Clear();
			if (viewModel != null)
			{
				AddListCartItemsEvents(speechPart2, viewModel, parameters);
				if (!string.IsNullOrEmpty(viewModel?.PricingModel?.DiscountValueText))
				{
					speechPart2.Texts.Add(new TextPart
					{
						Text = string.Format("The discount amount is {0}.", viewModel?.PricingModel?.DiscountValueText.Replace("-", string.Empty))
					});
				}
				NewCartConfirmViewModel newCartConfirmViewModel = viewModel;
				if (newCartConfirmViewModel != null && newCartConfirmViewModel.PricingModel?.ServiceFeeLineVisibility == Visibility.Visible)
				{
					speechPart2.Texts.Add(new TextPart
					{
						Text = $"The service fee is {viewModel?.PricingModel?.ServiceFeeValueText}."
					});
				}
				speechPart2.Texts.Add(new TextPart
				{
					Text = $"The subtotal amount is {viewModel?.PricingModel?.SubtotalValueText} and the tax is {viewModel?.PricingModel?.TaxValueText}."
				});
				speechPart2.Texts.Add(new TextPart
				{
					Text = $"The total amount that will be charged to your credit card will be ${viewModel?.PricingModel?.TotalValueText}."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart2);
		SpeechPart speechPart3 = new SpeechPart
		{
			Sequence = num++,
			Name = "CartButtons",
			Loop = false,
			StartPause = 0,
			EndPause = 37000
		};
		speechPart3.Refresh = delegate
		{
			speechPart3.Clear();
			if (viewModel != null)
			{
				AddListCartItemsEvents(speechPart3, viewModel, parameters);
				speechPart3.Texts.Add(new TextPart
				{
					Text = "To continue with checkout including the added item and discount, press ENTER."
				});
				if (parameters.SelectedOffer.DiscountType == OfferDiscountType.MultiNightPrice)
				{
					speechPart3.Texts.Add(new TextPart
					{
						Text = "To update your cart, press Plus."
					});
				}
				else
				{
					speechPart3.Texts.Add(new TextPart
					{
						Text = "To remove the new item and offer and continue with your original cart, press Cancel."
					});
				}
				speechPart3.Texts.Add(new TextPart
				{
					Text = "By paying with credit card you agree to the terms."
				});
				speechPart3.Texts.Add(new TextPart
				{
					Text = "To know more about our daily rental charges, press 5."
				});
			}
			speechPart3.RegularExpression = new RegularExpression
			{
				Expression = "Jump",
				Value = "Jump",
				Success = "ListCartItemsStart",
				Failure = "ListCartItemsStart"
			};
		};
		speechControl.SpeechParts.Add(speechPart3);
	}

	private static void AddListCartItemsEvents(ISpeechPart speechPart, NewCartConfirmViewModel shoppingCartViewModel, NewCartConfirmviewParameters parameters)
	{
		AddNextAudioSnippetEvent(speechPart);
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "ENTER",
			Action = delegate
			{
				shoppingCartViewModel.SubmitButtonCommand.Execute(null);
			}
		});
		if (parameters.SelectedOffer.DiscountType == OfferDiscountType.MultiNightPrice)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "PLUS",
				Action = delegate
				{
					shoppingCartViewModel.CancelButtonCommand.Execute(null);
				}
			});
		}
		else
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "CANCEL",
				Action = delegate
				{
					shoppingCartViewModel.CancelButtonCommand.Execute(null);
				}
			});
		}
		speechPart.Events.Add(new MapKeyPress
		{
			KeyCode = "5",
			Function = "TTSSkipToSpeechPart",
			Value = "DailyRentalCharges"
		});
	}

	private static void AddSpeechPartDailyRentalCharge(ISpeechControl speechControl, Func<NewCartConfirmViewModel> getShoppingCartViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 11,
			Name = "DailyRentalCharges",
			Loop = false,
			StartPause = 0,
			EndPause = 0
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = Resources.passive_sale_note
			});
			NewCartConfirmViewModel newCartConfirmViewModel = getShoppingCartViewModelFunction();
			if (newCartConfirmViewModel != null && newCartConfirmViewModel.ServiceFeeTextVisibility == Visibility.Visible)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = newCartConfirmViewModel.ServiceFeeText
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
