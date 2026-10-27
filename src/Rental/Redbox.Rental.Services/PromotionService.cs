using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Campaign;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Services;

public class PromotionService : IPromotionService
{
	public class Constants
	{
		public const string TemplateName = "template_name";

		public const string IncludedTitles = "INCLUDED_TITLES";

		public const string ExcludedTitles = "EXCLUDED_TITLES";

		public const string DiscountAmount = "DISCOUNT_AMOUNT";

		public const string PromoCode = "PROMO_CODE";

		public const string Store = "store";

		public const string Code = "code";

		public const string Response = "response";

		public const string Amount = "amount";

		public const string PromotionIntent = "promotion_intent";

		public const string RentQuantity = "rent_qty";

		public const string RentFormat = "rent_format";

		public const string GetQuantity = "get_qty";

		public const string GetFormat = "get_format";

		public const string CampaignTitles = "campaign_titles";

		public const string IsOnline = "is_online";

		public const string IsStandAlone = "stand_alone";

		public const string ServerName = "server_name";

		public const string ProductTypeId = "product_type_id";

		public const string FormatIds = "format_ids";

		public const string ActionCode = "action_code";

		public const string AllowFullDiscount = "allow_full_discount";

		public const string DefaultPromo = "default_promo";

		public const string GameRequired = "GameRequired";

		public const string Default = "in_cart_promotion_v2";

		public const string DefaultNoOffer = "DEFAULT NO OFFER";

		public const string RentOneGetFixedOff = "ROGO MOVIE A";

		public const string FirstNightFree = "FREE NIGHT M";
	}

	private static IPromotionService _instance;

	private Action<IPromoCodeValidationResult> _validatePromoCodeResultCallback;

	private int m_working;

	private IPromotion _promotion;

	private List<Dictionary<string, object>> _inCartPromoRules;

	private InCartOfferTemplate _inCartOfferTemplateDefault = new InCartOfferTemplate
	{
		Name = "in_cart_promotion_v2",
		ProgramName = "Default-No-Offer",
		VariablesUsed = null,
		Content = new List<Dictionary<string, object>>
		{
			new Dictionary<string, object>
			{
				{ "offer_condition", "cart" },
				{ "min_qty", "1001" },
				{ "max_qty", "1000" },
				{ "ask_only_once", "True" }
			}
		}
	};

	private InCartOfferTemplate _inCartOfferTemplateA = new InCartOfferTemplate
	{
		Name = "ROGO MOVIE A",
		ProgramName = "In-Cart Template A",
		VariablesUsed = "EXCLUDED_TITLES,DISCOUNT_AMOUNT,PROMO_CODE",
		Content = new List<Dictionary<string, object>>
		{
			new Dictionary<string, object>
			{
				{ "offer_condition", "cart" },
				{ "item_action", "rental" },
				{ "add_sequence", "pre_offer" },
				{ "min_qty", "1" },
				{ "ask_only_once", "True" }
			},
			new Dictionary<string, object>
			{
				{ "offer_condition", "cart" },
				{ "add_sequence", "pre_offer" },
				{ "max_qty", "4" }
			},
			new Dictionary<string, object>
			{
				{ "offer_action", "browse_filter" },
				{ "product_family", "Movies" },
				{ "exclude_id_list", "{{EXCLUDED_TITLES}}" }
			},
			new Dictionary<string, object>
			{
				{ "satisfy_condition", "product" },
				{ "item_action", "rental" },
				{ "add_sequence", "pre_offer" },
				{ "min_qty", "1" }
			},
			new Dictionary<string, object>
			{
				{ "satisfy_condition", "product" },
				{ "item_action", "rental" },
				{ "add_sequence", "post_offer" },
				{ "product_family", "Movies" },
				{ "exclude_id_list", "{{EXCLUDED_TITLES}}" },
				{ "min_qty", "1" }
			},
			new Dictionary<string, object>
			{
				{ "satisfy_condition", "cart" },
				{ "min_qty", "2" }
			},
			new Dictionary<string, object>
			{
				{ "redemption_rule", "fixed_amount" },
				{ "redemption_amount", "{{DISCOUNT_AMOUNT}}" },
				{ "max_qty", "1" },
				{ "add_sequence", "post_offer" },
				{ "item_action", "rental" },
				{ "exclude_id_list", "{{EXCLUDED_TITLES}}" },
				{ "redemption_code", "{{PROMO_CODE}}" },
				{ "display_code", "False" },
				{ "allow_user_entry", "False" }
			},
			new Dictionary<string, object>
			{
				{ "offer_language", "en-US" },
				{ "InCartPromoHeaderMessage", "Add a Movie for ${{DISCOUNT_AMOUNT}} Off Today!" },
				{ "InCartPromoBodyMessage", "Lucky you! Add a select DVD or Blu-ray Disc™ to your cart, and you’ll get it for ${{DISCOUNT_AMOUNT}} off for the first day.\n\n(Additional daily rental charges apply, plus tax where applicable. Questions? Check out the Redbox terms and conditions.)\n\nReady to pick out another movie?" },
				{ "InCartPromoNoButton", "NO THANKS" },
				{ "InCartPromoYesButton", "YES" }
			},
			new Dictionary<string, object>
			{
				{ "offer_language", "es-MX" },
				{ "InCartPromoHeaderMessage", "¡Añade otro con Descuento de ${{DISCOUNT_AMOUNT}}!" },
				{ "InCartPromoBodyMessage", "Añade un DVD o un disco Blu-ray al carrito de la selección ofrecida, y aquilelo con descuento de ${{DISCOUNT_AMOUNT}} al primer día.\n\n(Se cargaría su tarjeta para cada día adicional, más los impuestos donde se aplican. ¿Preguntas? Vea a los Términos y Condiciones de Uso.)\n\n¿Listo seleccionar otra película?" },
				{ "InCartPromoNoButton", "NO GRACIAS" },
				{ "InCartPromoYesButton", "CONTINUAR" }
			}
		}
	};

