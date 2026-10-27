using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services;

internal sealed class ClientBoardRevision : IBoardVersionResponse
{
	public bool ReadSuccess { get; private set; }

	public string Version { get; private set; }

	public string BoardName { get; private set; }

	internal ClientBoardRevision(ControlBoards board, string response)
	{
		BoardName = board.ToString().ToUpper();
		ReadSuccess = !response.Equals("TIMEOUT", StringComparison.CurrentCultureIgnoreCase);
		Version = response;
	}
}
