using System;

namespace Redbox.Rental.Model.Personalization;

public interface IAcceptedOffer
{
	string Code { get; set; }

	string Status { get; set; }

	DateTime? StartDate { get; set; }

	DateTime? EndDate { get; set; }

	string Name { get; set; }

	string Description { get; set; }

	string Partner { get; set; }

	DateTime? OptedInDate { get; set; }

	string TimeToComplete { get; set; }

	DateTime? CompleteDate { get; set; }

	int? ShowInBannerPriority { get; set; }

	string LegalInformation { get; set; }

	string SocialMediaDescription { get; set; }

	string ProgressTrackerCode { get; set; }

	int? CurrentValue { get; set; }

	int? MaxValue { get; set; }

	string EndValueText { get; set; }

	int? RemainderValue { get; set; }
}