	private InCartOfferTemplate _inCartOfferTemplateM = new InCartOfferTemplate
	{
		Name = "FREE NIGHT M",
		ProgramName = "In-Cart Template M",
		VariablesUsed = "EXCLUDED_TITLES,PROMO_CODE",
		Content = new List<Dictionary<string, object>>
		{
			new Dictionary<string, object>
			{
				{ "offer_condition", "cart" },
				{ "item_action", "rental" },
				{ "add_sequence", "pre_offer" },
				{ "min_qty", "1" },
				{ "ask_only_once", "True" }
			},
			new Dictionary<string, object>
			{
				{ "offer_condition", "cart" },
				{ "add_sequence", "pre_offer" },
				{ "max_qty", "4" }
			},
			new Dictionary<string, object>
			{
				{ "offer_action", "browse_filter" },
				{ "product_family", "Movies" },
				{ "exclude_id_list", "{{EXCLUDED_TITLES}}" }
			},
			new Dictionary<string, object>
			{
				{ "satisfy_condition", "product" },
				{ "item_action", "rental" },
				{ "add_sequence", "pre_offer" },
				{ "min_qty", "1" }
			},
			new Dictionary<string, object>
			{
				{ "satisfy_condition", "product" },
				{ "item_action", "rental" },
				{ "add_sequence", "post_offer" },
				{ "product_family", "Movies" },
				{ "exclude_id_list", "{{EXCLUDED_TITLES}}" },
				{ "min_qty", "1" }
			},
			new Dictionary<string, object>
			{
				{ "satisfy_condition", "cart" },
				{ "min_qty", "2" }
			},
			new Dictionary<string, object>
			{
				{ "redemption_rule", "post_offer_first_night" },
				{ "redemption_amount", "initial_night" },
				{ "max_qty", "1" },
				{ "add_sequence", "post_offer" },
				{ "item_action", "rental" },
				{ "exclude_id_list", "{{EXCLUDED_TITLES}}" },
				{ "redemption_code", "{{PROMO_CODE}}" },
				{ "display_code", "False" },
				{ "allow_user_entry", "False" }
			},
			new Dictionary<string, object>
			{
				{ "offer_language", "en-US" },
				{ "InCartPromoHeaderMessage", "Add a Movie for FREE!" },
				{ "InCartPromoBodyMessage", "Lucky you! Add a select DVD or Blu-ray to your bag for a FREE 1-night rental.\n\n(Charges may apply for additional nights plus tax where applicable.)\n\nReady to pick out another movie?" },
				{ "InCartPromoNoButton", "NO THANKS" },
				{ "InCartPromoYesButton", "YES" }
			},
			new Dictionary<string, object>
			{
				{ "offer_language", "es-MX" },
				{ "InCartPromoHeaderMessage", "¡Añade Película Gratis Hoy!" },
				{ "InCartPromoBodyMessage", "Añade selecto película al carrito, y aquilelo gratis el primer día.\n\n(Se cargaría su tarjeta para cada día adicional, más los impuestos donde se aplican. ¿Preguntas? Vea a los Términos y Condiciones de Uso de Redbox.)\n\n¿Listo seleccionar otra película?" },
				{ "InCartPromoNoButton", "NO GRACIAS" },
				{ "InCartPromoYesButton", "CONTINUAR" }
			}
		}
	};

