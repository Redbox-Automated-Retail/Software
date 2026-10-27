namespace KioskClientService.API.Services.Campaigns;

public interface IStartAssetControl : IControl
{
	Asset Asset { get; }

	AssetTarget? Target { get; }

	string TargetValue { get; }

	short? Order { get; }

	bool? ShowPressToStart { get; }

	bool? IncludeIfNoInventory { get; }
}
