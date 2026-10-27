namespace Redbox.Rental.Model.Profile;

public interface IProfileData
{
	bool HasStaged();

	bool HasActive();

	bool Activate();

	IProfileDataInstance LoadDataInstance();
}
