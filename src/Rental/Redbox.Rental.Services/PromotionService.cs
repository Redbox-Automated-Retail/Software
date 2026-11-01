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
			IRentalShoppingCartService rentalShoppingCartService = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			if (GetPromotion() == null && IsFreeInCartOfferRunning)
			{
				IPromoCodeValidationInfo promoCodeValidationInfo = ValidateFreeInCartOffer();
				if (promoCodeValidationInfo != null && promoCodeValidationInfo.IsCartValid)
				{
					return rentalShoppingCartService?.CurrentRentalShoppingCart?.InCartPromoOfferMade != true;
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
			int qualifyingCount = (Instance.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count()).GetValueOrDefault();
			bool oneQualifiesNotOffered = qualifyingCount == 1 && !(RentalShoppingCart?.InCartPromoOfferMade ?? false);
			bool severalQualify = qualifyingCount > 1;
			if (IsRentTwoInCartOfferRunning)
			{
				IRentalShoppingCart rentalShoppingCart = RentalShoppingCart;
				if ((rentalShoppingCart == null || !rentalShoppingCart.InCartDoNotShow) && GetPromotion() == null)
				{
					return oneQualifiesNotOffered | severalQualify;
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
			bool isReturnVisit = RentalShoppingCart?.Promotion?.IsReturnVisit == true;
			IRentalShoppingCart rentalShoppingCart = RentalShoppingCart;
			bool firstNightFreeItem = rentalShoppingCart != null && rentalShoppingCart.Promotion?.Intent == PromotionIntentCode.FirstNightFreeItem;
			bool discountUsed = (RentalShoppingCart?.Totals?.DiscountAmountUsed).GetValueOrDefault() > 0m;
			bool warningShown = BrowseState?.ReturnVisitIneligibleCartWarningShown ?? false;
			if ((isReturnVisit & firstNightFreeItem) && !discountUsed)
			{
				return !warningShown;
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
		IPersonalizationService personalizationService = ServiceLocator.Instance.GetService<IPersonalizationService>();
		if (promotion != null)
		{
			personalizationService?.SetSelectedPromo(promotion.Name);
		}
		else
		{
			personalizationService?.ClearSelectedPromo();
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
					if (!int.TryParse(storeNumber, out var storeId))
					{
						LogHelper.Instance.Log("The passed storeNumber '" + storeNumber + "' cannot be converted into an Int32.", LogEntryType.Error);
						result.Success = false;
						result.Properties["response"] = PromotionValidationResponse.Error.ToString();
					}
					else
					{
						ServiceLocator.Instance.GetService<IMacroService>();
						Guid messageId = Guid.NewGuid();
						IDictionary<string, object> validation = await ServiceLocator.Instance.GetService<IKioskClientService>().Promotion.ValidatePromotionCode(messageId, promotionCode, storeId);
						result.Success = validation.ContainsKey("Success") && (bool)validation["Success"];
						result.Properties["response"] = PromotionValidationResponse.Valid.ToString();
						result.Properties["amount"] = (validation.ContainsKey("Amount") ? validation["Amount"] : ((object)1));
						result.Properties["promotion_intent"] = (validation.ContainsKey("PromotionIntentCode") ? validation["PromotionIntentCode"] : PromotionIntentCode.FixedValue.ToString());
						result.Properties["rent_qty"] = (validation.ContainsKey("RentQty") ? validation["RentQty"] : ((object)0));
						result.Properties["rent_format"] = (validation.ContainsKey("RentFormat") ? validation["RentFormat"] : null);
						result.Properties["get_qty"] = (validation.ContainsKey("GetQty") ? validation["GetQty"] : ((object)0));
						result.Properties["get_format"] = (validation.ContainsKey("GetFormat") ? validation["GetFormat"] : null);
						Dictionary<string, object> campaignTitleData = (validation.ContainsKey("CampaignTitles") ? (validation["CampaignTitles"] as Dictionary<string, object>) : null);
						if (campaignTitleData != null)
						{
							ArrayList campaignTitles = campaignTitleData["Titles"] as ArrayList;
							Dictionary<string, object> campaignTitlesProperty = new Dictionary<string, object>
							{
								["include"] = (bool)campaignTitleData["Include"],
								["titles"] = campaignTitles
							};
							result.Properties["campaign_titles"] = campaignTitlesProperty;
						}
						if (validation.ContainsKey("ServerName"))
						{
							result.Properties["server_name"] = validation["ServerName"];
						}
						result.Properties["product_type_id"] = (validation.ContainsKey("ProductTypeId") ? validation["ProductTypeId"] : null);
						result.Properties["format_ids"] = (validation.ContainsKey("FormatIds") ? (validation["FormatIds"] as ArrayList) : null);
						result.Properties["action_code"] = (validation.ContainsKey("ActionType") ? validation["ActionType"] : PromotionActionCode.Rental.ToString());
						result.Properties["allow_full_discount"] = (validation.ContainsKey("AllowFullDiscount") ? validation["AllowFullDiscount"] : ((object)true));
						result.Properties["default_promo"] = (validation.ContainsKey("DefaultPromo") ? validation["DefaultPromo"] : null);
						if (!result.Success)
						{
							ArrayList arrayList = (validation.ContainsKey("Errors") ? (validation["Errors"] as ArrayList) : null);
							if (arrayList != null && arrayList.Count > 0)
							{
								if (arrayList[0] is Dictionary<string, object> responseCodeEntry)
								{
									object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(PromotionValidationResponse?), (string)responseCodeEntry["Code"]);
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
				if (result.Properties.ContainsKey("get_qty") && int.TryParse(result.Properties["get_qty"]?.ToString(), out var getQuantity))
				{
					promoCodeValidationResult.GetQuantity = getQuantity;
				}
				if (result.Properties.ContainsKey("rent_qty") && int.TryParse(result.Properties["rent_qty"]?.ToString(), out var rentQuantity))
				{
					promoCodeValidationResult.RentQuantity = rentQuantity;
				}
				if (result.Properties.ContainsKey("amount") && decimal.TryParse(result.Properties["amount"]?.ToString(), out var parsedAmount))
				{
					promoCodeValidationResult.Amount = parsedAmount;
				}
				if (result.Properties.ContainsKey("get_format") && Enum.TryParse<PromotionGetFormat>(result.Properties["get_format"]?.ToString(), out var getFormat))
				{
					promoCodeValidationResult.GetFormat = getFormat;
				}
				if (result.Properties.ContainsKey("rent_format") && Enum.TryParse<PromotionRentFormat>(result.Properties["rent_format"]?.ToString(), out var rentFormat))
				{
					promoCodeValidationResult.RentFormat = rentFormat;
				}
				if (result.Properties.ContainsKey("promotion_intent") && Enum.TryParse<PromotionIntentCode>(result.Properties["promotion_intent"]?.ToString(), out var intentCode))
				{
					promoCodeValidationResult.IntentCode = intentCode;
				}
				if (result.Properties.ContainsKey("campaign_titles") && result.Properties["campaign_titles"] is Dictionary<string, object> campaignTitles)
				{
					promoCodeValidationResult.CampaignTitles = new CampaignTitles
					{
						Titles = new List<long>()
					};
					if (campaignTitles.ContainsKey("include") && bool.TryParse(campaignTitles["include"]?.ToString(), out var includeTitles))
					{
						promoCodeValidationResult.CampaignTitles.Include = includeTitles;
					}
					if (campaignTitles.ContainsKey("titles") && campaignTitles["titles"] is ArrayList titleList)
					{
						foreach (object titleIdValue in titleList)
						{
							if (long.TryParse(titleIdValue?.ToString(), out var titleId))
							{
								promoCodeValidationResult.CampaignTitles.Titles.Add(titleId);
							}
						}
					}
				}
				if (result.Properties.ContainsKey("product_type_id") && int.TryParse(result.Properties["product_type_id"]?.ToString(), out var productTypeId))
				{
					promoCodeValidationResult.ProductTypeId = productTypeId;
				}
				if (result.Properties.ContainsKey("format_ids") && result.Properties["format_ids"] is ArrayList formatIdList)
				{
					promoCodeValidationResult.FormatIds = new List<int>();
					foreach (object formatIdValue in formatIdList)
					{
						if (int.TryParse(formatIdValue?.ToString(), out var formatId))
						{
							promoCodeValidationResult.FormatIds.Add(formatId);
						}
					}
				}
				if (result.Properties.ContainsKey("action_code") && Enum.TryParse<PromotionActionCode>(result.Properties["action_code"]?.ToString(), out var actionCode))
				{
					promoCodeValidationResult.ActionCode = actionCode;
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
		bool qualifies = false;
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
							qualifies = true;
							bool askOnlyOnce = false;
							int conditionNumber = 0;
							foreach (Dictionary<string, object> inCartPromoRule in _inCartPromoRules)
							{
								conditionNumber++;
								PromotionRule promotionRule = new PromotionRule(inCartPromoRule);
								if (!string.IsNullOrEmpty(promotionRule.OfferCondition))
								{
									int matchingCount = 0;
									if (promotionRule.OfferCondition == PromotionRule.Constants.Product)
									{
										matchingCount = rentalShoppingCart.GetTitleItemsByCriteria(promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes, null, null, includeMultiNightPricedItems: true, includeFlashDeals: true, includeLoyaltyRedeemedItems: true, null, includeDiscountRestrictionItems: true)?.Count ?? 0;
									}
									else if (promotionRule.OfferCondition == PromotionRule.Constants.Cart)
									{
										matchingCount = rentalShoppingCart.GetTitleItemsByCriteria(promotionRule.Action, promotionRule.Sequence, TitleType.All, TitleFamily.All, null, null, null, null, null, includeMultiNightPricedItems: true, includeFlashDeals: false, includeLoyaltyRedeemedItems: true, null, includeDiscountRestrictionItems: true)?.Count ?? 0;
									}
									else if (promotionRule.OfferCondition == PromotionRule.Constants.BrowseMinimumQuantity)
									{
										matchingCount = BrowseCountByCriteria(rentalShoppingCart, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts);
									}
									if (matchingCount < promotionRule.MinimumQuantity)
									{
										LogIfVerboseLoggingIsOn(string.Format(">> {2}, min_qty={0} true_count={1}", promotionRule.MinimumQuantity, matchingCount, promotionRule.OfferCondition));
										LogHelper.Instance.Log("> failed condition {0}", conditionNumber);
										qualifies = false;
									}
									if (matchingCount > promotionRule.MaximumQuantity)
									{
										LogIfVerboseLoggingIsOn(string.Format(">> {2}, max_qty={0} true_count={1}", promotionRule.MaximumQuantity, matchingCount, promotionRule.OfferCondition));
										LogHelper.Instance.Log("> failed condition {0}", conditionNumber);
										qualifies = false;
									}
									LogIfVerboseLoggingIsOn($"> offer condition ({conditionNumber}) {promotionRule.OfferCondition} returns {qualifies}");
								}
								if (promotionRule.AskOnlyOnce == true)
								{
									askOnlyOnce = true;
								}
								if (promotionRule.IncludeProducts == null || promotionRule.IncludeProducts.Count <= 0)
								{
									continue;
								}
								IDataService dataService = ServiceLocator.Instance.GetService<IDataService>();
								bool noIncludedProductInStock = true;
								if (dataService != null)
								{
									foreach (long includeProduct in promotionRule.IncludeProducts)
									{
										IKioskProduct kioskProduct = null;
										dataService.GetProduct(includeProduct, excludeTitleRollup: false, out kioskProduct);
										if (kioskProduct is ITitleProduct { IsRentableAndInStock: not false })
										{
											noIncludedProductInStock = false;
											break;
										}
									}
								}
								if (noIncludedProductInStock)
								{
									LogHelper.Instance.Log(">  no titles in the offer are in stock, returning False");
									qualifies = false;
								}
							}
							if (askOnlyOnce && rentalShoppingCart.InCartPromoOfferMade)
							{
								LogHelper.Instance.Log(">  offer already presented, returning False");
								qualifies = false;
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
		return qualifies;
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
		int count = 0;
		if (includeProductList == null)
		{
			includeProductList = new List<long>();
		}
		if (excludeProductList == null)
		{
			excludeProductList = new List<long>();
		}
		LogHelper.Instance.Log(string.Format("> BrowseCountBy pt: {0}, pf: {1}, include list: {2}, exclude list: {3}", titleType, titleFamily, string.Join(", ", includeProductList), string.Join(", ", excludeProductList)));
		IDataService dataService = ServiceLocator.Instance.GetService<IDataService>();
		if (dataService != null)
		{
			List<IKioskProduct> products = null;
			dataService.GetProducts(BrowseViewFilter.All, titleFamily, titleType, BrowseSort.MerchandiseDate, null, null, null, null, isADAView: false, null, null, out products);
			foreach (IKioskProduct item in products)
			{
				ITitleProduct titleProduct = item as ITitleProduct;
				if (titleProduct != null && titleProduct.IsRentableAndInStock && !titleProduct.IsEmptyCase && (!titleProduct.DiscNumber.HasValue || titleProduct.DiscNumber == 1) && (includeProductList.Count == 0 || includeProductList.Contains(titleProduct.ProductId)) && (excludeProductList.Count == 0 || !excludeProductList.Contains(titleProduct.ProductId)) && rentalShoppingCart.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.ProductId == titleProduct.ProductId && x.Action == RentalShoppingCartItemAction.rental) == null)
				{
					count++;
				}
			}
		}
		LogHelper.Instance.Log("> BrowseCountBy result: {0}", count);
		return count;
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
		List<IRentalShoppingCartTitleItem> eligibleItems = (from x in rentalShoppingCart.GetTitleItemsByCriteria(promotionRule.Action, promotionRule.Sequence, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, promotionRule.ProductTypes, null, null, includeMultiNightPricedItems: true, includeFlashDeals: true, includeLoyaltyRedeemedItems: true, null, includeDiscountRestrictionItems: true)
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).ToList();
		int discountedCount = eligibleItems?.Count() ?? 0;
		LogHelper.Instance.Log("qty_based_amount = {0} * {1}", promotionRule.RedemptionAmount, discountedCount);
		if (discountedCount > promotionRule.MaximumQuantity)
		{
			LogHelper.Instance.Log("> user exceeded the max qty on a qty-based-amount in-cart, resetting discount to max allowed");
			discountedCount = promotionRule.MaximumQuantity;
		}
		decimal amountPerItem = 0m;
		decimal.TryParse(promotionRule.RedemptionAmount, out amountPerItem);
		foreach (IRentalShoppingCartTitleItem item in eligibleItems)
		{
			if (result.PromotionItems.Count() < discountedCount)
			{
				decimal promotionAmount = Math.Min(amountPerItem, (item?.PricingRecord?.InitialNight).GetValueOrDefault());
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
		if (!decimal.TryParse(promotionRule.RedemptionAmount, out var fixedAmount))
		{
			return;
		}
		List<IRentalShoppingCartTitleItem> eligibleItems = (from x in rentalShoppingCart.GetTitleItemsByCriteria(RentalShoppingCartItemAction.rental, null, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, null, null, null, includeMultiNightPricedItems: false, includeFlashDeals: true, includeLoyaltyRedeemedItems: false, false, includeDiscountRestrictionItems: false)
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).ToList();
		decimal remaining = fixedAmount;
		foreach (IRentalShoppingCartTitleItem item in eligibleItems)
		{
			if (remaining > 0m)
			{
				decimal itemDiscount = Math.Min(remaining, (item?.PricingRecord?.InitialNight).GetValueOrDefault());
				result.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = item,
					PromotionAmount = itemDiscount
				});
				remaining -= itemDiscount;
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
		decimal productTypePrice = 0m;
		if (productTypePricingRecord != null)
		{
			string redemptionAmount = promotionRule.RedemptionAmount;
			if (!(redemptionAmount == "initial_night"))
			{
				if (redemptionAmount == "extra_night")
				{
					productTypePrice = productTypePricingRecord.ExtraNight;
				}
			}
			else
			{
				productTypePrice = productTypePricingRecord.InitialNight;
			}
		}
		List<IRentalShoppingCartTitleItem> eligibleItems = (from x in rentalShoppingCart.GetTitleItemsByCriteria(RentalShoppingCartItemAction.rental, null, promotionRule.TitleType, promotionRule.TitleFamily, promotionRule.IncludeProducts, promotionRule.ExcludeProducts, null, null, null, includeMultiNightPricedItems: false, includeFlashDeals: true, includeLoyaltyRedeemedItems: false, false, includeDiscountRestrictionItems: false)
			orderby x.PricingRecord.InitialNight, x.ProductId
			select x).ToList();
		decimal remaining = productTypePrice;
		foreach (IRentalShoppingCartTitleItem item in eligibleItems)
		{
			if (remaining > 0m)
			{
				decimal itemDiscount = Math.Min(remaining, (item?.PricingRecord?.InitialNight).GetValueOrDefault());
				result.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = item,
					PromotionAmount = itemDiscount
				});
				remaining -= itemDiscount;
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
					decimal redemptionAmount = 0m;
					if (decimal.TryParse(promotionRule.RedemptionAmount, out redemptionAmount))
					{
						result = redemptionAmount;
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
			decimal promoAmount = CalcInCartOfferPromoAmount(applyToLineItem, offer);
			if (offer.CustomerSegmentType == CustomerSegmentType.BasketStretch && applyToLineItem != null)
			{
				IPromotion promotion = new Redbox.Rental.Services.Promotion.Promotion
				{
					Name = (offer.PromotionCode ?? offer.OfferDiscountCode),
					DiscountType = ((offer.PromotionCode != null) ? DiscountType.PromotionCode : DiscountType.Discount),
					OriginalPrice = promoAmount,
					Price = promoAmount,
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
					PromotionAmount = promoAmount,
					RentalShoppingCartItem = applyToLineItem
				});
				SetPromotion(promotion);
			}
			else
			{
				PromotionIntentCode intent = ((offer.DiscountType == OfferDiscountType.Free) ? PromotionIntentCode.FirstNightFreeItem : ((offer.DiscountType == OfferDiscountType.MultiNightPrice) ? PromotionIntentCode.MultiNightItem : PromotionIntentCode.FixedValue));
				decimal offerPrice = ((offer.DiscountType == OfferDiscountType.Free) ? 0m : offer.Amount);
				IPromotion offerPromotion = new Redbox.Rental.Services.Promotion.Promotion
				{
					Name = (offer.PromotionCode ?? offer.OfferDiscountCode),
					DiscountType = ((offer.PromotionCode != null) ? DiscountType.PromotionCode : DiscountType.Discount),
					Price = offerPrice,
					OriginalPrice = offerPrice,
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
				SetPromotion(offerPromotion);
				IRentalShoppingCartService rentalShoppingCartService = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
				rentalShoppingCartService.CurrentRentalShoppingCart.ValidateAndUpdatePromoCode();
				rentalShoppingCartService.CurrentRentalShoppingCart.UpdateServiceFee();
			}
		}
	}

	private List<int> GetFilteredFormatIds(List<int> formatIds)
	{
		List<int> list = new List<int>();
		if (formatIds != null)
		{
			foreach (TitleType item in (TitleType[])Enum.GetValues(typeof(TitleType)))
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
		decimal promoAmount = 0m;
		if (newLineItem != null)
		{
			if (offer.DiscountType == OfferDiscountType.AmountOff)
			{
				promoAmount = offer.Amount;
			}
			else if (offer.DiscountType == OfferDiscountType.PercentOff)
			{
				promoAmount = newLineItem.Price * (offer.Amount / 100m);
			}
			else if (offer.DiscountType == OfferDiscountType.Free)
			{
				promoAmount = newLineItem.Price;
			}
			promoAmount = Math.Min(promoAmount, newLineItem.Price);
		}
		return promoAmount;
	}

	public IPromoCodeValidationInfo ValidateFreeInCartOffer()
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		TitleFamily titleFamilyFromInCartPromo = GetTitleFamilyFromInCartPromo();
		TitleType titleTypeFromInCartPromo = GetTitleTypeFromInCartPromo();
		List<long> excludeProducts = null;
		IRentalShoppingCartService rentalShoppingCartService = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (rentalShoppingCartService.CurrentRentalShoppingCart != null)
		{
			IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = (from x in rentalShoppingCartService.CurrentRentalShoppingCart.GetTitleItemsByCriteria(RentalShoppingCartItemAction.rental, null, titleTypeFromInCartPromo, titleFamilyFromInCartPromo, null, excludeProducts, null, null, null, includeMultiNightPricedItems: false, includeFlashDeals: true, includeLoyaltyRedeemedItems: false, null, includeDiscountRestrictionItems: false)
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
					decimal promotionAmount = Math.Min(redemptionAmountFromInCartPromo, (item?.PricingRecord?.InitialNight).GetValueOrDefault());
					promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
					{
						RentalShoppingCartItem = item,
						PromotionAmount = promotionAmount
					});
					promoCodeValidationInfo.QualifyingItems.Add(item);
					redemptionAmountFromInCartPromo -= promotionAmount;
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
				object offerAction = null;
				inCartPromoRule.TryGetValue("offer_action", out offerAction);
				if (offerAction == null)
				{
					continue;
				}
				object productIdList = null;
				string key = (include ? "product_id_list" : "exclude_id_list");
				if (!inCartPromoRule.TryGetValue(key, out productIdList) || productIdList == null)
				{
					continue;
				}
				string[] productIds = productIdList.ToString().Split(',');
				if (productIds == null)
				{
					continue;
				}
				for (int i = 0; i < productIds.Length; i++)
				{
					if (long.TryParse(productIds[i].Trim(), out var result) && !list.Contains(result))
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

	// The in-cart offer from the kiosk's settings: its template's rules, with each {{VARIABLE}} the template uses replaced by
	// the setting of that name (title lists get their multi-disc titles added). Without a template, or if a variable is
	// missing, the default in_cart_promotion_v2 rules are used.
	public void LoadInCartData(KioskInCartDetails inCart)
	{
		List<Dictionary<string, object>> kCSInCartConfigSettings = GetKCSInCartConfigSettings(inCart);
		Dictionary<string, object> settings = ((kCSInCartConfigSettings != null && kCSInCartConfigSettings.Count > 0) ? kCSInCartConfigSettings[0] : null);
		bool templateLoaded = false;
		if (settings != null)
		{
			object value = null;
			settings.TryGetValue("template_name", out value);
			string templateName = value?.ToString();
			if (templateName != "DEFAULT NO OFFER")
			{
				templateLoaded = true;
				LogHelper.Instance.Log("> InCart template loaded: {0}", templateName);
				List<Dictionary<string, object>> inCartWorkingRuleSet = GetInCartWorkingRuleSet(templateName);
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
					object inCartVariablesUsed = GetInCartVariablesUsed(templateName);
					if (inCartVariablesUsed != null)
					{
						string[] variableNames = inCartVariablesUsed.ToString().Split(',');
						Dictionary<string, string> variableValues = new Dictionary<string, string>();
						foreach (string variableName in variableNames)
						{
							object variableValue = null;
							if (settings.TryGetValue(variableName, out variableValue))
							{
								variableValues[variableName] = variableValue.ToString();
								LogHelper.Instance.Log("> Loaded {0} = {1}", variableName, variableValue.ToString());
							}
						}
						LogHelper.Instance.Log("> Variable count: {0}, macro count: {1}", variableNames.Length, variableValues.Count);
						if (variableNames.Length == variableValues.Count)
						{
							foreach (KeyValuePair<string, string> templateVariable in variableValues)
							{
								LogHelper.Instance.Log("> making replacements for: {0}", templateVariable.Key);
								string newValue = templateVariable.Value;
								if (templateVariable.Key == "INCLUDED_TITLES" || templateVariable.Key == "EXCLUDED_TITLES")
								{
									newValue = AddMultiDiscs(variableValues[templateVariable.Key]);
								}
								foreach (Dictionary<string, object> rule in workingRuleSetCopy)
								{
									Dictionary<string, string> changedSettings = new Dictionary<string, string>();
									foreach (KeyValuePair<string, object> setting in rule)
									{
										string settingValue = setting.Value?.ToString() ?? null;
										string placeholder = $"{{{{{templateVariable.Key}}}}}";
										if (settingValue.Contains(placeholder))
										{
											string replaced = settingValue.Replace(placeholder, newValue);
											LogHelper.Instance.Log("> replacing {0} with {1}", setting.Value, replaced);
											changedSettings[setting.Key] = replaced;
										}
									}
									foreach (KeyValuePair<string, string> changedSetting in changedSettings)
									{
										rule[changedSetting.Key] = changedSetting.Value;
									}
								}
							}
							_inCartPromoRules = workingRuleSetCopy;
							ApplicationState.Instance.InCartTemplateName = templateName;
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
					LogHelper.Instance.Log("> InCart template resource file {0} missing or unable to load", templateName);
				}
			}
		}
		if (!templateLoaded)
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
		IDataService dataService = ServiceLocator.Instance.GetService<IDataService>();
		if (dataService != null)
		{
			List<string> list = new List<string>(productIdList.Split(','));
			foreach (string item in list)
			{
				if (!long.TryParse(item, out var result))
				{
					continue;
				}
				IKioskProduct kioskProduct = null;
				dataService.GetProduct(result, excludeTitleRollup: false, out kioskProduct);
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
