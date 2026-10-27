using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Reservation.DataFile;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Reservation;

public class ReservationDataInstance : IReservationDataInstance
{
	public static class Conversions
	{
		public static IReservation ConvertToReservation(ReservationDataFile_V002 reservationDataFile)
		{
			IReservation reservation = null;
			if (reservationDataFile != null)
			{
				reservation = new Reservation
				{
					HashedCardId = reservationDataFile.HashedCardId,
					Id = reservationDataFile.ID,
					ReferenceNumber = reservationDataFile.ReferenceNumber
				};
				ReservationDetails reservationDetails = new ReservationDetails
				{
					ReservationVersion = reservationDataFile.ReservationDataInfo.ReservationVersion,
					ReferenceNumber = reservationDataFile.ReservationDataInfo.ReferenceNumber,
					TransactionDate = reservationDataFile.ReservationDataInfo.TransactionDate,
					IsMultiNightPrice = reservationDataFile.ReservationDataInfo.IsMultiNightPrice,
					IsMultiDiscVend = reservationDataFile.ReservationDataInfo.IsMultiDiscVend,
					IsGiftCard = reservationDataFile.ReservationDataInfo.IsGiftCard,
					AuthorizeAtPickup = reservationDataFile.ReservationDataInfo.AuthorizeAtPickup,
					IsLoyaltyRedemption = reservationDataFile.ReservationDataInfo.IsLoyaltyRedemption,
					AppliedTitleMarketing = reservationDataFile.ReservationDataInfo.AppliedTitleMarketing,
					AppliedHiveOnlinePromo = reservationDataFile.ReservationDataInfo.AppliedHiveOnlinePromo,
					SubTotal = reservationDataFile.ReservationDataInfo.SubTotal,
					DiscountedSubTotal = reservationDataFile.ReservationDataInfo.DiscountedSubTotal,
					HiveOnlinePromoDiscount = reservationDataFile.ReservationDataInfo.HiveOnlinePromoDiscount,
					Tax = reservationDataFile.ReservationDataInfo.Tax,
					TaxRate = reservationDataFile.ReservationDataInfo.TaxRate,
					DefaultServiceFee = reservationDataFile.ReservationDataInfo.DefaultServiceFee,
					ActualServiceFee = reservationDataFile.ReservationDataInfo.ActualServiceFee,
					GrandTotal = reservationDataFile.ReservationDataInfo.GrandTotal,
					PromoCode = reservationDataFile.ReservationDataInfo.PromoCode,
					ZipCode = reservationDataFile.ReservationDataInfo.ZipCode,
					CustomerNumber = reservationDataFile.ReservationDataInfo.CustomerNumber,
					EmailAddress = reservationDataFile.ReservationDataInfo.EmailAddress
				};
				reservationDetails.HistoryTitleIds.AddRange(reservationDataFile.ReservationDataInfo.HistoryTitleIds);
				reservationDetails.CreditCardIds.AddRange(reservationDataFile.ReservationDataInfo.CreditCardIds);
				reservationDetails.GiftCardIds.AddRange(reservationDataFile.ReservationDataInfo.GiftCardIds);
				foreach (ReservationTitle_V001 reservationTitle in reservationDataFile.ReservationDataInfo.ReservationTitles)
				{
					ReservedTitle reservedTitle = new ReservedTitle
					{
						Barcode = reservationTitle.Barcode,
						TitleId = reservationTitle.TitleId,
						Discount = reservationTitle.Discount,
						DiscountedPrice = reservationTitle.DiscountedPrice,
						Price = reservationTitle.Price,
						LoyaltyPoints = reservationTitle.LoyaltyPoints,
						DiscountType = reservationTitle.DiscountType
					};
					if (Enum.TryParse<RentalShoppingCartItemAction>(reservationTitle.ReservationType.ToString(), ignoreCase: true, out var result))
					{
						reservedTitle.Action = result;
					}
					reservationDetails.ReservedTitles.Add(reservedTitle);
				}
				if (reservationDataFile.ReservationDataInfo.PricingSet != null)
				{
					reservationDetails.PricingSet = new PricingSet
					{
						PriceSetId = reservationDataFile.ReservationDataInfo.PricingSet.PriceSetId,
						Name = reservationDataFile.ReservationDataInfo.PricingSet.Name,
						ProgramName = reservationDataFile.ReservationDataInfo.PricingSet.ProgramName
					};
					if (reservationDataFile.ReservationDataInfo.PricingSet.PriceRecords != null)
					{
						foreach (ReservationPricingRecord_V001 priceRecord in reservationDataFile.ReservationDataInfo.PricingSet.PriceRecords)
						{
							Enum.TryParse<TitleFamily>(priceRecord.TitleFamily, out var result2);
							Enum.TryParse<TitleType>(priceRecord.TitleType, out var result3);
							IPricingRecord item = new PricingRecord
							{
								TitleFamily = result2,
								TitleType = result3,
								InitialNight = priceRecord.InitialNight,
								ExtraNight = priceRecord.ExtraNight,
								ExpirationPrice = priceRecord.ExpirationPrice,
								NonReturn = priceRecord.NonReturn,
								NonReturnDays = priceRecord.NonReturnDays
							};
							reservationDetails.PricingSet.PriceRecords.Add(item);
						}
					}
				}
				reservation.ReservationDetails = reservationDetails;
			}
			return reservation;
		}

