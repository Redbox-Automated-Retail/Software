namespace Redbox.Rental.Model;

public class Error : IError
{
	public string Code { get; set; }

	public string Message { get; set; }
}
