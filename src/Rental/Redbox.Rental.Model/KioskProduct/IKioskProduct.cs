namespace Redbox.Rental.Model.KioskProduct;

public interface IKioskProduct
{
	KioskProductType KioskProductType { get; set; }

	bool IsValidProduct { get; set; }

	string ProductName { get; set; }

	string ProductDescription { get; set; }
}
