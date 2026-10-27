namespace Redbox.KioskEngine.ComponentModel;

public interface ISecurityService
{
	bool CreateUser(string userName, string password, int numberOfDaysToExpire);

	bool Authenticate(string userName, string password);

	bool UserExists(string userName, out bool isAccountLocked);

	bool RemoveUser(string userName);

	bool SlideExpirationDate(string userName, int numberOfDays);
}
