namespace Redbox.Rental.Model.UpdateClientService.KioskHealth;

public interface IPingRequest : IMessageBase
{
	string AppName { get; set; }

	string UTCDate { get; set; }

	string LocalDate { get; set; }
}
