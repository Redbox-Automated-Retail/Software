namespace Redbox.Rental.Model.KioskClientService.Product;

public interface IKioskClientServiceProduct
{
	IProductByTitleIdsResponse ProductsByTitleIds(IProductByTitleIdsRequest request);
}
