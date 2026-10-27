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
		SpeechControl obj = new SpeechControl
		{
			Name = "tts_product_detail_view",
			SpeechParts = 
			{
				IntroPart(getModel),
				ButtonsPart(getModel)
			}
		};
		int i = 0;
		obj.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.Sequence = i++;
		});
		return obj;
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
			ITitleProduct obj = browseItem?.Data as ITitleProduct;
			part.Events.AddRange(SharedEvents(model, browseItem));
			part.Events.Add(new MapKeyPress
			{
				KeyCode = "TAB",
				Function = "TTSNextSpeechPart"
			});
			TextPart item = new TextPart
			{
				Text = "You have selected to hear more about " + browseItem?.Name
			};
			part.Texts.Add(item);
			if (displayProductModel.OutOfStockVisibility == Visibility.Visible)
			{
				TextPart item2 = new TextPart
				{
					Text = "This title is currently out of stock."
				};
				part.Texts.Add(item2);
			}
			if (!string.IsNullOrWhiteSpace(displayProductModel.Description))
			{
				TextPart item3 = new TextPart
				{
					Text = "The synopsis is as follows: " + displayProductModel.Description
				};
				part.Texts.Add(item3);
			}
			if (obj != null && !obj.IsEmptyCase && displayProductModel.StarringVisibility == Visibility.Visible)
			{
				TextPart item4 = new TextPart
				{
					Text = "Starring: " + ((DisplayProductModel)browseItem).Starring
				};
				part.Texts.Add(item4);
			}
			if (obj != null && !obj.IsEmptyCase && !string.IsNullOrWhiteSpace(displayProductModel.Genres))
			{
				TextPart item5 = new TextPart
				{
					Text = "Category: " + displayProductModel.Genres
				};
				part.Texts.Add(item5);
			}
			if (obj != null && !obj.IsEmptyCase && displayProductModel.RatingVisibility == Visibility.Visible)
			{
				TextPart item6 = new TextPart
				{
					Text = ((DisplayProductModel)browseItem).RatingLabel + " " + ((DisplayProductModel)browseItem).RatingDescription
				};
				part.Texts.Add(item6);
			}
			TextPart item7 = new TextPart
			{
				Text = "1-Night Rental: " + ((DisplayProductModel)browseItem).OneNightPrice
			};
			part.Texts.Add(item7);
			TextPart item8 = new TextPart
			{
				Text = "Return: " + ((DisplayProductModel)browseItem).OneNightReturnTime
			};
			part.Texts.Add(item8);
			if (obj != null && obj.HasMultiNightPrice)
			{
				TextPart item9 = new TextPart
				{
					Text = "Multi-Night Rental: " + ((DisplayProductModel)browseItem).MultiNightPrice
				};
				part.Texts.Add(item9);
			}
			if (obj != null && obj.HasMultiNightPrice)
			{
				TextPart item10 = new TextPart
				{
					Text = "Return: " + ((DisplayProductModel)browseItem).MultiNightReturnTime
				};
				part.Texts.Add(item10);
			}
			if (browseItem is DisplayProductModel)
			{
				DisplayProductModel obj2 = (DisplayProductModel)browseItem;
				if (obj2 != null && obj2.ServiceFeeTextVisibility == Visibility.Visible)
				{
					part.Texts.Add(new TextPart
					{
						Text = "Service Fee: " + ((DisplayProductModel)browseItem)?.ServiceFeeText
					});
				}
			}
			if (obj != null && obj.HasDeal)
			{
				TextPart item11 = new TextPart
				{
					Text = ((DisplayProductModel)browseItem).DealText
				};
				part.Texts.Add(item11);
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
			string arg = primaryProduct.TitleFamily.ToString().Replace("s", "");
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
				TextPart item2 = new TextPart
				{
					Text = $"To add the {TitleTypeProperties.Properties[primaryProduct.TitleType].GetName(Resources.ResourceManager)} title {item.Name} to your cart, press Enter."
				};
				part.Texts.Add(item2);
			}
			if (displayProductModel.ShowBuy == Visibility.Visible)
			{
				TextPart item3 = new TextPart
				{
					Text = $"To buy this {arg} for ${primaryProduct.PurchasePrice}, press 2"
				};
				part.Texts.Add(item3);
			}
			if (displayProductModel.OutOfStockVisibility == Visibility.Visible)
			{
				TextPart item4 = new TextPart
				{
					Text = displayProductModel.OutOfStockLine1Text + " " + displayProductModel.OutOfStockLine2Text + " " + displayProductModel.OutOfStockLine3Text + " " + displayProductModel.OutOfStockLine4Text
				};
				part.Texts.Add(item4);
			}
			if (displayProductModel.ComingSoonVisibility == Visibility.Visible)
			{
				TextPart item5 = new TextPart
				{
					Text = "This title is coming soon. To set a reminder email, press ENTER."
				};
				part.Texts.Add(item5);
			}
			TextPart item6 = new TextPart
			{
				Text = string.Format("To hear the description again, press 1. To hear the previous {0}'s description, press MINUS. To hear the next {0}'s description, press PLUS. To go back to browsing {0} titles, press CANCEL.", arg)
			};
			part.Texts.Add(item6);
		};
		return part;
	}

	private static IEnumerable<ISpeechPartEvent> SharedEvents(TitleDetailModel model, IBrowseItemModel item)
	{
		List<ISpeechPartEvent> list = new List<ISpeechPartEvent>();
		if ((model?.BrowseProductControlModel?.PageNumber).GetValueOrDefault() < (model?.BrowseProductControlModel?.TotalPages).GetValueOrDefault())
		{
			list.Add(new MapKeyPressToAction
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
			list.Add(new MapKeyPressToAction
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
			list.Add(new MapKeyPressToAction
			{
				KeyCode = "CANCEL",
				Action = model.ProcessGoBackHit
			});
		}
		return list;
	}

	private static IBrowseItemModel GetBrowseItem(Func<TitleDetailModel> getModel)
	{
		TitleDetailModel obj = getModel?.Invoke();
		bool isAda = ApplicationState.Instance.IsADAAccessible;
		ITitleProduct selectedProduct = (ITitleProduct)Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
		return obj?.BrowseProductControlModel?.BrowseItems?.FirstOrDefault((IBrowseItemModel x) => (!isAda || !(selectedProduct is TitleRollupProduct)) ? (((ITitleProduct)x.Data)?.ProductId == selectedProduct?.ProductId) : TitleRollupUtilities.IsAdaDefaultTitle((ITitleProduct)x.Data, (TitleRollupProduct)selectedProduct));
	}
}
