namespace Redbox.Rental.Model.Session;

public interface IUserInfo
{
	string CustomerProfileNumber { get; set; }

	string Email { get; set; }

	string PostalCode { get; set; }

	string AccountNumber { get; set; }
}
