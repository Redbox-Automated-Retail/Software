using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class BrowseViewTextToSpeech
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

	private static string SelectedAlphaGroupEmptyMessage { get; set; }

	private static string BrowsingFamily
	{
		get
		{
			if (BrowseState.TitleFamily != TitleFamily.Movies)
			{
				return "Games";
			}
			return "Movies";
		}
	}

	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	private static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	public static ISpeechControl GetSpeechControl(Func<BrowseViewModel> getBrowseViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_browse_view";
		AddSpeechPartCheckForFullCart(result);
		AddSpeechPartCartIsFull(result, getBrowseViewModelFunction);
		AddSpeechPartCheckForItemsInCart(result);
		AddSpeechPartListCartItems(result, getBrowseViewModelFunction);
		AddSpeechPartGeneralInfo(result, getBrowseViewModelFunction);
		AddSpeechPartAvailableTitles(result, getBrowseViewModelFunction);
		AddSpeechPartMainMenu(result, getBrowseViewModelFunction);
		AddSpeechPartFilterSelection(result, getBrowseViewModelFunction);
		AddSpeechPartGotoAlphaCharacter(result, getBrowseViewModelFunction);
		AddSpeechPartPostAlphaSelectionPause(result, getBrowseViewModelFunction);
		AddSpeechPartAlphaSelectionEmpty(result, getBrowseViewModelFunction);
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

	private static void AddSpeechPartCartIsFull(ISpeechControl speechControl, Func<BrowseViewModel> getBrowseViewModelFunction)
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
			BrowseViewModel browseViewModel = getBrowseViewModelFunction();
			if (browseViewModel != null)
			{
				AddCheckOutEvent(speechPart, browseViewModel);
				speechPart.Texts.Add(new TextPart
				{
					Text = "Your cart is full, you are unable to add more titles at this time unless you remove some from checkout."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press Enter to Checkout."
				});
				AddStartOverOrHomeEventAndText(speechPart, browseViewModel);
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

	private static void AddSpeechPartListCartItems(ISpeechControl speechControl, Func<BrowseViewModel> getBrowseViewModelFunction)
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
			BrowseViewModel browseViewModel = getBrowseViewModelFunction();
			if (browseViewModel != null)
			{
				AddNextAudioSnippetEvent(speechPart);
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "CANCEL",
					Function = "TTSSkipToSpeechPart",
					Value = "AvailableTitles"
				});
				AddCheckOutEvent(speechPart, browseViewModel);
				int valueOrDefault = (RentalShoppingCartService?.CurrentRentalShoppingCart?.Count).GetValueOrDefault();
				speechPart.Texts.Add(new TextPart
				{
					Text = $"You have {valueOrDefault} {GetCartTitlePlurality()} in your cart."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press Enter to Checkout."
				});
				AddStartOverOrHomeEventAndText(speechPart, browseViewModel);
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartGeneralInfo(ISpeechControl speechControl, Func<BrowseViewModel> getBrowseViewModelFunction)
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
			BrowseViewModel browseViewModel = getBrowseViewModelFunction();
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
				string text = (BrowseFunctions.IsRedboxPlusBrowseMode ? "Redbox plus movies" : null);
				List<ITextPart> texts = speechPart.Texts;
				TextPart textPart = new TextPart();
				string[] obj = new string[5] { "You are now browsing ", text, " by ", null, null };
				IBrowseState browseState = BrowseState;
				obj[3] = ((browseState != null) ? browseState.BrowseSelectedSort.GetDisplayName() : null);
				obj[4] = " ";
				textPart.Text = string.Concat(obj);
				texts.Add(textPart);
				if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.ForSale && BrowseState.BrowsePriceRangeFilter != null)
				{
					string text2 = "filtered by items with prices";
					BrowsePriceRangeParameter browsePriceRangeFilter = BrowseState.BrowsePriceRangeFilter;
					if (!browsePriceRangeFilter.PriceRangeLow.HasValue)
					{
						text2 = text2 + " of $" + browsePriceRangeFilter.PriceRangeHigh.Value.ToString("0.00") + " and under.";
					}
					else if (!browsePriceRangeFilter.PriceRangeHigh.HasValue)
					{
						text2 = text2 + " of $" + browsePriceRangeFilter.PriceRangeLow.Value.ToString("0.00") + " plus.";
					}
					else
					{
						text2 = text2 + " between $" + browsePriceRangeFilter.PriceRangeLow.Value.ToString("0.00");
						text2 = text2 + " and $" + browsePriceRangeFilter.PriceRangeHigh.Value.ToString("0.00");
					}
					speechPart.Texts.Add(new TextPart
					{
						Text = text2
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = $"There are a total of {(browseViewModel?.BrowseProductControlModel?.BrowseItems?.Count).GetValueOrDefault()} titles available with up to {(browseViewModel?.BrowseProductControlModel?.ItemsPerPage).GetValueOrDefault()} titles per screen."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Press cancel to go back to list of options."
				});
				AddStartOverOrHomeEventAndText(speechPart, browseViewModel);
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartAvailableTitles(ISpeechControl speechControl, Func<BrowseViewModel> getBrowseViewModelFunction)
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
			BrowseViewModel browseViewModel = getBrowseViewModelFunction();
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
				int valueOrDefault = (BrowseState?.TTSBrowseItemModelsShown?.Count((IBrowseItemModel x) => x is DisplayProductModel)).GetValueOrDefault();
				string text = (BrowseFunctions.IsRedboxPlusBrowseMode ? "Redbox plus" : null);
				speechPart.Texts.Add(new TextPart
				{
					Text = $"You are currently browsing on page {(browseViewModel?.BrowseProductControlModel?.PageNumber).GetValueOrDefault()} of {(browseViewModel?.BrowseProductControlModel?.TotalPages).GetValueOrDefault()} pages and there are {valueOrDefault} {text} titles on this screen."
				});
				if ((browseViewModel?.BrowseProductControlModel?.PageNumber).GetValueOrDefault() > 1)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = $"To hear the previous {(browseViewModel?.BrowseProductControlModel?.ItemsPerPage).GetValueOrDefault()} titles, press the minus key."
					});
				}
				if (BrowseState?.TTSBrowseItemModelsShown != null)
				{
					foreach (IBrowseItemModel eachTTSBrowseItemModel in BrowseState.TTSBrowseItemModelsShown)
					{
						if (eachTTSBrowseItemModel?.Data is ITitleProduct titleProduct)
						{
							string text2 = (BrowseState.TTSBrowseItemModelsShown.IndexOf(eachTTSBrowseItemModel) + 1).ToString();
							if (!BrowseFunctions.IsRedboxPlusBrowseMode)
							{
								speechPart.Events.Add(new MapKeyPressToAction
								{
									KeyCode = text2,
									Action = delegate
									{
										browseViewModel.BrowseProductControlModel.ProcessBrowseItemModelSelected(eachTTSBrowseItemModel, null);
									}
								});
								speechPart.Texts.Add(new TextPart
								{
									Text = $"For {titleProduct.ProductName}, Press {text2} for more information."
								});
							}
							if (eachTTSBrowseItemModel.CanAdd)
							{
								speechPart.Events.Add(new MapKeyPressToAction
								{
									KeyCode = $"PLUS{text2}",
									Text = $"you added {titleProduct.ProductName} to your cart.",
									Action = delegate
									{
										browseViewModel.BrowseProductControlModel.ProcessAddBrowseItemModel(eachTTSBrowseItemModel, null);
										CommandManager.InvalidateRequerySuggested();
									}
								});
								string text3 = (BrowseFunctions.IsRedboxPlusBrowseMode ? ("For " + titleProduct.ProductName + ", ") : null);
								speechPart.Texts.Add(new TextPart
								{
									Text = text3 + " Press PLUS and " + text2 + " to add to cart."
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
				AddStartOverOrHomeEventAndText(speechPart, browseViewModel);
				if ((RentalShoppingCartService?.CurrentRentalShoppingCart?.Count).GetValueOrDefault() > 0)
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

	private static void AddSpeechPartMainMenu(ISpeechControl speechControl, Func<BrowseViewModel> getBrowseViewModelFunction)
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
			BrowseViewModel browseViewModel = getBrowseViewModelFunction();
			if (browseViewModel != null)
			{
				AddNextAudioSnippetEvent(speechPart);
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "1",
					Function = "TTSSkipToSpeechPart",
					Value = "GeneralInfo"
				});
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "2",
					Function = "TTSSkipToSpeechPart",
					Value = "GotoAlphaCharacter"
				});
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "3",
					Function = "TTSSkipToSpeechPart",
					Value = "FilterItems"
				});
				if (browseViewModel.SwitchProductFamilyButton1Visibility == Visibility.Visible)
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "4",
						Text = "Switching",
						Action = delegate
						{
							browseViewModel?.ProcessOnToggleTitleFamilyButton1Clicked();
						}
					});
				}
				AddCheckOutEvent(speechPart, browseViewModel);
				speechPart.Texts.Add(new TextPart
				{
					Text = string.Format("To browse all {0}, press 1.", BrowseFunctions.IsRedboxPlusBrowseMode ? ("Redbox plus " + BrowsingFamily) : BrowsingFamily)
				});
				if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "To browse all starting at a specific letter of the alphabet press 2."
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "To filter your selection choices press 3."
				});
				if (browseViewModel.SwitchProductFamilyButton1Visibility == Visibility.Visible)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = string.Format("To select {0} press 4.", (BrowseState.TitleFamily == TitleFamily.Movies) ? "Games" : "Movies")
					});
				}
				if (browseViewModel.SignInButtonVisibility == Visibility.Visible)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "To sign in press 5."
					});
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "5",
						Action = delegate
						{
							browseViewModel?.ProcessOnSignInButtonClicked();
						}
					});
				}
				if (browseViewModel.SignOutButtonVisibility == Visibility.Visible || browseViewModel.ADARedboxPlusSignOutGridVisibility == Visibility.Visible)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "To sign out press 5."
					});
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "5",
						Action = delegate
						{
							browseViewModel?.ProcessOnSignOutButtonClicked();
						}
					});
				}
				if (browseViewModel.MyPerksButtonVisibility == Visibility.Visible || browseViewModel.MyPerksRedboxPlusButtonVisibility == Visibility.Visible)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "To access my Perks press 6."
					});
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "6",
						Action = delegate
						{
							browseViewModel?.ProcessOnMyPerksButtonClicked();
						}
					});
				}
				if (browseViewModel.ViewRedboxPlusMoviesButtonVisibility == Visibility.Visible)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "To browse Redbox plus movies press 7."
					});
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "7",
						Action = delegate
						{
							browseViewModel?.ViewRedboxPlusMoviesCommand?.Execute(null);
						}
					});
				}
				if (browseViewModel.BackToAllMoviesButtonVisibility == Visibility.Visible)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "To go back to all movies press 8."
					});
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "8",
						Action = delegate
						{
							browseViewModel?.BackToAllMoviesCommand?.Execute(null);
						}
					});
				}
				AddStartOverOrHomeEventAndText(speechPart, browseViewModel);
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartFilterSelection(ISpeechControl speechControl, Func<BrowseViewModel> getBrowseViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 7,
			Name = "FilterItems",
			Loop = true,
			StartPause = 500,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			BrowseViewModel browseViewModel = getBrowseViewModelFunction();
			if (browseViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "TAB",
					Function = "TTSSkipToSpeechPart",
					Value = "GeneralInfo"
				});
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "CANCEL",
					Function = "TTSSkipToSpeechPart",
					Value = "AvailableTitles"
				});
				int num = 1;
				BrowseViewModel browseViewModel2 = browseViewModel;
				if (browseViewModel2 != null && browseViewModel2.BrowseFilterBarModel?.FormatFilterButtonVisibility == Visibility.Visible)
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "1",
						Action = delegate
						{
							browseViewModel?.BrowseFilterBarModel.ProcessOnFormatFilterButtonClicked();
							CommandManager.InvalidateRequerySuggested();
						}
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "To filter by format press 1."
					});
					num++;
				}
				BrowseViewModel browseViewModel3 = browseViewModel;
				if (browseViewModel3 != null && browseViewModel3.BrowseFilterBarModel?.PriceRangeFilterButtonVisibility == Visibility.Visible)
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = "2",
						Action = delegate
						{
							browseViewModel?.BrowseFilterBarModel.ProcessOnPriceRangeFilterButtonClicked();
							CommandManager.InvalidateRequerySuggested();
						}
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "To filter by price range press 2."
					});
					num++;
				}
				if (browseViewModel?.BrowseFilterBarModel?.Filters != null)
				{
					foreach (BrowseFilter eachFilter in browseViewModel.BrowseFilterBarModel.Filters.Where((BrowseFilter f) => f.IsVisible))
					{
						string text = (browseViewModel.BrowseFilterBarModel.Filters.IndexOf(eachFilter) + num).ToString();
						speechPart.Events.Add(new MapKeyPressToAction
						{
							KeyCode = text,
							Action = delegate
							{
								browseViewModel?.BrowseFilterBarModel.ProcessOnBrowseFilterButtonClicked(eachFilter);
								CommandManager.InvalidateRequerySuggested();
							},
							Function = "TTSSkipToSpeechPart",
							Value = "GeneralInfo"
						});
						speechPart.Texts.Add(new TextPart
						{
							Text = $"To filter by {eachFilter.Text} press {text}."
						});
					}
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "To cancel press CANCEL."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartGotoAlphaCharacter(ISpeechControl speechControl, Func<BrowseViewModel> getBrowseViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 8,
			Name = "GotoAlphaCharacter",
			Loop = true,
			StartPause = 500,
			EndPause = 250
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			BrowseViewModel browseViewModel = getBrowseViewModelFunction();
			if (browseViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "TAB",
					Function = "TTSSkipToSpeechPart",
					Value = "GeneralInfo"
				});
				speechPart.Events.Add(new MapKeyPress
				{
					KeyCode = "CANCEL",
					Function = "TTSSkipToSpeechPart",
					Value = "AvailableTitles"
				});
				if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
				{
					AddAlphaCharacterEvent(speechPart, browseViewModel, "0", "#");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "1", "#");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "2", "#");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "22", "A");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "222", "B");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "2222", "C");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "3", "#");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "33", "D");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "333", "E");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "3333", "F");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "4", "#");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "44", "G");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "444", "H");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "4444", "I");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "5", "#");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "55", "J");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "555", "K");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "5555", "L");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "6", "#");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "66", "M");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "666", "N");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "6666", "O");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "7", "#");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "77", "P");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "777", "Q");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "7777", "R");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "77777", "S");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "8", "#");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "88", "T");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "888", "U");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "8888", "V");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "9", "#");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "99", "W");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "999", "X");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "9999", "Y");
					AddAlphaCharacterEvent(speechPart, browseViewModel, "99999", "Z");
					speechPart.Texts.Add(new TextPart
					{
						Text = "Choose a letter using the numeric keypad."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing any number once will set the page to titles starting with a number."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing the number 2 twice, will set the page to titles starting with the letter A."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "Pressing the number 2 three times, will set the page to titles starting with the letter B."
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "and so on."
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

	private static void AddSpeechPartPostAlphaSelectionPause(ISpeechControl speechControl, Func<BrowseViewModel> getBrowseViewModelFunction)
	{
		ISpeechPart item = new SpeechPart
		{
			Sequence = 11,
			Name = "PostAlphaSelectionPause",
			Loop = false,
			StartPause = 250,
			EndPause = 250
		};
		speechControl.SpeechParts.Add(item);
	}

	private static void AddSpeechPartAlphaSelectionEmpty(ISpeechControl speechControl, Func<BrowseViewModel> getBrowseViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 12,
			Name = "AlphaSelectionEmpty",
			Loop = false,
			StartPause = 500,
			EndPause = 2000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			if (SelectedAlphaGroupEmptyMessage != null)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = SelectedAlphaGroupEmptyMessage
				});
			}
			speechPart.RegularExpression = new RegularExpression
			{
				Success = "AvailableTitles",
				Failure = "AvailableTitles"
			};
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartGotoAvailableTitles(ISpeechControl speechControl, Func<BrowseViewModel> getBrowseViewModelFunction)
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

	private static void AddAlphaCharacterEvent(ISpeechPart speechPart, BrowseViewModel browseViewModel, string keyCode, string alphaCharacter)
	{
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = keyCode,
			Action = delegate
			{
				SelectedAlphaGroupEmptyMessage = null;
				DisplayGroupSelectionItemModel displayGroupSelectionItemModel = browseViewModel?.BrowseGroupSelection?.GroupSelectionItems.FirstOrDefault((DisplayGroupSelectionItemModel x) => x.Name == alphaCharacter);
				if (displayGroupSelectionItemModel != null)
				{
					browseViewModel?.BrowseGroupSelection?.OnGroupSelectionItemClicked(displayGroupSelectionItemModel);
					if (!displayGroupSelectionItemModel.IsEnabled)
					{
						SelectedAlphaGroupEmptyMessage = $"There are no titles starting with the letter {displayGroupSelectionItemModel?.Name}.";
					}
					CommandManager.InvalidateRequerySuggested();
				}
			},
			Text = $"You have selected '{alphaCharacter}'",
			Function = "TTSSkipToSpeechPart",
			Value = "PostAlphaSelectionPause"
		});
	}

	private static void AddStartOverOrHomeEventAndText(ISpeechPart speechPart, BrowseViewModel browseViewModel)
	{
		if (browseViewModel.BrowseFilterBarModel.HomeButtonVisibility == Visibility.Visible)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "0",
				Action = delegate
				{
					browseViewModel?.BrowseFilterBarModel?.ProcessOnHomeButtonClicked();
				}
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "Press 0 to quit."
			});
		}
		if (browseViewModel.BrowseFilterBarModel.StartOverButtonVisibility == Visibility.Visible)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "0",
				Action = delegate
				{
					browseViewModel?.BrowseFilterBarModel?.ProcessOnStartOverButtonClicked();
				}
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "Press 0 to quit."
			});
		}
	}

	private static void AddCheckOutEvent(ISpeechPart speechPart, BrowseViewModel browseViewModel)
	{
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "ENTER",
			Action = delegate
			{
				browseViewModel?.ProcessOnCheckOutButtonClicked();
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