		public static IReservationDataFile ConvertToReservationDataFile(IReservation reservation)
		{
			ReservationDataFile_V002 reservationDataFile_V = null;
			if (reservation != null)
			{
				reservationDataFile_V = new ReservationDataFile_V002
				{
					HashedCardId = reservation.HashedCardId,
					ID = reservation.Id,
					ReferenceNumber = reservation.ReferenceNumber
				};
				ReservationDataInfo_V001 reservationDataInfo_V = new ReservationDataInfo_V001
				{
					ReservationVersion = reservation.ReservationDetails.ReservationVersion,
					ReferenceNumber = reservation.ReservationDetails.ReferenceNumber,
					TransactionDate = reservation.ReservationDetails.TransactionDate,
					IsMultiNightPrice = reservation.ReservationDetails.IsMultiNightPrice,
					IsMultiDiscVend = reservation.ReservationDetails.IsMultiDiscVend,
					IsGiftCard = reservation.ReservationDetails.IsGiftCard,
					AuthorizeAtPickup = reservation.ReservationDetails.AuthorizeAtPickup,
					IsLoyaltyRedemption = reservation.ReservationDetails.IsLoyaltyRedemption,
					AppliedTitleMarketing = reservation.ReservationDetails.AppliedTitleMarketing,
					AppliedHiveOnlinePromo = reservation.ReservationDetails.AppliedHiveOnlinePromo,
					SubTotal = reservation.ReservationDetails.SubTotal,
					DiscountedSubTotal = reservation.ReservationDetails.DiscountedSubTotal,
					HiveOnlinePromoDiscount = reservation.ReservationDetails.HiveOnlinePromoDiscount,
					Tax = reservation.ReservationDetails.Tax,
					TaxRate = reservation.ReservationDetails.TaxRate,
					DefaultServiceFee = reservation.ReservationDetails.DefaultServiceFee,
					ActualServiceFee = reservation.ReservationDetails.ActualServiceFee,
					GrandTotal = reservation.ReservationDetails.GrandTotal,
					PromoCode = reservation.ReservationDetails.PromoCode,
					ZipCode = reservation.ReservationDetails.ZipCode,
					CustomerNumber = reservation.ReservationDetails.CustomerNumber,
					EmailAddress = reservation.ReservationDetails.EmailAddress
				};
				reservationDataInfo_V.HistoryTitleIds.AddRange(reservation.ReservationDetails.HistoryTitleIds);
				reservationDataInfo_V.CreditCardIds.AddRange(reservation.ReservationDetails.CreditCardIds);
				reservationDataInfo_V.GiftCardIds.AddRange(reservation.ReservationDetails.GiftCardIds);
				foreach (IReservedTitle reservedTitle in reservation.ReservationDetails.ReservedTitles)
				{
					ReservationTitle_V001 reservationTitle_V = new ReservationTitle_V001
					{
						Barcode = reservedTitle.Barcode,
						TitleId = reservedTitle.TitleId,
						Discount = reservedTitle.Discount,
						DiscountedPrice = reservedTitle.DiscountedPrice,
						Price = reservedTitle.Price,
						LoyaltyPoints = reservedTitle.LoyaltyPoints,
						DiscountType = reservedTitle.DiscountType
					};
					if (Enum.TryParse<ReservationType_V001>(reservedTitle.Action.ToString(), ignoreCase: true, out var result))
					{
						reservationTitle_V.ReservationType = result;
					}
					reservationDataInfo_V.ReservationTitles.Add(reservationTitle_V);
				}
				if (reservation.ReservationDetails.PricingSet != null)
				{
					reservationDataInfo_V.PricingSet = new ReservationPricingSet_V001
					{
						PriceSetId = reservation.ReservationDetails.PricingSet.PriceSetId,
						Name = reservation.ReservationDetails.PricingSet.Name,
						ProgramName = reservation.ReservationDetails.PricingSet.ProgramName
					};
					if (reservation.ReservationDetails.PricingSet.PriceRecords != null && reservation.ReservationDetails.PricingSet.PriceRecords.Count > 0)
					{
						reservationDataInfo_V.PricingSet.PriceRecords = new List<ReservationPricingRecord_V001>();
						foreach (IPricingRecord priceRecord in reservation.ReservationDetails.PricingSet.PriceRecords)
						{
							ReservationPricingRecord_V001 item = new ReservationPricingRecord_V001
							{
								TitleFamily = priceRecord.TitleFamily.ToString(),
								TitleType = priceRecord.TitleType.ToString(),
								InitialNight = priceRecord.InitialNight,
								ExtraNight = priceRecord.ExtraNight,
								ExpirationPrice = priceRecord.ExpirationPrice,
								NonReturn = priceRecord.NonReturn,
								NonReturnDays = priceRecord.NonReturnDays
							};
							reservationDataInfo_V.PricingSet.PriceRecords.Add(item);
						}
					}
				}
				reservationDataFile_V.ReservationDataInfo = reservationDataInfo_V;
			}
			return reservationDataFile_V;
		}

