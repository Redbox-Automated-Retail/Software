namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class KioskClientServiceTransactionConstants
{
	public class TestData
	{
		public class Authorize
		{
			public const string LiveCall = "Authorize.LiveCall";

			public const string SimulateCommunicationFailure = "Authorize.SimulateCommunicationFailure";

			public const string ErrorCode = "Authorize.ErrorCode";

			public const string HasCustomerProfile = "Authorize.HasCustomerProfile";

			public const string AccountNumber = "Authorize.AccountNumber";

			public const string CustomerProfileNumber = "Authorize.CustomerProfileNumber";

			public const string CustomerProfileFirstName = "Authorize.CustomerProfileFirstName";

			public const string CustomerProfileLastName = "Authorize.CustomerProfileLastName";

			public const string Email = "Authorize.Email";

			public const string ReferenceNumber = "Authorize.ReferenceNumber";

			public const string ConfirmationStatus = "Authorize.ConfirmationStatus";
		}

		public class GCAuth
		{
			public const string LiveCall = "GCAuth.LiveCall";

			public const string SimulateCommunicationFailure = "GCAuth.SimulateCommunicationFailure";

			public const string ErrorCode = "GCAuth.ErrorCode";

			public const string GCResponseType = "GCAuth.GCResponseType";

			public const string Exists = "GCAuth.Exists";

			public const string GCStatusType = "GCAuth.GCStatusType";

			public const string Balance = "GCAuth.Balance";

			public const string MaxDiscs = "GCAuth.MaxDiscs";
		}

		public class AfterSwipeCustomerOffers
		{
			public enum Offers
			{
				NoOffer,
				InCartOffers,
				FracFreeMovieFirstVisit,
				FracFreeMovieSecondVisit,
				FracDiscountedDiscFirstVisit,
				FracDiscountedDiscSecondVisit,
				FreeItemPerksEquivalent,
				AmountOffPerksEquivalent,
				ExtraPointsPerksEquivalent,
				PerksMessage
			}

			public const string LiveCall = "AfterSwipeCustomerOffers.LiveCall";

			public const string Success = "AfterSwipeCustomerOffers.Success";

			public const string SelectedOffer = "AfterSwipeCustomerOffers.SelectedOffer";

			public const string IncludeMovieInCartOffer = "AfterSwipeCustomerOffers.IncludeMovieInCartOffer";

			public const string IncludeFreeGameInCartOffer = "AfterSwipeCustomerOffers.IncludeFreeGameInCartOffer";

			public const string IncludeAmountOffPurchaseInCartOffer = "AfterSwipeCustomerOffers.IncludeAmountOffPurchaseInCartOffer";

			public const string IncludeMultiNightPriceInCartOffer = "AfterSwipeCustomerOffers.IncludeMultiNightPriceInCartOffer";

			public const string MultiNightPriceProductIds = "AfterSwipeCustomerOffers.MultiNightPriceProductIds";
		}

		public const string CategoryName = "TransactionService";
	}
}
