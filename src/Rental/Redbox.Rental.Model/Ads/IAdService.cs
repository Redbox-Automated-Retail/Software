using System.Threading.Tasks;

namespace Redbox.Rental.Model.Ads;

public interface IAdService
{
	IAd GetScreenAd(AdLocation AdLocation);

	Task<IAd> GetScreenAdAsync(AdLocation AdLocation);

	bool RecordAdImpression(IAdImpression adImpression);

	Task<bool> RecordAdImpressionAsync(IAdImpression adImpression);
}
