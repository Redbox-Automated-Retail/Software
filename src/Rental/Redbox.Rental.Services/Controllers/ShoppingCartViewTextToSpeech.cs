using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class ShoppingCartViewTextToSpeech
{
	private class TTSConstants
	{
		public const string CheckForCartEmpty = "CheckForCartEmpty";

		public const string CartIsEmpty = "CartIsEmpty";

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

	public static ISpeechControl GetSpeechControl(Func<ShoppingCartViewModel> getBrowseViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_shopping_cart_view";
		AddSpeechPartCheckForCartEmpty(result);
		AddSpeechPartCartIsEmpty(result, getBrowseViewModelFunction);
		AddSpeechPartListCartItems(result, getBrowseViewModelFunction);
		AddSpeechPartDailyRentalCharge(result, getBrowseViewModelFunction);
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
			string text = ((CartCount == 0) ? "CartIsEmpty" : "ListCartItemsStart");
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

	private static void AddSpeechPartCartIsEmpty(ISpeechControl speechControl, Func<ShoppingCartViewModel> getShoppingCartViewModelFunction)
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
			ShoppingCartViewModel shoppingCartViewModel = getShoppingCartViewModelFunction();
			if (shoppingCartViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						shoppingCartViewModel.ProcessOnBackButtonClicked();
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "3",
					Action = delegate
					{
						shoppingCartViewModel.ProcessOnUpdateBagButtonClicked();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Your Cart is Empty."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To go back to the previous screen, Press the CANCEL key."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To update you cart, press 3."
				});
				AddSignInOrUsePointsEvent(speechPart, shoppingCartViewModel);
				AddSignInOrUsePointsText(speechPart, shoppingCartViewModel);
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSignInOrUsePointsEvent(ISpeechPart speechPart, ShoppingCartViewModel shoppingCartViewModel)
	{
		if (shoppingCartViewModel.SignInButtonVisibility == Visibility.Visible)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "4",
				Action = delegate
				{
					shoppingCartViewModel.ProcessOnSignInButtonClicked();
				}
			});
		}
		else if (shoppingCartViewModel.UsePointsButtonVisibility == Visibility.Visible)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "4",
				Action = delegate
				{
					shoppingCartViewModel.ProcessOnUsePointsButtonClicked();
				}
			});
		}
	}

	private static void AddSignInOrUsePointsText(ISpeechPart speechPart, ShoppingCartViewModel shoppingCartViewModel)
	{
		if (shoppingCartViewModel.SignInButtonVisibility == Visibility.Visible)
		{
			speechPart.Texts.Add(new TextPart
			{
				Text = "To sign in press 4."
			});
		}
		else if (shoppingCartViewModel.UsePointsButtonVisibility == Visibility.Visible)
		{
			speechPart.Texts.Add(new TextPart
			{
				Text = "To use points press 4."
			});
		}
	}

	private static void AddSpeechPartListCartItems(ISpeechControl speechControl, Func<ShoppingCartViewModel> getShoppingCartViewModelFunction)
	{
		int num = 3;
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
			ShoppingCartViewModel shoppingCartViewModel2 = getShoppingCartViewModelFunction();
			if (shoppingCartViewModel2 != null)
			{
				AddListCartItemsEvents(speechPart, shoppingCartViewModel2);
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
		ShoppingCartViewModel shoppingCartViewModel = getShoppingCartViewModelFunction();
		if (shoppingCartViewModel != null && CartCount > 0)
		{
			int num2 = 0;
			List<SpeechPart> speechParts = new List<SpeechPart>();
			foreach (IRentalShoppingCartTitleItem eachCartItem in RentalShoppingCartService.CurrentRentalShoppingCart.TitleItems)
			{
				int speechPartIndex = num2;
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
					ShoppingCartViewModel shoppingCartViewModel2 = getShoppingCartViewModelFunction();
					if (shoppingCartViewModel2 != null)
					{
						AddListCartItemsEvents(speechParts[speechPartIndex], shoppingCartViewModel2);
						DisplayCheckoutProductModel displayCheckoutProductModel = shoppingCartViewModel.DisplayProductModels[speechPartIndex];
						string text = null;
						text = ((eachCartItem.Action != RentalShoppingCartItemAction.purchase) ? displayCheckoutProductModel.ProductDescriptionLine2Text.ReplaceFirst("|", $"{eachCartItem.Price:C}. ") : $"{eachCartItem.Price:C} {eachCartItem.Action}");
						speechParts[speechPartIndex].Texts.Add(new TextPart
						{
							Text = $"{eachCartItem.Action} title {displayCheckoutProductModel.Name} {text}."
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
			ShoppingCartViewModel shoppingCartViewModel2 = getShoppingCartViewModelFunction();
			if (shoppingCartViewModel2 != null)
			{
				AddListCartItemsEvents(speechPart2, shoppingCartViewModel2);
				if (!string.IsNullOrEmpty(shoppingCartViewModel?.PricingModel?.DiscountValueText))
				{
					speechPart2.Texts.Add(new TextPart
					{
						Text = string.Format("The discount amount is {0}.", shoppingCartViewModel?.PricingModel?.DiscountValueText.Replace("-", string.Empty))
					});
				}
				ShoppingCartViewModel shoppingCartViewModel3 = shoppingCartViewModel;
				if (shoppingCartViewModel3 != null && shoppingCartViewModel3.PricingModel?.ServiceFeeLineVisibility == Visibility.Visible)
				{
					speechPart2.Texts.Add(new TextPart
					{
						Text = $"The service fee is {shoppingCartViewModel?.PricingModel?.ServiceFeeValueText}."
					});
				}
				speechPart2.Texts.Add(new TextPart
				{
					Text = $"The subtotal amount is {shoppingCartViewModel?.PricingModel?.SubtotalValueText} and the tax is {shoppingCartViewModel?.PricingModel?.TaxValueText}."
				});
				speechPart2.Texts.Add(new TextPart
				{
					Text = $"The total amount that will be charged to your credit card will be ${shoppingCartViewModel?.PricingModel?.TotalValueText}."
				});
				ShoppingCartViewModel shoppingCartViewModel4 = shoppingCartViewModel;
				if (shoppingCartViewModel4 != null && shoppingCartViewModel4.PointsEstimateVisibility == Visibility.Visible)
				{
					speechPart2.Texts.Add(new TextPart
					{
						Text = string.Format("You'll earn " + shoppingCartViewModel?.PointsEstimateText + " for the stuff in your bag.")
					});
				}
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
			ShoppingCartViewModel shoppingCartViewModel2 = getShoppingCartViewModelFunction();
			if (shoppingCartViewModel2 != null)
			{
				AddListCartItemsEvents(speechPart3, shoppingCartViewModel2);
				if (shoppingCartViewModel.PayButtonEnabled)
				{
					speechPart3.Texts.Add(new TextPart
					{
						Text = "To pay with a credit card, press ENTER."
					});
				}
				speechPart3.Texts.Add(new TextPart
				{
					Text = "To update your cart press 3."
				});
				if (shoppingCartViewModel.PromoCodeButtonEnabled)
				{
					IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
					string text;
					if ((personalizationSession?.StoredPromoCodes?.Count).GetValueOrDefault() > 0)
					{
						int count = personalizationSession.StoredPromoCodes.Count;
						text = string.Format("You have {0} promo{1} available. If you would like to use {2} now, ", count, (count > 1) ? "s" : "", (count > 1) ? "one" : "it");
					}
					else
					{
						text = "If you have a promo code, and would like to use it now, ";
					}
					text += "press 1.";
					speechPart3.Texts.Add(new TextPart
					{
						Text = text
					});
				}
				speechPart3.Texts.Add(new TextPart
				{
					Text = "By paying with credit card you agree to the terms."
				});
				speechPart3.Texts.Add(new TextPart
				{
					Text = "To hear our terms and privacy in full, press 2."
				});
				speechPart3.Texts.Add(new TextPart
				{
					Text = "To go back to the previous screen, press the CANCEL key."
				});
				AddSignInOrUsePointsText(speechPart3, shoppingCartViewModel);
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

	private static void AddListCartItemsEvents(ISpeechPart speechPart, ShoppingCartViewModel shoppingCartViewModel)
	{
		AddNextAudioSnippetEvent(speechPart);
		if (shoppingCartViewModel.PayButtonEnabled)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "ENTER",
				Action = delegate
				{
					shoppingCartViewModel.ProcessOnPayButtonClicked();
				}
			});
		}
		if (shoppingCartViewModel.PromoCodeButtonEnabled)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "1",
				Action = delegate
				{
					shoppingCartViewModel.ProcessOnPromoCodeButtonClicked();
				}
			});
		}
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "2",
			Action = delegate
			{
				shoppingCartViewModel.ProcessOnTermsAndPrivacyButtonClicked();
			}
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "CANCEL",
			Action = delegate
			{
				shoppingCartViewModel.ProcessOnBackButtonClicked();
			}
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "3",
			Action = delegate
			{
				shoppingCartViewModel.ProcessOnUpdateBagButtonClicked();
			}
		});
		speechPart.Events.Add(new MapKeyPress
		{
			KeyCode = "5",
			Function = "TTSSkipToSpeechPart",
			Value = "DailyRentalCharges"
		});
		AddSignInOrUsePointsEvent(speechPart, shoppingCartViewModel);
	}

	private static void AddSpeechPartDailyRentalCharge(ISpeechControl speechControl, Func<ShoppingCartViewModel> getShoppingCartViewModelFunction)
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
			speechPart.Events.Add(new MapKeyPress
			{
				KeyCode = "TAB",
				Function = "TTSSkipToSpeechPart",
				Value = "CheckForCartEmpty"
			});
			speechPart.Events.Add(new MapKeyPress
			{
				KeyCode = "CANCEL",
				Function = "TTSSkipToSpeechPart",
				Value = "CheckForCartEmpty"
			});
			speechPart.Events.Add(new MapKeyPress
			{
				KeyCode = "MINUS",
				Function = "TTSSkipToSpeechPart",
				Value = "CheckForCartEmpty"
			});
			speechPart.Events.Add(new MapKeyPress
			{
				KeyCode = "ENTER",
				Function = "TTSSkipToSpeechPart",
				Value = "CheckForCartEmpty"
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = Resources.passive_sale_note
			});
			ShoppingCartViewModel shoppingCartViewModel = getShoppingCartViewModelFunction();
			if (shoppingCartViewModel != null && shoppingCartViewModel.ServiceFeeTextVisibility == Visibility.Visible)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = shoppingCartViewModel.ServiceFeeText
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
