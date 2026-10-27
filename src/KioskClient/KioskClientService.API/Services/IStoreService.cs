namespace KioskClientService.API.Services;

public interface IStoreService
{
	long KioskId { get; }

	string DataPath { get; }

	string RunningPath { get; }
}
