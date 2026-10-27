namespace Redbox.Rental.Model.Cache;

public interface ICacheData
{
	ICacheDataInstance LoadData();

	BarcodeProduct UpdateBarcodePosition(ICacheDataInstance instance, string key, object data, out bool isNew);

	TitleCache UpdateProductCatalog(ICacheDataInstance instance, string key, object data, out bool isNew);

	TitleCache UpdateProductHistory(ICacheDataInstance instance, string key, object data, out bool isNew);

	TitleCache UpdateProductToBarcode(ICacheDataInstance instance, string key, object data, out bool isNew);

	void RemoveBarcodePosition(ICacheDataInstance instance, string key);

	void RemoveProductCatalog(ICacheDataInstance instance, string key);

	void RemoveProductHistory(ICacheDataInstance instance, string key);

	void RemoveProductToBarcode(ICacheDataInstance instance, string key);
}
