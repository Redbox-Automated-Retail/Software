using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class MultiDiscVendViewController : BaseViewController
{
	public class Constants
	{
		public const string MultiDiscVendViewTimer = "multi_disc_vend_view_timer";

		public const string Intro = "Intro";
	}

	private static object _lock = new object();

	private static Action _continueAction;

	public static void ConfigureView(Action continueAction)
	{
		_continueAction = continueAction;
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.SetCurrentUICulture();
			MultiDiscVendViewModel multiDiscVendViewModel = new MultiDiscVendViewModel();
			multiDiscVendViewModel.OkayButtonAction += OkayButtonClickedAction;
			PopulateModelText(multiDiscVendViewModel);
			ConfigureTTS(multiDiscVendViewModel);
			viewUserControl.DataContext = multiDiscVendViewModel;
			ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			int value = ((service != null && service.TTSEnabled) ? 35000 : 15000);
			(ServiceLocator.Instance.GetService<ITimerService>()?.CreateTimer("multi_disc_vend_view_timer", value, value, delegate
			{
				LogHelper.Instance.Log("multi_disc_vend_view_timer executed");
				ExecuteCallback(fromOkayButton: false);
			}))?.Start();
		}
	}

	private static void OkayButtonClickedAction()
	{
		ExecuteCallback(fromOkayButton: true);
	}

	private static void ExecuteCallback(bool fromOkayButton)
	{
		MultiDiscVendViewModel multiDiscVendViewModel = BaseViewController.GetViewModel<MultiDiscVendViewModel>();
		if (multiDiscVendViewModel == null)
		{
			return;
		}
		LockHelper.TryLockObject(_lock, delegate
		{
			if (!multiDiscVendViewModel.CallbackExecuted)
			{
				multiDiscVendViewModel.CallbackExecuted = true;
				if (fromOkayButton)
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("MultiDiscVendOkay");
				}
				_continueAction?.Invoke();
			}
			else
			{
				LogHelper.Instance.Log("ExecuteCallback was already executed.");
			}
		}, "ExecuteCallback.Execute");
	}

	private static void PopulateModelText(MultiDiscVendViewModel multiDiscVendViewModel)
	{
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		int num = 0;
		List<string> list = new List<string>();
		int num2 = 0;
		if (rentalShoppingCart != null)
		{
			foreach (IRentalShoppingCartTitleItem item in rentalShoppingCart.TitleItems.Where((IRentalShoppingCartTitleItem x) => x.MultiDisc && x.DiscNumber == 1))
			{
				if (item?.TitleProduct != null)
				{
					num2++;
					string productDisplayName = BrowseFunctions.GetProductDisplayName(item.TitleProduct);
					TitleTypeProperty value = null;
					TitleTypeProperties.Properties.TryGetValue(item.TitleProduct.TitleType, out value);
					string text = " " + value?.GetName(Resources.ResourceManager);
					MDVProduct mDVProduct = item.TitleProduct as MDVProduct;
					string text2 = $" ({mDVProduct?.TitleProducts?.Count ?? 1} {Resources.multi_disc_vend_discs_text})";
					list.Add(productDisplayName + " " + text + " " + text2);
					num = mDVProduct?.TitleProducts?.Count ?? 2;
				}
			}
		}
		if (num2 == 1)
		{
			multiDiscVendViewModel.NoteBodyText = Resources.multi_disc_vend_note_body_single_mdv;
			multiDiscVendViewModel.GrabDiscsText = Resources.multi_disc_vend_grab_discs_text_single_mdv;
		}
		else
		{
			multiDiscVendViewModel.NoteBodyText = Resources.multi_disc_vend_note_body_multiple_mdv;
			multiDiscVendViewModel.GrabDiscsText = Resources.multi_disc_vend_grab_discs_text_multiple_mdv;
		}
		multiDiscVendViewModel.HeaderText = Resources.multi_disc_vend_header;
		multiDiscVendViewModel.ProductListText = list.Join(", ");
		multiDiscVendViewModel.NoteHeaderText = Resources.multi_disc_vend_note_header;
		multiDiscVendViewModel.DiscVendText = Resources.disc_vend;
		multiDiscVendViewModel.NumberOfDiscsText = num.ToString();
		multiDiscVendViewModel.OkayButtonText = Resources.multi_disc_vend_okay;
	}

	private static void ConfigureTTS(MultiDiscVendViewModel multiDiscVendViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled || multiDiscVendViewModel == null)
		{
			return;
		}
		multiDiscVendViewModel.OnGetSpeechControl = delegate
		{
			SpeechControl obj = new SpeechControl
			{
				Name = "tts_multi_disc_vend_view"
			};
			SpeechPart part = new SpeechPart
			{
				Name = "Intro",
				Loop = true,
				StartPause = 250,
				EndPause = 10000
			};
			part.Refresh = delegate
			{
				part.Clear();
				part.Texts.Add(new TextPart
				{
					Text = multiDiscVendViewModel?.HeaderText
				});
				part.Texts.Add(new TextPart
				{
					Text = multiDiscVendViewModel?.ProductListText
				});
				part.Texts.Add(new TextPart
				{
					Text = multiDiscVendViewModel?.GrabDiscsText
				});
				part.Texts.Add(new TextPart
				{
					Text = multiDiscVendViewModel?.NoteHeaderText
				});
				part.Texts.Add(new TextPart
				{
					Text = multiDiscVendViewModel?.NoteBodyText
				});
				part.Texts.Add(new TextPart
				{
					Text = "To continue press ENTER"
				});
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						multiDiscVendViewModel?.ProcessOkayButtonClick();
					}
				});
			};
			obj.SpeechParts.Add(part);
			int i = 0;
			obj.SpeechParts.ForEach(delegate(ISpeechPart x)
			{
				x.Sequence = i++;
			});
			return obj;
		};
	}

	public static void LeaveView()
	{
		OrchestrationService.RemoveTimer("multi_disc_vend_view_timer");
	}
}
