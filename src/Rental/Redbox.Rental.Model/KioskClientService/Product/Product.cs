namespace Redbox.Rental.Model.KioskClientService.Product;

public class Product
{
	public long TitleId { get; set; }

	public ProductGroup ProductGroup { get; set; } = new ProductGroup();

	public string ProductPage { get; set; }
}
