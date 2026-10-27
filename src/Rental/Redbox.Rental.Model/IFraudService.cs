namespace Redbox.Rental.Model;

public interface IFraudService
{
	void AddToFraudStatistics(string count);

	void CleanImageFolder();
}
