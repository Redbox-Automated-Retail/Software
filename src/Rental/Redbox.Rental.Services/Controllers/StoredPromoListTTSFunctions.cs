using System;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public static class StoredPromoListTTSFunctions
{
	private static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	public static void AddPromoListTTS(PromoListLogic logic, ITextToSpeechService ttsService, ISpeechPart speechPart, IPromoListModel model)
	{
		speechPart.Texts.Add(new TextPart
		{
			Text = "You have a total of " + model.StoredPromoCodes.Count + " offers available."
		});
		string activePromoCode = string.Empty;
		if (RentalShoppingCartService?.CurrentRentalShoppingCart != null)
		{
			activePromoCode = RentalShoppingCartService.CurrentRentalShoppingCart.Promotion?.Name;
		}
		if (!string.IsNullOrEmpty(activePromoCode))
		{
			IPerksOfferListItem perksOfferListItem = model.StoredPromoCodes.FirstOrDefault((IPerksOfferListItem p) => p is StoredPromoCodeModel && ((StoredPromoCodeModel)p).PromoCode == activePromoCode);
			speechPart.Texts.Add(new TextPart
			{
				Text = "The offer "
			});
			SayPromoInfo(speechPart, (StoredPromoCodeModel)perksOfferListItem);
			speechPart.Texts.Add(new TextPart
			{
				Text = "is currently in your bag."
			});
		}
		if (model.NumberOfPages > 1)
		{
			speechPart.Texts.Add(new TextPart
			{
				Text = "There " + model.NumberOfPages + " pages with up to " + model.PromosPerPage + " offers on each page."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "You are currently viewing page " + model.CurrentPageNumber
			});
			if (model.CurrentPageNumber < model.NumberOfPages)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press the plus key to go to the next page."
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "PLUS",
					Action = delegate
					{
						model.CurrentPageNumber++;
						logic.AddNextPagePressedAnalytics();
						ttsService.TTSRepeatSequence();
					}
				});
			}
			if (model.CurrentPageNumber > 1)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press the minus key to go to the previous page."
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "MINUS",
					Action = delegate
					{
						model.CurrentPageNumber--;
						logic.AddPreviousPagePressedAnalytics();
						ttsService.TTSRepeatSequence();
					}
				});
			}
			if (model.CurrentPagePromos.Count == 1)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "There is " + model.CurrentPagePromos.Count + " offer on this page."
				});
			}
			else
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "There are " + model.CurrentPagePromos.Count + " offers on this page."
				});
			}
		}
		foreach (IPerksOfferListItem currentPagePromo in model.CurrentPagePromos)
		{
			if (currentPagePromo is StoredPromoCodeModel)
			{
				StoredPromoCodeModel promoInfo = (StoredPromoCodeModel)currentPagePromo;
				string text = (promoInfo.IsAdded ? "remove" : "add");
				string text2 = (promoInfo.IsAdded ? "MINUS" : "PLUS");
				speechPart.Texts.Add(new TextPart
				{
					Text = "To " + text + " the promo "
				});
				SayPromoInfo(speechPart, promoInfo);
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press " + text2 + " then " + promoInfo.AdaButtonNumber + "."
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = text2 + promoInfo.AdaButtonNumber,
					Action = delegate
					{
						model.AddOrRemovePromoCommand.Execute(promoInfo);
						ttsService.TTSRepeatSequence();
					}
				});
			}
			else
			{
				if (!(currentPagePromo is PerksOfferListItemModel))
				{
					continue;
				}
				PerksOfferListItemModel offerInfo = (PerksOfferListItemModel)currentPagePromo;
				if (offerInfo.OfferStatus != "Completed")
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "To hear the details about the offer "
					});
					SayPointsOfferInfo(speechPart, offerInfo);
					speechPart.Texts.Add(new TextPart
					{
						Text = "Press Plus then " + offerInfo.AdaButtonNumber + "."
					});
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "PLUS" + offerInfo.AdaButtonNumber,
						Action = delegate
						{
							offerInfo.DetailsButtonCommand.Execute(offerInfo);
							ttsService.TTSRepeatSequence();
						}
					});
				}
				else
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "Congrats! you earned " + offerInfo.OfferValueText + " points for completing " + offerInfo.Name
					});
				}
			}
		}
	}

	private static void SayPointsOfferInfo(ISpeechPart speechPart, PerksOfferListItemModel offerInfo)
	{
		speechPart.Texts.Add(new TextPart
		{
			Text = offerInfo.OfferValueText + " points " + offerInfo.Name + " " + offerInfo.TimeToComplete + " " + offerInfo.Description + " "
		});
	}

	public static void SayPromoInfo(ISpeechPart speechPart, StoredPromoCodeModel promoInfo)
	{
		GetMaskedPromoCodeEndCharacters(promoInfo.MaskedPromoCode);
		speechPart.Texts.Add(new TextPart
		{
			Text = promoInfo.PromoCodeDescription + ", " + promoInfo.PromoCodeExpirationText + ","
		});
	}

	public static string GetMaskedPromoCodeEndCharacters(string maskedPromoCode)
	{
		string text = maskedPromoCode.Substring(Math.Max(0, maskedPromoCode.Length - 4));
		string text2 = string.Empty;
		string text3 = text;
		foreach (char c in text3)
		{
			text2 += $"{c}{((c == 'A') ? ',' : ' ')} ";
		}
		return text2;
	}
}
