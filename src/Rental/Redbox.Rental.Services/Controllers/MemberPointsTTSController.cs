using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class MemberPointsTTSController : BaseTTSController<MemberPointsTTSController, MemberPointsLogic>
{
	private class TTSConstants
	{
		public const string PointsMenu = "PointsMenu";
	}

	private static Action EmptyAction = delegate
	{
	};

	public static void AddSpeechParts(ISpeechControl speechControl, MemberPointsLogic model)
	{
		BaseTTSController<MemberPointsTTSController, MemberPointsLogic>.StopIdleTimer();
		AddSpeechPartForPoints(speechControl, model);
	}

	private static string GetRentals(TierType memberTier, string rentals)
	{
		if (memberTier != TierType.Legend)
		{
			return $"You have {rentals}";
		}
		return rentals;
	}

	private static void AddBaseEvents(ISpeechPart speechPart, MemberPointsLogic logic)
	{
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "CANCEL",
			Action = delegate
			{
				logic.CancelButtonCommand_Execute();
			}
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "ENTER",
			Action = delegate
			{
				logic.ContinueButtonCommand_Execute();
			}
		});
	}

	private static Func<string> CreateSelectedCartItemAction(int cartIndex, DisplayProductModel displayModel)
	{
		return () => (!IsCartItem4K(displayModel)) ? $"you have selected item {cartIndex} title, {displayModel.Name} to redeem." : $"you cannot redeem 4 K titles. Item {cartIndex} title, {displayModel.Name}, cannot be redeemed.";
	}

	private static Func<string> CreateSelectedCartItemsAction(MemberPointsLogic logic)
	{
		return delegate
		{
			string text = "";
			string text2 = "";
			List<DisplayProductModel> selectedCartItems = logic.GetSelectedCartItems();
			foreach (DisplayProductModel item in selectedCartItems)
			{
				text2 = text2 + text + item.Name;
				if (text == "")
				{
					text = ", ";
				}
			}
			return (selectedCartItems.Count == 0) ? "There are no selected items" : $"You have selected {selectedCartItems.Count} items as follows: {text2}";
		};
	}

	private static Func<string> CreateUnselectedCartItemsAction(MemberPointsLogic logic)
	{
		return delegate
		{
			string text = "";
			string text2 = "";
			List<DisplayProductModel> unselectedCartItems = logic.GetUnselectedCartItems();
			foreach (DisplayProductModel item in unselectedCartItems)
			{
				text2 = text2 + text + item.Name;
				if (text == "")
				{
					text = ", ";
				}
			}
			return (unselectedCartItems.Count == 0) ? "All items have been selected." : $"The {unselectedCartItems.Count} items you have not selected are as follows: {text2}";
		};
	}

	private static TitleType GetTitleType(DisplayProductModel displayModel)
	{
		return (displayModel.Data as IRentalShoppingCartTitleItem).TitleType;
	}

	private static bool IsCartItem4K(DisplayProductModel displayModel)
	{
		if (displayModel != null)
		{
			return GetTitleType(displayModel) == TitleType._4KUHD;
		}
		return false;
	}

	private static Action CreateUpdatePointsAction(int index, MemberPointsLogic logic, bool bAdd, DisplayProductModel displayModel)
	{
		return delegate
		{
			if (!IsCartItem4K(displayModel))
			{
				logic.UpdatePointsInfo(index, bAdd);
			}
		};
	}

	private static void AddSpeechPartForPoints(ISpeechControl speechControl, MemberPointsLogic logic)
	{
		MemberPointsModel model = logic.GetModel();
		List<DisplayProductModel> displayProductModels = model.DisplayProductModels;
		int CartCount = ((displayProductModels != null) ? displayProductModels.Count : 0);
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "PointsMenu",
			StartPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = "Welcome to My Points. Your picks are on point."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To exit My Points, and go back to the previous screen, press CANCEL."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = model.PointsMessage
			});
			if (CartCount > 0)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "To redeem Points, select an item in your cart by, pressing PLUS, then the item number."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To cancel a selection, pressing MINUS, then the item number."
				});
			}
			speechPart.Texts.Add(new TextPart
			{
				Text = string.Format("Your cart has {0} title{1}.", CartCount, (CartCount != 1) ? "s" : "")
			});
			int num = 0;
			foreach (DisplayProductModel item in displayProductModels)
			{
				num++;
				speechPart.Texts.Add(new TextPart
				{
					Text = $"To redeem title {item.Name}, press PLUS and number {num}."
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = $"PLUS{num}",
					Action = CreateUpdatePointsAction(num - 1, logic, bAdd: true, item),
					ActionText = CreateSelectedCartItemAction(num, item)
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = $"MINUS{num}",
					Action = CreateUpdatePointsAction(num - 1, logic, bAdd: false, item),
					Text = (IsCartItem4K(item) ? $"Item {num} title, {item.Name} has not been selected." : $"Item {num} title, {item.Name} is no longer selected.")
				});
			}
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "8",
				Action = EmptyAction,
				ActionText = CreateSelectedCartItemsAction(logic)
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "9",
				Action = EmptyAction,
				ActionText = CreateUnselectedCartItemsAction(logic)
			});
			AddBaseEvents(speechPart, logic);
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
