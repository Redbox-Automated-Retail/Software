using System;
using Redbox.Core;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Products;

namespace Redbox.Rental.Services.KioskProduct;

public class ProductGroupService : IProductGroupService
{
	private LogHelper _logger;

	private IKioskClientService _kioskClientService;

	private ProductGroupSortOrder _productGroupSortOrder;

	private ProductGroupSortOrder _productGroupSortOrderStaged;

	public ProductGroupService(LogHelper logger, IKioskClientService kioskClientService)
	{
		_logger = logger;
		_kioskClientService = kioskClientService;
	}

	public void StartSession()
	{
		try
		{
			LoadProductGroupSortOrder();
			if (_productGroupSortOrderStaged != null)
			{
				_productGroupSortOrder = _productGroupSortOrderStaged;
				_productGroupSortOrderStaged = null;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("ProdcutGroupService.LoadProductGroupSortOrder - an exception occurred", e);
		}
	}

	public ProductGroupSortOrder GetSortOrder()
	{
		try
		{
			return _productGroupSortOrder;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("ProdcutGroupService.LoadProductGroupSortOrder - an exception occurred", e);
		}
		return null;
	}

	private void LoadProductGroupSortOrder()
	{
		try
		{
			ProductGroupSortOrder sortOrder = _kioskClientService.ProductGroups.GetSortOrder();
			if (sortOrder == null)
			{
				_logger.Log("ProductGroupService.LoadProductGroupSortOrder - kcs.ProductGroups.GetSortOrder return null");
				_productGroupSortOrderStaged = null;
			}
			else
			{
				_productGroupSortOrderStaged = sortOrder;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("ProdcutGroupService.LoadProductGroupSortOrder an exception occurred", e);
		}
	}
}
