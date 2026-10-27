namespace KioskClientService.API.Services.ProxyApi;

public static class KioskLoginErrorCodes
{
	public const string AccountLocked = "(KLI001)";

	public const string InvalidPasswordOrPin = "(KLI002)";

	public const string AccountNotFound = "(KLI003)";

	public const string CommunicationError = "(KLI004)";

	public const string LoyaltyIsDown = "(KLI005)";

	public const string LoyaltyNotFound = "(KLI006)";

	public const string SubscriptionServiceIsDown = "(KLI007)";

	public const string ApiError = "(KLI008)";
}
