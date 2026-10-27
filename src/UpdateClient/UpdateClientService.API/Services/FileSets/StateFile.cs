namespace UpdateClientService.API.Services.FileSets;

public class StateFile
{
	private long _fileSetId;

	private long _activeRevisionId;

	private long _inProgressRevisionId;

	private FileSetState _inProgressFileSetState;

	public long FileSetId => _fileSetId;

	public long ActiveRevisionId => _activeRevisionId;

	public long InProgressRevisionId
	{
		get
		{
			return _inProgressRevisionId;
		}
		set
		{
			_inProgressRevisionId = value;
		}
	}

	public long CurrentRevisionId
	{
		get
		{
			if (!IsRevisionDownloadInProgress)
			{
				return _activeRevisionId;
			}
			return _inProgressRevisionId;
		}
	}

	public FileSetState InProgressFileSetState
	{
		get
		{
			return _inProgressFileSetState;
		}
		set
		{
			_inProgressFileSetState = value;
			if (_inProgressFileSetState == FileSetState.Active)
			{
				_activeRevisionId = _inProgressRevisionId;
				_inProgressRevisionId = 0L;
			}
		}
	}

	public FileSetState CurrentFileSetState
	{
		get
		{
			if (!IsRevisionDownloadInProgress)
			{
				return FileSetState.Active;
			}
			return _inProgressFileSetState;
		}
	}

	public bool IsRevisionDownloadInProgress => _inProgressRevisionId > 0;

	public bool HasActiveRevision => _activeRevisionId > 0;

	public StateFile(long fileSetId, long activeRevisionId, long inProgressRevisionId, FileSetState inProgressFileSetState)
	{
		_fileSetId = fileSetId;
		_activeRevisionId = activeRevisionId;
		_inProgressRevisionId = inProgressRevisionId;
		_inProgressFileSetState = inProgressFileSetState;
	}
}
