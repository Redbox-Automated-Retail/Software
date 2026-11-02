namespace Redbox.UpdateService.Model;

public class InterServerMessage
{
	public InterServerTask Task { get; set; } = null!; // set by the constructor that takes it, or the JSON reader

	public Server Server { get; set; }

	public InterServerMessage(InterServerTask task, Server server)
	{
		Task = task;
		Server = server;
	}

	public InterServerMessage()
	{
	}
}