	public static IPromotionService Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new PromotionService();
			}
			return _instance;
		}
	}

	public bool IsFreeInCartOfferRunning
	{
		get
		{
			if (!(ApplicationState.Instance.InCartTemplateName == "FREE DVD I") && !(ApplicationState.Instance.InCartTemplateName == "FREE BLU-RAY J"))
			{
				return ApplicationState.Instance.InCartTemplateName == "FREE GAME K";
			}
			return true;
		}
	}

	public bool NeedToShowFreeInCartMessage
	{
		get
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			if (GetPromotion() == null && IsFreeInCartOfferRunning)
			{
				IPromoCodeValidationInfo promoCodeValidationInfo = ValidateFreeInCartOffer();
				if (promoCodeValidationInfo != null && promoCodeValidationInfo.IsCartValid)
				{
					return service?.CurrentRentalShoppingCart?.InCartPromoOfferMade != true;
				}
			}
			return false;
		}
	}

	public bool IsRentTwoInCartOfferRunning => ApplicationState.Instance.InCartTemplateName == "RTWO MONEY OFF L";

	public bool NeedToShowRentTwoInCartMessage
	{
		get
		{
			int valueOrDefault = (Instance.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count()).GetValueOrDefault();
			bool flag = valueOrDefault == 1 && !(RentalShoppingCart?.InCartPromoOfferMade ?? false);
			bool flag2 = valueOrDefault > 1;
			if (IsRentTwoInCartOfferRunning)
			{
				IRentalShoppingCart rentalShoppingCart = RentalShoppingCart;
				if ((rentalShoppingCart == null || !rentalShoppingCart.InCartDoNotShow) && GetPromotion() == null)
				{
					return flag | flag2;
				}
			}
			return false;
		}
	}

	public bool IsExcludeEmptyCaseInCartOfferRunning
	{
		get
		{
			if (!(ApplicationState.Instance.InCartTemplateName == "ROGO MOVIE A"))
			{
				return ApplicationState.Instance.InCartTemplateName == "FREE NIGHT M";
			}
			return true;
		}
	}

	public bool IsExclude4KInCartOfferRunning
	{
		get
		{
			if (!(ApplicationState.Instance.InCartTemplateName == "ROGO MOVIE A"))
			{
				return ApplicationState.Instance.InCartTemplateName == "FREE NIGHT M";
			}
			return true;
		}
	}

	public bool IsExcludeDiscountRestrictionInCartOfferRunning => ApplicationState.Instance.InCartTemplateName == "FREE NIGHT M";

	public bool IsFirstNightFreeItemInCartOfferRunning => ApplicationState.Instance.InCartTemplateName == "FREE NIGHT M";

	public bool NeedToShowReturnVisitIneligibleCartWarning
	{
		get
		{
			bool valueOrDefault = RentalShoppingCart?.Promotion?.IsReturnVisit == true;
			IRentalShoppingCart rentalShoppingCart = RentalShoppingCart;
			bool flag = rentalShoppingCart != null && rentalShoppingCart.Promotion?.Intent == PromotionIntentCode.FirstNightFreeItem;
			bool flag2 = (RentalShoppingCart?.Totals?.DiscountAmountUsed).GetValueOrDefault() > 0m;
			bool flag3 = BrowseState?.ReturnVisitIneligibleCartWarningShown ?? false;
			if ((valueOrDefault & flag) && !flag2)
			{
				return !flag3;
			}
			return false;
		}
	}

	private IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	private IRentalShoppingCart RentalShoppingCart => RentalShoppingCartService?.CurrentRentalShoppingCart;

	private IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance?.BrowseState;

	public IPromotion GetPromotion()
	{
		return _promotion;
	}

	public void SetPromotion(IPromotion promotion)
	{
		_promotion = promotion;
		IPersonalizationService service = ServiceLocator.Instance.GetService<IPersonalizationService>();
		if (promotion != null)
		{
			service?.SetSelectedPromo(promotion.Name);
		}
		else
		{
			service?.ClearSelectedPromo();
		}
	}

	public void ClearPromotion()
	{
		_promotion = null;
		ServiceLocator.Instance.GetService<IPersonalizationService>()?.ClearSelectedPromo();
	}

	public void ValidatePromoCodeOnServer(string promoCode, Action<IPromoCodeValidationResult> promoCodeValidationResultCallback)
	{
		_validatePromoCodeResultCallback = promoCodeValidationResultCallback;
		RemoteServiceCallback completeCallback = ProcessPromoCodeValidationResult;
		ValidatePromoCodeInKioskClientService((ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0).ToString(), promoCode, completeCallback);
	}

	private void ValidatePromoCodeInKioskClientService(string storeNumber, string promotionCode, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(async delegate
		{
			RemoteServiceResult result = new RemoteServiceResult();
			result.Properties["is_online"] = true;
			result.Properties["store"] = storeNumber;
			result.Properties["code"] = promotionCode;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				string kioskClientServiceBaseUrl = ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl;
				if (!ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone)
				{
					LogHelper.Instance.Log("Live call to remote IPromotionServices.ValidatePromotionCode, URL: " + kioskClientServiceBaseUrl);
					if (!int.TryParse(storeNumber, out var result2))
					{
						LogHelper.Instance.Log("The passed storeNumber '" + storeNumber + "' cannot be converted into an Int32.", LogEntryType.Error);
						result.Success = false;
						result.Properties["response"] = PromotionValidationResponse.Error.ToString();
					}
					else
					{
						ServiceLocator.Instance.GetService<IMacroService>();
						Guid messageId = Guid.NewGuid();
						IDictionary<string, object> dictionary = await ServiceLocator.Instance.GetService<IKioskClientService>().Promotion.ValidatePromotionCode(messageId, promotionCode, result2);
						result.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						result.Properties["response"] = PromotionValidationResponse.Valid.ToString();
						result.Properties["amount"] = (dictionary.ContainsKey("Amount") ? dictionary["Amount"] : ((object)1));
						result.Properties["promotion_intent"] = (dictionary.ContainsKey("PromotionIntentCode") ? dictionary["PromotionIntentCode"] : PromotionIntentCode.FixedValue.ToString());
						result.Properties["rent_qty"] = (dictionary.ContainsKey("RentQty") ? dictionary["RentQty"] : ((object)0));
						result.Properties["rent_format"] = (dictionary.ContainsKey("RentFormat") ? dictionary["RentFormat"] : null);
						result.Properties["get_qty"] = (dictionary.ContainsKey("GetQty") ? dictionary["GetQty"] : ((object)0));
						result.Properties["get_format"] = (dictionary.ContainsKey("GetFormat") ? dictionary["GetFormat"] : null);
						Dictionary<string, object> dictionary2 = (dictionary.ContainsKey("CampaignTitles") ? (dictionary["CampaignTitles"] as Dictionary<string, object>) : null);
						if (dictionary2 != null)
						{
							ArrayList value = dictionary2["Titles"] as ArrayList;
							Dictionary<string, object> value2 = new Dictionary<string, object>
							{
								["include"] = (bool)dictionary2["Include"],
								["titles"] = value
							};
							result.Properties["campaign_titles"] = value2;
						}
						if (dictionary.ContainsKey("ServerName"))
						{
							result.Properties["server_name"] = dictionary["ServerName"];
						}
						result.Properties["product_type_id"] = (dictionary.ContainsKey("ProductTypeId") ? dictionary["ProductTypeId"] : null);
						result.Properties["format_ids"] = (dictionary.ContainsKey("FormatIds") ? (dictionary["FormatIds"] as ArrayList) : null);
						result.Properties["action_code"] = (dictionary.ContainsKey("ActionType") ? dictionary["ActionType"] : PromotionActionCode.Rental.ToString());
						result.Properties["allow_full_discount"] = (dictionary.ContainsKey("AllowFullDiscount") ? dictionary["AllowFullDiscount"] : ((object)true));
						result.Properties["default_promo"] = (dictionary.ContainsKey("DefaultPromo") ? dictionary["DefaultPromo"] : null);
						if (!result.Success)
						{
							ArrayList arrayList = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
							if (arrayList != null && arrayList.Count > 0)
							{
								if (arrayList[0] is Dictionary<string, object> dictionary3)
								{
									object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(PromotionValidationResponse?), (string)dictionary3["Code"]);
									result.Properties["response"] = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : PromotionValidationResponse.Error.ToString());
								}
							}
							else
							{
								result.Properties["response"] = PromotionValidationResponse.Error.ToString();
							}
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, not issuing realtime call to ValidatePromotionCode.", LogEntryType.Info);
					result.Properties["response"] = PromotionValidationResponse.Error.ToString();
					result.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PromotionServicesProxy.ValidatePromotionCode.", e);
				result.Properties["response"] = PromotionValidationResponse.Error.ToString();
			}
			finally
			{
				try
				{
					result.ExecutionTime = executionTimer.Elapsed;
					result.Properties["stand_alone"] = ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", result?.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = result,
						Message = "Invoking completeCallback from PromotionServicesProxy.ValidatePromotionCode."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void ProcessPromoCodeValidationResult(IRemoteServiceResult result)
	{
		IPromoCodeValidationResult promoCodeValidationResult = new PromoCodeValidationResult
		{
			Success = (result?.Success ?? false)
		};
		if (result != null)
		{
			try
			{
				promoCodeValidationResult.IsStandAlone = result.Properties.ContainsKey("stand_alone") && (bool)result.Properties["stand_alone"];
				promoCodeValidationResult.IsOnline = result.Properties.ContainsKey("is_online") && (bool)result.Properties["is_online"];
				promoCodeValidationResult.ServerName = ((!result.Properties.ContainsKey("server_name")) ? null : result.Properties["server_name"]?.ToString());
				promoCodeValidationResult.StoreNumber = ((!result.Properties.ContainsKey("store")) ? null : result.Properties["store"]?.ToString());
				promoCodeValidationResult.PromoCode = ((!result.Properties.ContainsKey("code")) ? null : result.Properties["code"]?.ToString());
				promoCodeValidationResult.Response = ((!result.Properties.ContainsKey("response")) ? null : result.Properties["response"]?.ToString());
				if (result.Properties.ContainsKey("get_qty") && int.TryParse(result.Properties["get_qty"]?.ToString(), out var result2))
				{
					promoCodeValidationResult.GetQuantity = result2;
				}
				if (result.Properties.ContainsKey("rent_qty") && int.TryParse(result.Properties["rent_qty"]?.ToString(), out var result3))
				{
					promoCodeValidationResult.RentQuantity = result3;
				}
				if (result.Properties.ContainsKey("amount") && decimal.TryParse(result.Properties["amount"]?.ToString(), out var result4))
				{
					promoCodeValidationResult.Amount = result4;
				}
				if (result.Properties.ContainsKey("get_format") && Enum.TryParse<PromotionGetFormat>(result.Properties["get_format"]?.ToString(), out var result5))
				{
					promoCodeValidationResult.GetFormat = result5;
				}
				if (result.Properties.ContainsKey("rent_format") && Enum.TryParse<PromotionRentFormat>(result.Properties["rent_format"]?.ToString(), out var result6))
				{
					promoCodeValidationResult.RentFormat = result6;
				}
				if (result.Properties.ContainsKey("promotion_intent") && Enum.TryParse<PromotionIntentCode>(result.Properties["promotion_intent"]?.ToString(), out var result7))
				{
					promoCodeValidationResult.IntentCode = result7;
				}
				if (result.Properties.ContainsKey("campaign_titles") && result.Properties["campaign_titles"] is Dictionary<string, object> dictionary)
				{
					promoCodeValidationResult.CampaignTitles = new CampaignTitles
					{
						Titles = new List<long>()
					};
					if (dictionary.ContainsKey("include") && bool.TryParse(dictionary["include"]?.ToString(), out var result8))
					{
						promoCodeValidationResult.CampaignTitles.Include = result8;
					}
					if (dictionary.ContainsKey("titles") && dictionary["titles"] is ArrayList arrayList)
					{
						foreach (object item in arrayList)
						{
							if (long.TryParse(item?.ToString(), out var result9))
							{
								promoCodeValidationResult.CampaignTitles.Titles.Add(result9);
							}
						}
					}
				}
				if (result.Properties.ContainsKey("product_type_id") && int.TryParse(result.Properties["product_type_id"]?.ToString(), out var result10))
				{
					promoCodeValidationResult.ProductTypeId = result10;
				}
				if (result.Properties.ContainsKey("format_ids") && result.Properties["format_ids"] is ArrayList arrayList2)
				{
					promoCodeValidationResult.FormatIds = new List<int>();
					foreach (object item2 in arrayList2)
					{
						if (int.TryParse(item2?.ToString(), out var result11))
						{
							promoCodeValidationResult.FormatIds.Add(result11);
						}
					}
				}
				if (result.Properties.ContainsKey("action_code") && Enum.TryParse<PromotionActionCode>(result.Properties["action_code"]?.ToString(), out var result12))
				{
					promoCodeValidationResult.ActionCode = result12;
				}
				else
				{
					promoCodeValidationResult.ActionCode = PromotionActionCode.Rental;
				}
				promoCodeValidationResult.AllowFullDiscount = !result.Properties.ContainsKey("allow_full_discount") || (bool)result.Properties["allow_full_discount"];
				promoCodeValidationResult.DefaultPromo = (result.Properties.ContainsKey("default_promo") ? ((IDefaultPromo)result.Properties["default_promo"]) : null);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Error processing promotion code validation result. {0}", e);
				promoCodeValidationResult.Response = PromotionValidationResponse.Error.ToString();
			}
		}
		if (_validatePromoCodeResultCallback != null)
		{
			_validatePromoCodeResultCallback(promoCodeValidationResult);
		}
	}

	public InCartOfferMessageText GetInCartOfferMessageText()
	{
		InCartOfferMessageText inCartOfferMessageText = new InCartOfferMessageText();
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
				if (promotionRule.OfferLanguage == OrchestrationService.GetCulture() && (!IsRentTwoInCartOfferRunning || (Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count() == 1 && promotionRule.OfferMessageVersion == "1") || ((Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count()).GetValueOrDefault() > 1 && promotionRule.OfferMessageVersion == "2")))
				{
					inCartOfferMessageText.Header = promotionRule.InCartPromoHeaderMessage;
					inCartOfferMessageText.Message = promotionRule.InCartPromoBodyMessage;
					IPricingRecord pricingRecord = ServiceLocator.Instance.GetService<IPricingService>()?.GetProductTypePricingRecord(TitleFamily.Movies, TitleType.DVD, useAdjustedPrices: false);
					if (pricingRecord != null && !string.IsNullOrEmpty(promotionRule.InCartPromoHeaderMessage))
					{
						inCartOfferMessageText.Message = string.Format(promotionRule.InCartPromoBodyMessage, $"{pricingRecord.ExtraNight:f2}");
					}
					inCartOfferMessageText.YesButtonText = promotionRule.InCartPromoYesButton;
					inCartOfferMessageText.NoButtonText = promotionRule.InCartPromoNoButton;
				}
			}
		}
		return inCartOfferMessageText;
	}

	public bool CartQualifiesForInCartOffer(IRentalShoppingCart rentalShoppingCart)
	{
		bool flag = false;
		if (rentalShoppingCart != null)
		{
			LogHelper.Instance.Log("CartQualifiesForInCartOffer BEGIN");
			if (_inCartPromoRules != null)
			{
				if (!IsFreeInCartOfferRunning && !IsRentTwoInCartOfferRunning)
				{
					if (rentalShoppingCart.Promotion == null)
					{
						if (!rentalShoppingCart.HasRedboxPlusFreeOneNightRentalSelected)
						{
							flag = true;
							bool flag2 = false;
							int num = 0;
							foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
							{
								num++;
								PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
								if (!string.IsNullOrEmpty(promotionRule.OfferCondition))
								{
									int num2 = 0;
									if (promotionRule.OfferCondition == PromotionRule.Constants.Product)
									{
										num2 = rentalShoppingCart.GetTitleItemsByCriteria(promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes, null, null, includeMultiNightPricedItems: true, includeFlashDeals: true, includeLoyaltyRedeemedItems: true, null, includeDiscountRestrictionItems: true)?.Count ?? 0;
									}
									else if (promotionRule.OfferCondition == PromotionRule.Constants.Cart)
									{
										num2 = rentalShoppingCart.GetTitleItemsByCriteria(promotionRule.Action, promotionRule.Sequence, TitleType.All, TitleFamily.All, null, null, null, null, null, includeMultiNightPricedItems: true, includeFlashDeals: false, includeLoyaltyRedeemedItems: true, null, includeDiscountRestrictionItems: true)?.Count ?? 0;
									}
									else if (promotionRule.OfferCondition == PromotionRule.Constants.BrowseMinimumQuantity)
									{
										num2 = BrowseCountByCriteria(rentalShoppingCart, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts);
									}
									if (num2 < promotionRule.MinimumQuantity)
									{
										LogIfVerboseLoggingIsOn(string.Format(">> {2}, min_qty={0} true_count={1}", promotionRule.MinimumQuantity, num2, promotionRule.OfferCondition));
										LogHelper.Instance.Log("> failed condition {0}", num);
										flag = false;
									}
									if (num2 > promotionRule.MaximumQuantity)
									{
										LogIfVerboseLoggingIsOn(string.Format(">> {2}, max_qty={0} true_count={1}", promotionRule.MaximumQuantity, num2, promotionRule.OfferCondition));
										LogHelper.Instance.Log("> failed condition {0}", num);
										flag = false;
									}
									LogIfVerboseLoggingIsOn($"> offer condition ({num}) {promotionRule.OfferCondition} returns {flag}");
								}
								if (promotionRule.AskOnlyOnce == true)
								{
									flag2 = true;
								}
								if (promotionRule.IncludeProducts == null || promotionRule.IncludeProducts.Count <= 0)
								{
									continue;
								}
								IDataService service = ServiceLocator.Instance.GetService<IDataService>();
								bool flag3 = true;
								if (service != null)
								{
									foreach (long includeProduct in promotionRule.IncludeProducts)
									{
										IKioskProduct kioskProduct = null;
										service.GetProduct(includeProduct, excludeTitleRollup: false, out kioskProduct);
										if (kioskProduct is ITitleProduct { IsRentableAndInStock: not false })
										{
											flag3 = false;
											break;
										}
									}
								}
								if (flag3)
								{
									LogHelper.Instance.Log(">  no titles in the offer are in stock, returning False");
									flag = false;
								}
							}
							if (flag2 && rentalShoppingCart.InCartPromoOfferMade)
							{
								LogHelper.Instance.Log(">  offer already presented, returning False");
								flag = false;
							}
						}
						else
						{
							LogHelper.Instance.Log("customer already using Redbox+ benefit, CartQualifiesForInCartOffer = false");
						}
					}
					else
					{
						LogHelper.Instance.Log("customer already entered a promo code, CartQualifiesForInCartOffer = false");
					}
				}
			}
			else
			{
				LogHelper.Instance.Log("InCartPromoRules == nil at CartQualifiesForInCartOffer");
			}
		}
		return flag;
	}

	private IPromoCodeValidationInfo CartSatisfiesInCartOffer(IRentalShoppingCart rentalShoppingCart)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		if (rentalShoppingCart != null && _inCartPromoRules != null && !IsFreeInCartOfferRunning && !IsRentTwoInCartOfferRunning && rentalShoppingCart.InCartPromoOfferMade)
		{
			promoCodeValidationInfo.IsCartValid = true;
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
				if (!string.IsNullOrEmpty(promotionRule.SatisfyCondition))
				{
					if (promotionRule.SatisfyCondition == PromotionRule.Constants.Product)
					{
						promoCodeValidationInfo.QualifyingItems = rentalShoppingCart.GetTitleItemsByCriteria(promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes, null, null, includeMultiNightPricedItems: true, includeFlashDeals: true, includeLoyaltyRedeemedItems: true, null, includeDiscountRestrictionItems: true);
					}
					else if (promotionRule.SatisfyCondition == PromotionRule.Constants.Cart)
					{
						promoCodeValidationInfo.QualifyingItems = rentalShoppingCart.GetTitleItemsByCriteria(promotionRule.Action, promotionRule.Sequence, TitleType.All, TitleFamily.All, null, null, null, null, null, includeMultiNightPricedItems: true, includeFlashDeals: true, includeLoyaltyRedeemedItems: true, null, includeDiscountRestrictionItems: true);
					}
					if (promoCodeValidationInfo.QualifyingItems.Count() < promotionRule.MinimumQuantity)
					{
						promoCodeValidationInfo.IsCartValid = false;
					}
					if (promoCodeValidationInfo.QualifyingItems.Count() > promotionRule.MaximumQuantity)
					{
						promoCodeValidationInfo.IsCartValid = false;
					}
				}
			}
		}
		return promoCodeValidationInfo;
	}

	private PromotionRule GetRedemptionCodePromotionRule()
	{
		PromotionRule result = null;
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
				if (!string.IsNullOrEmpty(promotionRule.RedemptionCode))
				{
					result = promotionRule;
				}
			}
		}
		return result;
	}

	private void LogIfVerboseLoggingIsOn(string message)
	{
		if (ApplicationState.Instance.VerboseLogging)
		{
			LogHelper.Instance.Log(message);
		}
	}

	public int BrowseCountByCriteria(IRentalShoppingCart rentalShoppingCart, TitleType titleType, TitleFamily titleFamily, List<long> includeProductList, List<long> excludeProductList)
	{
		int num = 0;
		if (includeProductList == null)
		{
			includeProductList = new List<long>();
		}
		if (excludeProductList == null)
		{
			excludeProductList = new List<long>();
		}
		LogHelper.Instance.Log(string.Format("> BrowseCountBy pt: {0}, pf: {1}, include list: {2}, exclude list: {3}", titleType, titleFamily, string.Join(", ", includeProductList), string.Join(", ", excludeProductList)));
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null)
		{
			List<IKioskProduct> products = null;
			service.GetProducts(BrowseViewFilter.All, titleFamily, titleType, BrowseSort.MerchandiseDate, null, null, null, null, isADAView: false, null, null, out products);
			foreach (IKioskProduct item in products)
			{
				ITitleProduct titleProduct = item as ITitleProduct;
				if (titleProduct != null && titleProduct.IsRentableAndInStock && !titleProduct.IsEmptyCase && (!titleProduct.DiscNumber.HasValue || titleProduct.DiscNumber == 1) && (includeProductList.Count == 0 || includeProductList.Contains(titleProduct.ProductId)) && (excludeProductList.Count == 0 || !excludeProductList.Contains(titleProduct.ProductId)) && rentalShoppingCart.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.ProductId == titleProduct.ProductId && x.Action == RentalShoppingCartItemAction.rental) == null)
				{
					num++;
				}
			}
		}
		LogHelper.Instance.Log("> BrowseCountBy result: {0}", num);
		return num;
	}

	public IPromoCodeValidationInfo ValidateRegularInCartPromo(IRentalShoppingCart rentalShoppingCart)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		if (!IsFreeInCartOfferRunning && !IsRentTwoInCartOfferRunning)
		{
			PromotionRule redemptionCodePromotionRule = GetRedemptionCodePromotionRule();
			IPromotion promotion = GetPromotion();
			promoCodeValidationInfo = CartSatisfiesInCartOffer(rentalShoppingCart);
			if (redemptionCodePromotionRule != null && promoCodeValidationInfo.IsCartValid && PromoCodeMatchesInCartRedemptionCode(promotion, redemptionCodePromotionRule))
			{
				if (redemptionCodePromotionRule.RedemptionRule == PromotionRule.Constants.RedemptionRuleProductType)
				{
					ComputeProductTypePromo(rentalShoppingCart, redemptionCodePromotionRule, promoCodeValidationInfo);
				}
				else if (redemptionCodePromotionRule.RedemptionRule == PromotionRule.Constants.RedemptionRuleFixedAmount)
				{
					ComputeFixedAmountPromoPrice(rentalShoppingCart, redemptionCodePromotionRule, promoCodeValidationInfo);
				}
				else if (redemptionCodePromotionRule.RedemptionRule == PromotionRule.Constants.RedemptionRuleQuantityBasedAmount)
				{
					ComputeQuantityBasedAmountPromoPrice(rentalShoppingCart, redemptionCodePromotionRule, promoCodeValidationInfo);
				}
				else if (redemptionCodePromotionRule.RedemptionRule == PromotionRule.Constants.RedemptionRulePostOfferFirstNight)
				{
					ComputePostOfferPromoPrice(rentalShoppingCart, redemptionCodePromotionRule, promoCodeValidationInfo);
				}
			}
		}
		return promoCodeValidationInfo;
	}

	private void ComputePostOfferPromoPrice(IRentalShoppingCart rentalShoppingCart, PromotionRule promotionRule, IPromoCodeValidationInfo result)
	{
		foreach (IRentalShoppingCartTitleItem item in (from x in rentalShoppingCart.GetTitleItemsByCriteria(promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes, null, null, includeMultiNightPricedItems: true, includeFlashDeals: true, includeLoyaltyRedeemedItems: true, null, includeDiscountRestrictionItems: true)
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).Take(promotionRule.MaximumQuantity).ToList())
		{
			result.PromotionItems.Add(new PromotionItem
			{
				RentalShoppingCartItem = item,
				PromotionAmount = item.PricingRecord.InitialNight
			});
		}
		result.Price = result.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
	}

	private void ComputeQuantityBasedAmountPromoPrice(IRentalShoppingCart rentalShoppingCart, PromotionRule promotionRule, IPromoCodeValidationInfo result)
	{
		List<IRentalShoppingCartTitleItem> list = (from x in rentalShoppingCart.GetTitleItemsByCriteria(promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes, null, null, includeMultiNightPricedItems: true, includeFlashDeals: true, includeLoyaltyRedeemedItems: true, null, includeDiscountRestrictionItems: true)
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).ToList();
		int num = list?.Count() ?? 0;
		LogHelper.Instance.Log("qty_based_amount = {0} * {1}", promotionRule.RedemptionAmount, num);
		if (num > promotionRule.MaximumQuantity)
		{
			LogHelper.Instance.Log("> user exceeded the max qty on a qty-based-amount in-cart, resetting discount to max allowed");
			num = promotionRule.MaximumQuantity;
		}
		decimal result2 = 0m;
		decimal.TryParse(promotionRule.RedemptionAmount, out result2);
		foreach (IRentalShoppingCartTitleItem item in list)
		{
			if (result.PromotionItems.Count() < num)
			{
				decimal promotionAmount = Math.Min(result2, (item?.PricingRecord?.InitialNight).GetValueOrDefault());
				result.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = item,
					PromotionAmount = promotionAmount
				});
			}
		}
		result.Price = result.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
	}

	private void ComputeFixedAmountPromoPrice(IRentalShoppingCart rentalShoppingCart, PromotionRule promotionRule, IPromoCodeValidationInfo result)
	{
		if (!decimal.TryParse(promotionRule.RedemptionAmount, out var result2))
		{
			return;
		}
		List<IRentalShoppingCartTitleItem> list = (from x in rentalShoppingCart.GetTitleItemsByCriteria(RentalShoppingCartItemAction.rental, null, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, null, null, null, includeMultiNightPricedItems: false, includeFlashDeals: true, includeLoyaltyRedeemedItems: false, false, includeDiscountRestrictionItems: false)
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).ToList();
		decimal num = result2;
		foreach (IRentalShoppingCartTitleItem item in list)
		{
			if (num > 0m)
			{
				decimal num2 = Math.Min(num, (item?.PricingRecord?.InitialNight).GetValueOrDefault());
				result.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = item,
					PromotionAmount = num2
				});
				num -= num2;
			}
		}
		result.Price = result.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
	}

	private void ComputeProductTypePromo(IRentalShoppingCart rentalShoppingCart, PromotionRule promotionRule, IPromoCodeValidationInfo result)
	{
		if (promotionRule.TitleFamily == TitleFamily.All || (promotionRule.TitleType == TitleType.All && promotionRule.TitleFamily != TitleFamily.Games))
		{
			return;
		}
		IPricingRecord productTypePricingRecord = ServiceLocator.Instance.GetService<IPricingService>().GetProductTypePricingRecord(promotionRule.TitleFamily, promotionRule.TitleType, useAdjustedPrices: true);
		decimal num = 0m;
		if (productTypePricingRecord != null)
		{
			string redemptionAmount = promotionRule.RedemptionAmount;
			if (!(redemptionAmount == "initial_night"))
			{
				if (redemptionAmount == "extra_night")
				{
					num = productTypePricingRecord.ExtraNight;
				}
			}
			else
			{
				num = productTypePricingRecord.InitialNight;
			}
		}
		List<IRentalShoppingCartTitleItem> list = (from x in rentalShoppingCart.GetTitleItemsByCriteria(RentalShoppingCartItemAction.rental, null, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, null, null, null, includeMultiNightPricedItems: false, includeFlashDeals: true, includeLoyaltyRedeemedItems: false, false, includeDiscountRestrictionItems: false)
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).ToList();
		decimal num2 = num;
		foreach (IRentalShoppingCartTitleItem item in list)
		{
			if (num2 > 0m)
			{
				decimal num3 = Math.Min(num2, (item?.PricingRecord?.InitialNight).GetValueOrDefault());
				result.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = item,
					PromotionAmount = num3
				});
				num2 -= num3;
			}
		}
		result.Price = result.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
	}

	private bool PromoCodeMatchesInCartRedemptionCode(IPromotion promotion, PromotionRule promotionRule)
	{
		if (promotion != null)
		{
			return promotion?.Name == promotionRule?.RedemptionCode;
		}
		return true;
	}

	public IPromotionRule GetInCartPromoActionRule()
	{
		IPromotionRule result = null;
		if (!IsFreeInCartOfferRunning && _inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				IPromotionRule promotionRule = new PromotionRule(inCartPromoRule);
				if (promotionRule.OfferAction != null)
				{
					result = promotionRule;
					break;
				}
			}
		}
		return result;
	}

	public string GetInCartPromoCode()
	{
		string result = null;
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
				if (promotionRule.RedemptionCode != null)
				{
					result = promotionRule.RedemptionCode;
					break;
				}
			}
		}
		return result;
	}

	private decimal GetRedemptionAmountFromInCartPromo()
	{
		decimal result = 0m;
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
				if (promotionRule.RedemptionAmount != null)
				{
					decimal result2 = 0m;
					if (decimal.TryParse(promotionRule.RedemptionAmount, out result2))
					{
						result = result2;
						return result;
					}
				}
			}
		}
		return result;
	}

	private TitleFamily GetTitleFamilyFromInCartPromo()
	{
		TitleFamily result = TitleFamily.All;
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				if (inCartPromoRule.Keys.Contains(PromotionRule.Constants.ProductFamily))
				{
					result = new PromotionRule(inCartPromoRule).TitleFamily;
				}
			}
		}
		return result;
	}

	private TitleType GetTitleTypeFromInCartPromo()
	{
		TitleType result = TitleType.All;
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				if (inCartPromoRule.Keys.Contains(PromotionRule.Constants.ProductType))
				{
					result = new PromotionRule(inCartPromoRule).TitleType;
				}
			}
		}
		return result;
	}

	public void SetFreeInCartPromo(IRentalShoppingCart rentalShoppingCart)
	{
		SetPromoToFixedPriceInCart(rentalShoppingCart, 0m);
	}

	public void SetRentTwoInCartPromo(IRentalShoppingCart rentalShoppingCart)
	{
		decimal redemptionAmountFromInCartPromo = GetRedemptionAmountFromInCartPromo();
		if (rentalShoppingCart != null)
		{
			SetPromoToFixedPriceInCart(rentalShoppingCart, redemptionAmountFromInCartPromo);
		}
	}

	public void SetPromoToFixedPriceInCart(IRentalShoppingCart rentalShoppingCart, decimal promoValue)
	{
		string inCartPromoCode = GetInCartPromoCode();
		if (inCartPromoCode != null && rentalShoppingCart != null)
		{
			IPromotion promotion = new Redbox.Rental.Services.Promotion.Promotion
			{
				Name = inCartPromoCode,
				DiscountType = DiscountType.PromotionCode,
				OriginalPrice = promoValue,
				Price = promoValue,
				Quantity = 1,
				Intent = PromotionIntentCode.FixedValue,
				OriginalPercent = 0m,
				Percent = 0m,
				RentQuantity = 0,
				RentFormat = null,
				GetQuantity = 0,
				GetFormat = null,
				CampaignTitles = null,
				ProductTypeId = null,
				FormatIds = null,
				ActionCode = PromotionActionCode.Rental
			};
			SetPromotion(promotion);
		}
	}

	public void SetPromoToFirstNightFreeItemInCart(IRentalShoppingCart rentalShoppingCart)
	{
		string inCartPromoCode = GetInCartPromoCode();
		if (inCartPromoCode != null && rentalShoppingCart != null)
		{
			IPromotion promotion = new Redbox.Rental.Services.Promotion.Promotion
			{
				Name = inCartPromoCode,
				DiscountType = DiscountType.PromotionCode,
				OriginalPrice = 0m,
				Price = 0m,
				Quantity = 1,
				Intent = PromotionIntentCode.FirstNightFreeItem,
				OriginalPercent = 0m,
				Percent = 0m,
				RentQuantity = 0,
				RentFormat = null,
				GetQuantity = 0,
				GetFormat = null,
				CampaignTitles = null,
				ProductTypeId = null,
				FormatIds = null,
				ActionCode = PromotionActionCode.Rental
			};
			SetPromotion(promotion);
		}
	}

	public void SetPromoFromCustomerOffer(IRentalShoppingCart rentalShoppingCart, CustomerOffer offer, IRentalShoppingCartTitleItem applyToLineItem)
	{
		if (rentalShoppingCart != null && offer != null)
		{
			decimal num = CalcInCartOfferPromoAmount(applyToLineItem, offer);
			if (offer.CustomerSegmentType == CustomerSegmentType.BasketStretch && applyToLineItem != null)
			{
				IPromotion promotion = new Redbox.Rental.Services.Promotion.Promotion
				{
					Name = (offer.PromotionCode ?? offer.OfferDiscountCode),
					DiscountType = ((offer.PromotionCode != null) ? DiscountType.PromotionCode : DiscountType.Discount),
					OriginalPrice = num,
					Price = num,
					Quantity = 1,
					Intent = PromotionIntentCode.FixedValue,
					OriginalPercent = 0m,
					Percent = 0m,
					RentQuantity = 0,
					RentFormat = null,
					GetQuantity = 0,
					GetFormat = null,
					CampaignTitles = null,
					ProductTypeId = null,
					FormatIds = null,
					ActionCode = ((offer.ActionCode != ActionCode.Purchase) ? PromotionActionCode.Rental : PromotionActionCode.Purchase)
				};
				promotion.PromotionItems.Add(new PromotionItem
				{
					PromotionAmount = num,
					RentalShoppingCartItem = applyToLineItem
				});
				SetPromotion(promotion);
			}
			else
			{
				PromotionIntentCode intent = ((offer.DiscountType == OfferDiscountType.Free) ? PromotionIntentCode.FirstNightFreeItem : ((offer.DiscountType == OfferDiscountType.MultiNightPrice) ? PromotionIntentCode.MultiNightItem : PromotionIntentCode.FixedValue));
				decimal num2 = ((offer.DiscountType == OfferDiscountType.Free) ? 0m : offer.Amount);
				IPromotion promotion2 = new Redbox.Rental.Services.Promotion.Promotion
				{
					Name = (offer.PromotionCode ?? offer.OfferDiscountCode),
					DiscountType = ((offer.PromotionCode != null) ? DiscountType.PromotionCode : DiscountType.Discount),
					Price = num2,
					OriginalPrice = num2,
					Quantity = 1,
					Intent = intent,
					OriginalPercent = 0m,
					Percent = 0m,
					RentQuantity = 0,
					RentFormat = null,
					GetQuantity = 0,
					GetFormat = null,
					CampaignTitles = new CampaignTitles
					{
						Include = offer.IncludeTitles
					},
					ProductTypeId = (int)offer.TitleFamily.Value,
					FormatIds = (offer.IncludeFormats ? offer.FormatIds : GetFilteredFormatIds(offer.FormatIds)),
					ActionCode = ((offer.ActionCode != ActionCode.Purchase) ? PromotionActionCode.Rental : PromotionActionCode.Purchase)
				};
				SetPromotion(promotion2);
				IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
				service.CurrentRentalShoppingCart.ValidateAndUpdatePromoCode();
				service.CurrentRentalShoppingCart.UpdateServiceFee();
			}
		}
	}

	private List<int> GetFilteredFormatIds(List<int> formatIds)
	{
		List<int> list = new List<int>();
		if (formatIds != null)
		{
			TitleType[] array = (TitleType[])Enum.GetValues(typeof(TitleType));
			foreach (TitleType item in array)
			{
				if (!formatIds.Contains((int)item))
				{
					list.Add((int)item);
				}
			}
		}
		return list;
	}

	public decimal CalcInCartOfferPromoAmount(IRentalShoppingCartTitleItem newLineItem, CustomerOffer offer)
	{
		decimal num = 0m;
		if (newLineItem != null)
		{
			if (offer.DiscountType == OfferDiscountType.AmountOff)
			{
				num = offer.Amount;
			}
			else if (offer.DiscountType == OfferDiscountType.PercentOff)
			{
				num = newLineItem.Price * (offer.Amount / 100m);
			}
			else if (offer.DiscountType == OfferDiscountType.Free)
			{
				num = newLineItem.Price;
			}
			num = Math.Min(num, newLineItem.Price);
		}
		return num;
	}

	public IPromoCodeValidationInfo ValidateFreeInCartOffer()
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		TitleFamily titleFamilyFromInCartPromo = GetTitleFamilyFromInCartPromo();
		TitleType titleTypeFromInCartPromo = GetTitleTypeFromInCartPromo();
		List<long> excludeProducts = null;
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (service.CurrentRentalShoppingCart != null)
		{
			IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = (from x in service.CurrentRentalShoppingCart.GetTitleItemsByCriteria(RentalShoppingCartItemAction.rental, null, titleTypeFromInCartPromo, titleFamilyFromInCartPromo, null, excludeProducts, null, null, null, includeMultiNightPricedItems: false, includeFlashDeals: true, includeLoyaltyRedeemedItems: false, null, includeDiscountRestrictionItems: false)
				orderby (x?.PricingRecord?.InitialNight).GetValueOrDefault(), x.ProductId
				select x).FirstOrDefault();
			promoCodeValidationInfo.IsCartValid = rentalShoppingCartTitleItem != null;
			promoCodeValidationInfo.Price = (rentalShoppingCartTitleItem?.PricingRecord?.InitialNight).GetValueOrDefault();
			if (rentalShoppingCartTitleItem != null)
			{
				promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = rentalShoppingCartTitleItem,
					PromotionAmount = promoCodeValidationInfo.Price
				});
				promoCodeValidationInfo.QualifyingItems.Add(rentalShoppingCartTitleItem);
			}
		}
		return promoCodeValidationInfo;
	}

	public IPromoCodeValidationInfo ValidateRentTwoInCartPromo()
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		List<long> inCartExcludeTitleList = GetInCartExcludeTitleList();
		if (RentalShoppingCart != null)
		{
			List<IRentalShoppingCartTitleItem> list = (from x in RentalShoppingCart.GetTitleItemsByCriteria(RentalShoppingCartItemAction.rental, null, TitleType.All, TitleFamily.All, null, inCartExcludeTitleList, null, null, null, includeMultiNightPricedItems: false, includeFlashDeals: true, includeLoyaltyRedeemedItems: false, false, includeDiscountRestrictionItems: false)
				orderby (x?.PricingRecord?.InitialNight).GetValueOrDefault(), x.ProductId
				select x).ToList();
			promoCodeValidationInfo.IsCartValid = list.Count() > 1;
			decimal redemptionAmountFromInCartPromo = GetRedemptionAmountFromInCartPromo();
			foreach (IRentalShoppingCartTitleItem item in list)
			{
				if (promoCodeValidationInfo.QualifyingItems.Count() < 2)
				{
					decimal num = Math.Min(redemptionAmountFromInCartPromo, (item?.PricingRecord?.InitialNight).GetValueOrDefault());
					promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
					{
						RentalShoppingCartItem = item,
						PromotionAmount = num
					});
					promoCodeValidationInfo.QualifyingItems.Add(item);
					redemptionAmountFromInCartPromo -= num;
				}
			}
			promoCodeValidationInfo.Price = promoCodeValidationInfo.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		}
		return promoCodeValidationInfo;
	}

	public List<long> GetInCartIncludeTitleList()
	{
		return GetInCartTitleList(include: true);
	}

	public List<long> GetInCartExcludeTitleList()
	{
		return GetInCartTitleList(include: false);
	}

	private List<long> GetInCartTitleList(bool include)
	{
		List<long> list = new List<long>();
		if (_inCartPromoRules != null && !IsFreeInCartOfferRunning)
		{
			if (IsExcludeEmptyCaseInCartOfferRunning && !include)
			{
				list.Add(1139L);
			}
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				if (inCartPromoRule == null)
				{
					continue;
				}
				object value = null;
				inCartPromoRule.TryGetValue("offer_action", out value);
				if (value == null)
				{
					continue;
				}
				object value2 = null;
				string key = (include ? "product_id_list" : "exclude_id_list");
				if (!inCartPromoRule.TryGetValue(key, out value2) || value2 == null)
				{
					continue;
				}
				string[] array = value2.ToString().Split(',');
				if (array == null)
				{
					continue;
				}
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					if (long.TryParse(array2[i].Trim(), out var result) && !list.Contains(result))
					{
						list.Add(result);
					}
				}
			}
		}
		return list;
	}

	public List<TitleType> GetInCartExcludeTitleTypeList()
	{
		List<TitleType> list = new List<TitleType>();
		if (_inCartPromoRules != null && !IsFreeInCartOfferRunning && IsExclude4KInCartOfferRunning)
		{
			list.Add(TitleType._4KUHD);
		}
		return list;
	}

	public bool GetInCartExcludeDiscountRestriction()
	{
		bool result = false;
		if (_inCartPromoRules != null && !IsFreeInCartOfferRunning && IsExcludeDiscountRestrictionInCartOfferRunning)
		{
			result = true;
		}
		return result;
	}

	public void LoadInCartData(KioskInCartDetails inCart)
	{
		List<Dictionary<string, object>> kCSInCartConfigSettings = GetKCSInCartConfigSettings(inCart);
		Dictionary<string, object> dictionary = ((kCSInCartConfigSettings != null && kCSInCartConfigSettings.Count > 0) ? kCSInCartConfigSettings[0] : null);
		bool flag = false;
		if (dictionary != null)
		{
			object value = null;
			dictionary.TryGetValue("template_name", out value);
			string text = value?.ToString();
			if (text != "DEFAULT NO OFFER")
			{
				flag = true;
				LogHelper.Instance.Log("> InCart template loaded: {0}", text);
				List<Dictionary<string, object>> inCartWorkingRuleSet = GetInCartWorkingRuleSet(text);
				List<Dictionary<string, object>> workingRuleSetCopy = null;
				if (inCartWorkingRuleSet != null)
				{
					workingRuleSetCopy = new List<Dictionary<string, object>>();
					inCartWorkingRuleSet.ForEach(delegate(Dictionary<string, object> ruleSet)
					{
						workingRuleSetCopy.Add(new Dictionary<string, object>(ruleSet));
					});
				}
				if (workingRuleSetCopy != null)
				{
					object inCartVariablesUsed = GetInCartVariablesUsed(text);
					if (inCartVariablesUsed != null)
					{
						string[] array = inCartVariablesUsed.ToString().Split(',');
						Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
						string[] array2 = array;
						foreach (string text2 in array2)
						{
							object value2 = null;
							if (dictionary.TryGetValue(text2, out value2))
							{
								dictionary2[text2] = value2.ToString();
								LogHelper.Instance.Log("> Loaded {0} = {1}", text2, value2.ToString());
							}
						}
						LogHelper.Instance.Log("> Variable count: {0}, macro count: {1}", array.Length, dictionary2.Count);
						if (array.Length == dictionary2.Count)
						{
							foreach (KeyValuePair<string, string> item in dictionary2)
							{
								LogHelper.Instance.Log("> making replacements for: {0}", item.Key);
								string newValue = item.Value;
								if (item.Key == "INCLUDED_TITLES" || item.Key == "EXCLUDED_TITLES")
								{
									newValue = AddMultiDiscs(dictionary2[item.Key]);
								}
								foreach (Dictionary<string, object> item2 in workingRuleSetCopy)
								{
									Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
									foreach (KeyValuePair<string, object> item3 in item2)
									{
										string text3 = item3.Value?.ToString() ?? null;
										string text4 = $"{{{{{item.Key}}}}}";
										if (text3.Contains(text4))
										{
											string text5 = text3.Replace(text4, newValue);
											LogHelper.Instance.Log("> replacing {0} with {1}", item3.Value, text5);
											dictionary3[item3.Key] = text5;
										}
									}
									foreach (KeyValuePair<string, string> item4 in dictionary3)
									{
										item2[item4.Key] = item4.Value;
									}
								}
							}
							_inCartPromoRules = workingRuleSetCopy;
							ApplicationState.Instance.InCartTemplateName = text;
							LogHelper.Instance.Log("> InCart template loaded successfully");
						}
						else
						{
							LogHelper.Instance.Log("> Not all template variables were defined in promotion.config, unable to use template");
						}
					}
					else
					{
						LogHelper.Instance.Log("> InCart template variable names not defined in resource header");
					}
				}
				else
				{
					LogHelper.Instance.Log("> InCart template resource file {0} missing or unable to load", text);
				}
			}
		}
		if (!flag)
		{
			_inCartPromoRules = GetInCartWorkingRuleSet("in_cart_promotion_v2");
			object inCartProgramName = GetInCartProgramName("in_cart_promotion_v2");
			ApplicationState.Instance.InCartTemplateName = inCartProgramName?.ToString();
			LogHelper.Instance.Log("InCart loaded: {0}", inCartProgramName?.ToString());
		}
	}

	private List<Dictionary<string, object>> GetKCSInCartConfigSettings(KioskInCartDetails inCart)
	{
		if (inCart != null)
		{
			if (inCart.InCartType == InCartType.RentOneGetFixedOff)
			{
				return new List<Dictionary<string, object>>
				{
					new Dictionary<string, object>
					{
						{ "template_name", "ROGO MOVIE A" },
						{
							"EXCLUDED_TITLES",
							string.Join(",", inCart.ExcludeTitles)
						},
						{
							"DISCOUNT_AMOUNT",
							inCart.Amount.GetValueOrDefault().ToString("N2")
						},
						{ "PROMO_CODE", inCart.PromoCode }
					}
				};
			}
			if (inCart.InCartType == InCartType.FirstNightFree)
			{
				return new List<Dictionary<string, object>>
				{
					new Dictionary<string, object>
					{
						{ "template_name", "FREE NIGHT M" },
						{
							"EXCLUDED_TITLES",
							string.Join(",", inCart.ExcludeTitles)
						},
						{ "PROMO_CODE", inCart.PromoCode }
					}
				};
			}
		}
		return null;
	}

	private List<Dictionary<string, object>> GetInCartWorkingRuleSet(string templateName)
	{
		return templateName switch
		{
			"in_cart_promotion_v2" => _inCartOfferTemplateDefault.Content, 
			"ROGO MOVIE A" => _inCartOfferTemplateA.Content, 
			"FREE NIGHT M" => _inCartOfferTemplateM.Content, 
			_ => null, 
		};
	}

	private object GetInCartVariablesUsed(string templateName)
	{
		if (templateName == "ROGO MOVIE A")
		{
			return _inCartOfferTemplateA.VariablesUsed;
		}
		if (templateName == "FREE NIGHT M")
		{
			return _inCartOfferTemplateM.VariablesUsed;
		}
		return null;
	}

	private object GetInCartProgramName(string templateName)
	{
		return templateName switch
		{
			"in_cart_promotion_v2" => _inCartOfferTemplateDefault.ProgramName, 
			"ROGO MOVIE A" => _inCartOfferTemplateA.ProgramName, 
			"FREE NIGHT M" => _inCartOfferTemplateM.ProgramName, 
			_ => null, 
		};
	}

	private string GetValue(string key)
	{
		string result = null;
		if (_inCartPromoRules != null)
		{
			foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
			{
				foreach (KeyValuePair<string, object> item in inCartPromoRule)
				{
					if (item.Key == key)
					{
						result = item.Value.ToString();
					}
				}
			}
		}
		return result;
	}

	private string AddMultiDiscs(string productIdList)
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null)
		{
			List<string> list = new List<string>(productIdList.Split(','));
			foreach (string item in list)
			{
				if (!long.TryParse(item, out var result))
				{
					continue;
				}
				IKioskProduct kioskProduct = null;
				service.GetProduct(result, excludeTitleRollup: false, out kioskProduct);
				if (!(kioskProduct is MDVProduct mDVProduct))
				{
					continue;
				}
				foreach (ITitleProduct titleProduct in mDVProduct.TitleProducts)
				{
					if (!productIdList.Contains(titleProduct.ProductId.ToString()))
					{
						list.Add(titleProduct.ProductId.ToString());
					}
				}
			}
			productIdList = list.Join(",");
		}
		return productIdList;
	}
}
