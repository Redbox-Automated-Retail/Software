namespace Redbox.HAL.Component.Model;

public interface IFraudService
{
	bool IsConfigured { get; }

	string FraudImagesPath { get; }
}
