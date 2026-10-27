using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Campaign;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.KioskClientService.Campaign;

public class KioskClientServiceCampaign : BaseKioskClientService, IKioskClientServiceCampaign
{
	private const string _controllerPrefix = "api/campaign";

	private bool _enableInCart;

	private bool _showStartScreen;

	private int _loadCampaignTimerDuration;

	private KioskInCartDetails _inCart;

	private KioskStartScreenDetails _startScreen;

	public bool EnableInCart => _enableInCart;

	public bool ShowStartScreen => _showStartScreen;

	public int LoadCampaignTimerDuration => _loadCampaignTimerDuration;

	public KioskClientServiceCampaign()
	{
		LoadCampaign();
	}

	public void LoadCampaign()
	{
		LogHelper.Instance.Log("KCS.Campaign.KioskClientServiceCampaign.LoadCampaign");
		IConfiguration instance = Redbox.Rental.Services.Configuration.Configuration.Instance;
		_enableInCart = instance.Current.CMS.EnableInCart;
		LogHelper.Instance.Log($"EnableInCart {_enableInCart}");
		_inCart = (_enableInCart ? GetKCSInCart() : null);
		LogHelper.Instance.Log("InCart " + ((_inCart != null) ? "loaded" : "null"));
		PromotionService.Instance?.LoadInCartData(_inCart);
		_showStartScreen = instance.Current.CMS.ShowStartScreen;
		LogHelper.Instance.Log($"ShowStartScreen {_showStartScreen}");
		_startScreen = (_showStartScreen ? GetKCSStartScreen() : null);
		LogHelper instance2 = LogHelper.Instance;
		KioskStartScreenDetails startScreen = _startScreen;
		object obj;
		if (startScreen != null)
		{
			ControlList controls = startScreen.Controls;
			if (controls != null)
			{
				_ = controls.Count;
				if (true)
				{
					obj = $"loaded {_startScreen.Controls.Count}";
					goto IL_0151;
				}
			}
		}
		obj = "null";
		goto IL_0151;
		IL_0151:
		instance2.Log("StartScreen " + (string)obj);
		_loadCampaignTimerDuration = instance.Current.CMS.LoadCampaignTimerDuration;
		LogHelper.Instance.Log($"LoadCampaignTimerDuration {_loadCampaignTimerDuration}");
	}

	public KioskInCartDetails GetInCart()
	{
		return _inCart;
	}

	public KioskStartScreenDetails GetStartScreen()
	{
		return _startScreen;
	}

	public ICarouselControl GetCarouselControl()
	{
		return _startScreen?.Controls?.Where((IControl c) => c.ControlType == ControlType.Carousel && c is ICarouselControl).Select((IControl c) => c as ICarouselControl).FirstOrDefault();
	}

	public IList<IStartAssetControl> GetStartAssetControls()
	{
		return _startScreen?.Controls?.Where((IControl c) => c.ControlType == ControlType.StartAsset && c is IStartAssetControl).Select((IControl c) => c as IStartAssetControl).OrderBy((IStartAssetControl sac) => sac.Order)
			.ToList();
	}

	private KioskInCartDetails GetKCSInCart()
	{
		try
		{
			WebResult<GetInCartsResponse> webResult = ExecuteWebRequest<GetInCartsResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/campaign/incart"), null, "GET", 500);
			return (webResult == null || webResult.StatusCode != HttpStatusCode.OK) ? null : webResult?.Response?.InCart;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KCS.Campaign.KioskClientServiceCampaign.GetKCSInCart", e);
			return null;
		}
	}

	private KioskStartScreenDetails GetKCSStartScreen()
	{
		try
		{
			WebResult<GetStartScreenResponse> webResult = ExecuteWebRequest<GetStartScreenResponse>(new Uri(base.KioskClientServiceBaseUrl + "/api/campaign/startscreen"), null, "GET", 500);
			return (webResult == null || webResult.StatusCode != HttpStatusCode.OK) ? null : webResult?.Response?.StartScreen;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KCS.Campaign.KioskClientServiceCampaign.GetKCSStartScreen", e);
			return null;
		}
	}
}
