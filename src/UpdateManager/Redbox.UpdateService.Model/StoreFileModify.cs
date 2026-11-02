namespace Redbox.UpdateService.Model;

public class StoreFileModify
{
	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public StoreFileInfo StoreFileInfo { get; set; } = null!;

	public StoreFileAdd StoreFileAdd { get; set; } = null!;
}
