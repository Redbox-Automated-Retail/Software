using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class UserInfo : IUserInfo
{
	private string _email;

	private string _postalCode;

	public string CustomerProfileNumber { get; set; }

	public string Email
	{
		get
		{
			return _email;
		}
		set
		{
			_email = value;
		}
	}

	public string PostalCode
	{
		get
		{
			return _postalCode;
		}
		set
		{
			_postalCode = value;
		}
	}

	public string AccountNumber { get; set; }
}
