using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class BoardVersionResponse : IBoardVersionResponse
{
	public bool ReadSuccess { get; private set; }

	public string Version { get; private set; }

	public string BoardName { get; private set; }

	public override string ToString()
	{
		if (!ReadSuccess)
		{
			return ErrorCodes.Timeout.ToString().ToUpper();
		}
		return Version;
	}

	internal BoardVersionResponse(ControlBoards board, CoreResponse response)
	{
		ReadSuccess = response.Success;
		Version = (ReadSuccess ? response.OpCodeResponse.Trim() : string.Empty);
		BoardName = board.ToString();
	}
}
