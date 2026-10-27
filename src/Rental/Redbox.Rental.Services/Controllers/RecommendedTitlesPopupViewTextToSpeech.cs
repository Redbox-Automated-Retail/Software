using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class RecommendedTitlesPopupViewTextToSpeech
{
	private class TTSConstants
	{
		public const string Instructions = "Instructions";
	}

	public static ISpeechControl GetSpeechControl(Func<RecommendedTitlesPopupViewModel> getRecommendedTitlesPopupViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_recommended_titles_popup_view";
		AddSpeechPartInstructions(result, getRecommendedTitlesPopupViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartInstructions(ISpeechControl speechControl, Func<RecommendedTitlesPopupViewModel> getRecommendedTitlesPopupViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "Instructions",
			Sequence = 1,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 500,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			RecommendedTitlesPopupViewModel recommendedTitlesPopupViewModel = getRecommendedTitlesPopupViewModelFunction();
			if (recommendedTitlesPopupViewModel != null && recommendedTitlesPopupViewModel.BrowseProductControlModel != null)
			{
				if (recommendedTitlesPopupViewModel.PerksInfoBorderVisibility == Visibility.Visible)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = recommendedTitlesPopupViewModel.PerksHeadingText
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = recommendedTitlesPopupViewModel.PerksMessageText
					});
				}
				if (recommendedTitlesPopupViewModel.BrowseProductControlModel.PageNumber == 1)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = recommendedTitlesPopupViewModel.HeaderText
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = $"You are currently browsing page {recommendedTitlesPopupViewModel.BrowseProductControlModel.PageNumber} of {recommendedTitlesPopupViewModel.BrowseProductControlModel.TotalPages}. There are {recommendedTitlesPopupViewModel.BrowseProductControlModel.ItemsOnCurrentPage} titles on this screen."
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "MINUS",
					Function = "TTSRepeatSpeechPart",
					Action = delegate
					{
						if (recommendedTitlesPopupViewModel.BrowseProductControlModel.PageNumber > 1)
						{
							recommendedTitlesPopupViewModel.BrowseProductControlModel.SetPageNumber(recommendedTitlesPopupViewModel.BrowseProductControlModel.PageNumber - 1, PageChangeSource.PaddleButton);
						}
					}
				});
				if (recommendedTitlesPopupViewModel.BrowseProductControlModel.PageNumber > 1)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "To hear the previous titles, press the MINUS key."
					});
				}
				int firstVisibleItemIndex = (recommendedTitlesPopupViewModel.BrowseProductControlModel.PageNumber - 1) * recommendedTitlesPopupViewModel.BrowseProductControlModel.ItemsPerPage;
				int lastVisibleItemIndex = firstVisibleItemIndex + (recommendedTitlesPopupViewModel.BrowseProductControlModel.ItemsOnCurrentPage - 1);
				List<IBrowseItemModel> list = recommendedTitlesPopupViewModel.BrowseProductControlModel.BrowseItems?.Where((IBrowseItemModel item, int index) => index >= firstVisibleItemIndex && index <= lastVisibleItemIndex).ToList();
				if (list != null && list.Count > 0)
				{
					foreach (IBrowseItemModel eachVisibleBrowseItemModel in list)
					{
						if (eachVisibleBrowseItemModel?.Data is ITitleProduct titleProduct)
						{
							string text = (list.IndexOf(eachVisibleBrowseItemModel) + 1).ToString();
							speechPart.Texts.Add(new TextPart
							{
								Text = titleProduct.ProductName
							});
							speechPart.Events.Add(new MapKeyPressToAction
							{
								KeyCode = text,
								Action = delegate
								{
									recommendedTitlesPopupViewModel.BrowseProductControlModel.ProcessBrowseItemModelSelected(eachVisibleBrowseItemModel, null);
								}
							});
							speechPart.Texts.Add(new TextPart
							{
								Text = $"Press {text} for more information."
							});
							speechPart.Events.Add(new MapKeyPressToAction
							{
								KeyCode = $"PLUS{text}",
								Action = delegate
								{
									recommendedTitlesPopupViewModel.BrowseProductControlModel.ProcessAddBrowseItemModel(eachVisibleBrowseItemModel, null);
								}
							});
							speechPart.Texts.Add(new TextPart
							{
								Text = $"Press PLUS and {text} to add to cart."
							});
						}
					}
				}
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "PLUS",
					Function = "TTSRepeatSpeechPart",
					Action = delegate
					{
						if (recommendedTitlesPopupViewModel.BrowseProductControlModel.PageNumber < recommendedTitlesPopupViewModel.BrowseProductControlModel.TotalPages)
						{
							recommendedTitlesPopupViewModel.BrowseProductControlModel.SetPageNumber(recommendedTitlesPopupViewModel.BrowseProductControlModel.PageNumber + 1, PageChangeSource.PaddleButton);
						}
					}
				});
				if (recommendedTitlesPopupViewModel.BrowseProductControlModel.PageNumber < recommendedTitlesPopupViewModel.BrowseProductControlModel.TotalPages)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "To hear the next titles, press the PLUS key."
					});
				}
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Function = "TTSSkipToSpeechPart",
					Action = delegate
					{
						recommendedTitlesPopupViewModel.ProcessOnCancelButtonClicked();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To CANCEL, press CANCEL."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
