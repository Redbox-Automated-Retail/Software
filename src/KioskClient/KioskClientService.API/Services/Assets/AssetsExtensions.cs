using System.Collections.Generic;
using System.Linq;
using KioskClientService.API.Services.Campaigns;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.Assets;

public static class AssetsExtensions
{
	public static IServiceCollection AddAssetsService(this IServiceCollection services)
	{
		return services.AddScoped<IAssetsService, AssetsService>();
	}

	internal static IEnumerable<Asset> GetAssets(this IEnumerable<StartScreenDetails> details)
	{
		HashSet<Asset> hashSet = new HashSet<Asset>();
		foreach (StartScreenDetails detail in details)
		{
			IEnumerable<IControl> enumerable = detail.Controls.Where((IControl c) => c.ControlType == ControlType.StartAsset);
			foreach (IControl item in enumerable)
			{
				StartAssetControl startAssetControl = item as StartAssetControl;
				if (startAssetControl.Asset != null)
				{
					hashSet.Add(startAssetControl.Asset);
				}
			}
		}
		return hashSet;
	}
}
