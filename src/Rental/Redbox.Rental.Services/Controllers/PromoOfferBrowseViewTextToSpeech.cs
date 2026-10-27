using System;
using System.Linq;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class PromoOfferBrowseViewTextToSpeech
{
	private class TTSConstants
	{
		public const string CheckForCartFull = "CheckForCartFull";

		public const string CartIsFull = "CartIsFull";

		public const string CheckForItemsInCart = "CheckForItemsInCart";

		public const string ListCartItems = "ListCartItems";

		public const string MainMenu = "MainMenu";

		public const string GeneralInfo = "GeneralInfo";

		public const string AvailableTitles = "AvailableTitles";

		public const string FilterItems = "FilterItems";

		public const string GotoAlphaCharacter = "GotoAlphaCharacter";

		public const string PostAlphaSelectionPause = "PostAlphaSelectionPause";

		public const string AlphaSelectionEmpty = "AlphaSelectionEmpty";

		public const string GotoSpeechPartAvailableTitles = "GotoSpeechPartAvailableTitles";
	}

	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	private static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	private static IPromoOfferState PromoOfferState => Redbox.Rental.Services.Session.Session.Instance.PromoOfferState;

	public static ISpeechControl GetSpeechControl(Func<PromoOfferBrowseViewModel> getBrowseViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_promo_offer_browse_view";
		AddSpeechPartCheckForItemsInCart(result);
		AddSpeechPartListCartItems(result, getBrowseViewModelFunction);
		AddSpeechPartGeneralInfo(result, getBrowseViewModelFunction);
		AddSpeechPartAvailableTitles(result, getBrowseViewModelFunction);
		AddSpeechPartMainMenu(result, getBrowseViewModelFunction);
		AddSpeechPartGotoAvailableTitles(result, getBrowseViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartCheckForFullCart(ISpeechControl speechControl)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 0,
			Name = "CheckForCartFull",
			StartPause = 250
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			string text = ((RentalShoppingCartService?.CurrentRentalShoppingCart?.IsFull == true) ? "CartIsFull" : "CheckForItemsInCart");
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

	private static void AddSpeechPartCartIsFull(ISpeechControl speechControl, Func<PromoOfferBrowseViewModel> getBrowseViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "CartIsFull",
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			PromoOfferBrowseViewModel promoOfferBrowseViewModel = getBrowseViewModelFunction();
			if (promoOfferBrowseViewModel != null)
			{
				AddCheckOutEvent(speechPart, promoOfferBrowseViewModel);
				speechPart.Texts.Add(new TextPart
				{
					Text = "Your cart is full, you are unable to add more titles at this time unless you remove some from checkout."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press Enter to Checkout."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartCheckForItemsInCart(ISpeechControl speechControl)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 2,
			Name = "CheckForItemsInCart"
		};
		speechPart.Refresh = delegate
		{
			string text = (((RentalShoppingCartService?.CurrentRentalShoppingCart?.Count).GetValueOrDefault() == 0) ? "MainMenu" : "ListCartItems");
			speechPart.Clear();
			speechPart.RegularExpression = new RegularExpression
			{
				Success = text,
				Failure = text
			};
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartListCartItems(ISpeechControl speechControl, Func<PromoOfferBrowseViewModel> getBrowseViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 3,
			Name = "ListCartItems",
			StartPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			PromoOfferBrowseViewModel promoOfferBrowseViewModel = getBrowseViewModelFunction();
			if (promoOfferBrowseViewModel != null)
			{
				AddNextAudioSnippetEvent(speechPart);
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "CANCEL",
					Function = "TTSSkipToSpeechPart",
					Value = "AvailableTitles"
				});
				AddCheckOutEvent(speechPart, promoOfferBrowseViewModel);
				int valueOrDefault = (RentalShoppingCartService?.CurrentRentalShoppingCart?.Count).GetValueOrDefault();
				speechPart.Texts.Add(new TextPart
				{
					Text = $"You have {valueOrDefault} {GetCartTitlePlurality()} in your cart."
				});
				if (Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.SelectedProduct != null)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "Press Enter to Checkout."
					});
				}
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartGeneralInfo(ISpeechControl speechControl, Func<PromoOfferBrowseViewModel> getBrowseViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 4,
			Name = "GeneralInfo",
			StartPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			PromoOfferBrowseViewModel promoOfferBrowseViewModel = getBrowseViewModelFunction();
			if (promoOfferBrowseViewModel != null)
			{
				AddNextAudioSnippetEvent(speechPart);
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "CANCEL",
					Function = "TTSSkipToSpeechPart",
					Value = "MainMenu"
				});
				AddCheckOutEvent(speechPart, promoOfferBrowseViewModel);
				speechPart.Texts.Add(new TextPart
				{
					Text = promoOfferBrowseViewModel.HeaderText
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = $"There are a total of {(promoOfferBrowseViewModel?.BrowseProductControlModel?.BrowseItems?.Count).GetValueOrDefault()} titles available with up to {(promoOfferBrowseViewModel?.BrowseProductControlModel?.ItemsPerPage).GetValueOrDefault()} titles per screen."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press cancel to go back to list of options."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartAvailableTitles(ISpeechControl speechControl, Func<PromoOfferBrowseViewModel> getBrowseViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 5,
			Name = "AvailableTitles",
			StartPause = 2500,
			EndPause = 250,
			Loop = true
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			PromoOfferBrowseViewModel browseViewModel = getBrowseViewModelFunction();
			if (browseViewModel != null)
			{
				AddNextAudioSnippetEvent(speechPart);
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "CANCEL",
					Function = "TTSSkipToSpeechPart",
					Value = "MainMenu"
				});
				AddCheckOutEvent(speechPart, browseViewModel);
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "MINUS",
					Function = "TTSRepeatSpeechPart",
					Action = delegate
					{
						if (browseViewModel?.BrowseProductControlModel != null && browseViewModel.BrowseProductControlModel.PageNumber > 1)
						{
							browseViewModel.BrowseProductControlModel.SetPageNumber(browseViewModel.BrowseProductControlModel.PageNumber - 1, PageChangeSource.PaddleButton);
							CommandManager.InvalidateRequerySuggested();
						}
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "PLUS",
					Function = "TTSRepeatSpeechPart",
					Action = delegate
					{
						if (browseViewModel?.BrowseProductControlModel != null && browseViewModel.BrowseProductControlModel.PageNumber < browseViewModel.BrowseProductControlModel.TotalPages)
						{
							browseViewModel.BrowseProductControlModel.SetPageNumber(browseViewModel.BrowseProductControlModel.PageNumber + 1, PageChangeSource.PaddleButton);
							CommandManager.InvalidateRequerySuggested();
						}
					}
				});
				int valueOrDefault = (PromoOfferState?.TTSBrowseItemModelsShown?.Count((IBrowseItemModel x) => x is DisplayProductModel)).GetValueOrDefault();
				speechPart.Texts.Add(new TextPart
				{
					Text = $"You are currently browsing on page {(browseViewModel?.BrowseProductControlModel?.PageNumber).GetValueOrDefault()} of {(browseViewModel?.BrowseProductControlModel?.TotalPages).GetValueOrDefault()} pages and there are {valueOrDefault} titles on this screen."
				});
				if ((browseViewModel?.BrowseProductControlModel?.PageNumber).GetValueOrDefault() > 1)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = $"To hear the previous {(browseViewModel?.BrowseProductControlModel?.ItemsPerPage).GetValueOrDefault()} titles, press the minus key."
					});
				}
				if (PromoOfferState?.TTSBrowseItemModelsShown != null)
				{
					foreach (IBrowseItemModel eachTTSBrowseItemModel in PromoOfferState.TTSBrowseItemModelsShown)
					{
						if (eachTTSBrowseItemModel?.Data is ITitleProduct titleProduct)
						{
							string text = (PromoOfferState.TTSBrowseItemModelsShown.IndexOf(eachTTSBrowseItemModel) + 1).ToString();
							speechPart.Events.Add(new MapKeyPressToAction
							{
								KeyCode = text,
								Action = delegate
								{
									browseViewModel.BrowseProductControlModel.ProcessBrowseItemModelSelected(eachTTSBrowseItemModel, null);
								}
							});
							speechPart.Texts.Add(new TextPart
							{
								Text = $"For {titleProduct.ProductName}, Press {text} to add to cart."
							});
							if (eachTTSBrowseItemModel.CanAdd)
							{
								speechPart.Events.Add(new MapKeyPressToAction
								{
									KeyCode = $"PLUS{text}",
									Text = $"you added {titleProduct.ProductName} to your cart.",
									Action = delegate
									{
										browseViewModel.BrowseProductControlModel.ProcessAddBrowseItemModel(eachTTSBrowseItemModel, null);
										CommandManager.InvalidateRequerySuggested();
									}
								});
							}
						}
					}
				}
				if ((browseViewModel?.BrowseProductControlModel?.PageNumber).GetValueOrDefault() < (browseViewModel?.BrowseProductControlModel?.TotalPages).GetValueOrDefault())
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = $"To hear {(browseViewModel?.BrowseProductControlModel?.ItemsPerPage).GetValueOrDefault()} more titles, press the plus key."
					});
				}
				if (Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.SelectedProduct != null)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "Press Enter to Checkout."
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press cancel to go back to list of options."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartMainMenu(ISpeechControl speechControl, Func<PromoOfferBrowseViewModel> getBrowseViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 6,
			Name = "MainMenu",
			Loop = true,
			StartPause = 500,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			PromoOfferBrowseViewModel browseViewModel = getBrowseViewModelFunction();
			if (browseViewModel != null)
			{
				AddNextAudioSnippetEvent(speechPart);
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "1",
					Function = "TTSSkipToSpeechPart",
					Value = "GeneralInfo"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						browseViewModel.CancelButtonCommand.Execute(null);
					}
				});
				AddCheckOutEvent(speechPart, browseViewModel);
				speechPart.Texts.Add(new TextPart
				{
					Text = "To browse all available products, press 1."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To Skip this offer and continue with your current cart, press cancel."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartGotoAvailableTitles(ISpeechControl speechControl, Func<PromoOfferBrowseViewModel> getBrowseViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 13,
			Name = "GotoSpeechPartAvailableTitles",
			Loop = false,
			StartPause = 250,
			EndPause = 250
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.RegularExpression = new RegularExpression
			{
				Success = "AvailableTitles",
				Failure = "AvailableTitles"
			};
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddCheckOutEvent(ISpeechPart speechPart, PromoOfferBrowseViewModel browseViewModel)
	{
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "ENTER",
			Action = delegate
			{
				browseViewModel?.ContinueButtonCommand.Execute(null);
			}
		});
	}

	private static void AddNextAudioSnippetEvent(ISpeechPart speechPart)
	{
		speechPart.Events.Add(new MapKeyPress
		{
			KeyCode = "TAB",
			Function = "TTSNextAudioSnippet"
		});
	}

	private static string GetCartTitlePlurality()
	{
		if (RentalShoppingCartService?.CurrentRentalShoppingCart?.Count != 1)
		{
			return "Titles";
		}
		return "Title";
	}
}
