using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services;

public class TitleDetailViewTextToSpeech
{
	private static class TTSConstants
	{
		internal static string Intro = "Intro";

		internal static string Buttons = "Buttons";
	}

	public static ISpeechControl GetSpeechControl(Func<TitleDetailModel> getModel)
	{
		SpeechControl speechControl = new SpeechControl
		{
			Name = "tts_product_detail_view",
			SpeechParts =
			{
				IntroPart(getModel),
				ButtonsPart(getModel)
			}
		};
		int i = 0;
		speechControl.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.Sequence = i++;
		});
		return speechControl;
	}

	public static ISpeechPart IntroPart(Func<TitleDetailModel> getModel)
	{
		SpeechPart part = new SpeechPart
		{
			Name = TTSConstants.Intro,
			Loop = false,
			StartPause = 250,
			EndPause = 1500
		};
		part.Refresh = delegate
		{
			part.Clear();
			TitleDetailModel model = getModel();
			IBrowseItemModel browseItem = GetBrowseItem(getModel);
			DisplayProductModel displayProductModel = browseItem as DisplayProductModel;
			ITitleProduct titleProduct = browseItem?.Data as ITitleProduct;
			// Empty cases have no cast, genre or rating to read out.
			bool hasDetails = titleProduct != null && !titleProduct.IsEmptyCase;
			part.Events.AddRange(SharedEvents(model, browseItem));
			part.Events.Add(new MapKeyPress
			{
				KeyCode = "TAB",
				Function = "TTSNextSpeechPart"
			});
			part.Texts.Add(new TextPart
			{
				Text = "You have selected to hear more about " + browseItem?.Name
			});
			if (displayProductModel.OutOfStockVisibility == Visibility.Visible)
			{
				part.Texts.Add(new TextPart
				{
					Text = "This title is currently out of stock."
				});
			}
			if (!string.IsNullOrWhiteSpace(displayProductModel.Description))
			{
				part.Texts.Add(new TextPart
				{
					Text = "The synopsis is as follows: " + displayProductModel.Description
				});
			}
			if (hasDetails && displayProductModel.StarringVisibility == Visibility.Visible)
			{
				part.Texts.Add(new TextPart
				{
					Text = "Starring: " + displayProductModel.Starring
				});
			}
			if (hasDetails && !string.IsNullOrWhiteSpace(displayProductModel.Genres))
			{
				part.Texts.Add(new TextPart
				{
					Text = "Category: " + displayProductModel.Genres
				});
			}
			if (hasDetails && displayProductModel.RatingVisibility == Visibility.Visible)
			{
				part.Texts.Add(new TextPart
				{
					Text = displayProductModel.RatingLabel + " " + displayProductModel.RatingDescription
				});
			}
			part.Texts.Add(new TextPart
			{
				Text = "1-Night Rental: " + displayProductModel.OneNightPrice
			});
			part.Texts.Add(new TextPart
			{
				Text = "Return: " + displayProductModel.OneNightReturnTime
			});
			if (titleProduct != null && titleProduct.HasMultiNightPrice)
			{
				part.Texts.Add(new TextPart
				{
					Text = "Multi-Night Rental: " + displayProductModel.MultiNightPrice
				});
				part.Texts.Add(new TextPart
				{
					Text = "Return: " + displayProductModel.MultiNightReturnTime
				});
			}
			if (displayProductModel.ServiceFeeTextVisibility == Visibility.Visible)
			{
				part.Texts.Add(new TextPart
				{
					Text = "Service Fee: " + displayProductModel.ServiceFeeText
				});
			}
			if (titleProduct != null && titleProduct.HasDeal)
			{
				part.Texts.Add(new TextPart
				{
					Text = displayProductModel.DealText
				});
			}
		};
		return part;
	}

	public static ISpeechPart ButtonsPart(Func<TitleDetailModel> getModel)
	{
		SpeechPart part = new SpeechPart
		{
			Name = TTSConstants.Buttons,
			Loop = true,
			StartPause = 250,
			EndPause = 5000
		};
		part.Refresh = delegate
		{
			part.Clear();
			TitleDetailModel model = getModel();
			IBrowseItemModel item = GetBrowseItem(getModel);
			DisplayProductModel displayProductModel = item as DisplayProductModel;
			ITitleProduct primaryProduct = item.Data as ITitleProduct;
			// "Movies" -> "Movie", "Games" -> "Game".
			string familyName = primaryProduct.TitleFamily.ToString().Replace("s", "");
			part.Events.AddRange(SharedEvents(model, item));
			part.Events.Add(new MapKeyPress
			{
				KeyCode = "1",
				Function = "TTSSkipToSpeechPart",
				Value = TTSConstants.Intro
			});
			part.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "ENTER",
				Action = delegate
				{
					if (primaryProduct.ComingSoon)
					{
						model.ProcessMoreInfoHit(item);
					}
					else
					{
						model.ProcessAddProduct(item, null, primaryProduct.TitleType);
					}
				}
			});
			part.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "2",
				Action = delegate
				{
					model.ProcessBuyButtonHit(item, null);
				}
			});
			if (displayProductModel.ButtonData.Any((TilteDetailRentalFormatButtonData b) => b.Format == primaryProduct.TitleType && b.IsEnabled))
			{
				part.Texts.Add(new TextPart
				{
					Text = $"To add the {TitleTypeProperties.Properties[primaryProduct.TitleType].GetName(Resources.ResourceManager)} title {item.Name} to your cart, press Enter."
				});
			}
			if (displayProductModel.ShowBuy == Visibility.Visible)
			{
				part.Texts.Add(new TextPart
				{
					Text = $"To buy this {familyName} for ${primaryProduct.PurchasePrice}, press 2"
				});
			}
			if (displayProductModel.OutOfStockVisibility == Visibility.Visible)
			{
				part.Texts.Add(new TextPart
				{
					Text = displayProductModel.OutOfStockLine1Text + " " + displayProductModel.OutOfStockLine2Text + " " + displayProductModel.OutOfStockLine3Text + " " + displayProductModel.OutOfStockLine4Text
				});
			}
			if (displayProductModel.ComingSoonVisibility == Visibility.Visible)
			{
				part.Texts.Add(new TextPart
				{
					Text = "This title is coming soon. To set a reminder email, press ENTER."
				});
			}
			part.Texts.Add(new TextPart
			{
				Text = string.Format("To hear the description again, press 1. To hear the previous {0}'s description, press MINUS. To hear the next {0}'s description, press PLUS. To go back to browsing {0} titles, press CANCEL.", familyName)
			});
		};
		return part;
	}

	private static IEnumerable<ISpeechPartEvent> SharedEvents(TitleDetailModel model, IBrowseItemModel item)
	{
		List<ISpeechPartEvent> events = new List<ISpeechPartEvent>();
		if ((model?.BrowseProductControlModel?.PageNumber).GetValueOrDefault() < (model?.BrowseProductControlModel?.TotalPages).GetValueOrDefault())
		{
			events.Add(new MapKeyPressToAction
			{
				KeyCode = "PLUS",
				Function = "TTSSkipToSpeechPart",
				Value = TTSConstants.Intro,
				Action = delegate
				{
					model.BrowseProductControlModel.ProcessPageChanged(model.BrowseProductControlModel.PageNumber, model.BrowseProductControlModel.PageNumber + 1, PageChangeSource.PaddleButton);
				}
			});
		}
		if ((model?.BrowseProductControlModel?.PageNumber).GetValueOrDefault() > 1)
		{
			events.Add(new MapKeyPressToAction
			{
				KeyCode = "MINUS",
				Function = "TTSSkipToSpeechPart",
				Value = TTSConstants.Intro,
				Action = delegate
				{
					model.BrowseProductControlModel.ProcessPageChanged(model.BrowseProductControlModel.PageNumber, model.BrowseProductControlModel.PageNumber - 1, PageChangeSource.PaddleButton);
				}
			});
		}
		if (model != null)
		{
			events.Add(new MapKeyPressToAction
			{
				KeyCode = "CANCEL",
				Action = model.ProcessGoBackHit
			});
		}
		return events;
	}

	private static IBrowseItemModel GetBrowseItem(Func<TitleDetailModel> getModel)
	{
		TitleDetailModel model = getModel?.Invoke();
		bool isAda = ApplicationState.Instance.IsADAAccessible;
		ITitleProduct selectedProduct = (ITitleProduct)Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
		// In ADA mode a selected title rollup reads out its default format.
		return model?.BrowseProductControlModel?.BrowseItems?.FirstOrDefault((IBrowseItemModel x) => (isAda && selectedProduct is TitleRollupProduct selectedRollup) ? TitleRollupUtilities.IsAdaDefaultTitle((ITitleProduct)x.Data, selectedRollup) : (((ITitleProduct)x.Data)?.ProductId == selectedProduct?.ProductId));
	}
}
