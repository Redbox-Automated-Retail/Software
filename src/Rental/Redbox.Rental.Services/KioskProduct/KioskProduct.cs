using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.KioskProduct;

public class KioskProduct : IKioskProduct
{
	internal Store _store;

	protected KioskProductType _kioskProductType;

	public bool _isValidProduct = true;

	public KioskProductType KioskProductType
	{
		get
		{
			return _kioskProductType;
		}
		set
		{
			_kioskProductType = value;
		}
	}

	public virtual bool IsValidProduct
	{
		get
		{
			return _isValidProduct;
		}
		set
		{
			_isValidProduct = value;
		}
	}

	public virtual string ProductName { get; set; }

	public virtual string ProductDescription { get; set; }

	public KioskProduct(Store store)
	{
		_store = store;
	}
}
