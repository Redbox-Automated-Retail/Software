namespace Redbox.Rental.Model.KioskClientService.Campaign;

public interface IStartAssetControl : IControl
{
	Asset Asset { get; }

	AssetTarget Target { get; }

	string TargetValue { get; }

	short Order { get; }

	bool? IncludeIfNoInventory { get; }

	StartScreenPromotionCodeValidationResponse Validation { get; }
}