		internal static List<ReservationDataFile_V002> ConvertReservationDataFileV001ToV002Files(IReservationDataFile inputDataFile)
		{
			List<ReservationDataFile_V002> list = new List<ReservationDataFile_V002>();
			if (inputDataFile is ReservationDataFile_V001 reservationDataFile_V)
			{
				foreach (ReservationDataInfo_V001 reservationDataInfo in reservationDataFile_V.ReservationDataInfos)
				{
					ReservationDataFile_V002 item = new ReservationDataFile_V002
					{
						HashedCardId = reservationDataFile_V.HashedCardId,
						ID = Guid.NewGuid(),
						ReferenceNumber = reservationDataInfo.ReferenceNumber,
						ReservationDataInfo = reservationDataInfo
					};
					list.Add(item);
				}
			}
			return list;
		}
	}

	private Dictionary<string, IReservationDataFile> _dataFiles = new Dictionary<string, IReservationDataFile>();

	private IDataFileStoreService _dataFileStoreService;

	public Dictionary<string, IReservationDataFile> DataFiles
	{
		get
		{
			return _dataFiles;
		}
		set
		{
			_dataFiles = value;
		}
	}

	public ReservationDataInstance(IDataFileStoreService dataFileStoreService)
	{
		_dataFileStoreService = dataFileStoreService;
	}

	public bool Save(IReservationDataFile reservationDataFile)
	{
		bool result = false;
		if (reservationDataFile != null)
		{
			result = _dataFileStoreService.Set(reservationDataFile.FileName, reservationDataFile);
		}
		return result;
	}

	public void Delete(IReservationDataFile reservationDataFile)
	{
		_dataFileStoreService?.Delete(reservationDataFile.FileName);
	}

	public void LoadAll()
	{
		if (_dataFileStoreService == null)
		{
			return;
		}
		List<IReservationDataFile> all = _dataFileStoreService.GetAll<IReservationDataFile>();
		if (all == null)
		{
			return;
		}
		foreach (IReservationDataFile item in all)
		{
			if (!(item is ReservationDataFile_V002 reservationDataFile_V))
			{
				bool flag = false;
				List<ReservationDataFile_V002> list = Conversions.ConvertReservationDataFileV001ToV002Files(item);
				if ((list?.Count ?? 0) > 0)
				{
					flag = true;
					foreach (ReservationDataFile_V002 item2 in list)
					{
						DataFiles[item2.ReferenceNumber] = item2;
						flag = flag && Save(item2);
					}
				}
				if (flag)
				{
					Delete(item);
				}
				else
				{
					LogHelper.Instance.Log("Unable to convert reservation data file " + item.FileName);
				}
			}
			else
			{
				DataFiles[reservationDataFile_V.ReferenceNumber] = reservationDataFile_V;
			}
		}
	}
}
