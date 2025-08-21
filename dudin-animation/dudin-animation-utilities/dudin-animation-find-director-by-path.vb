function GetDirectorByPath(_path As String, _createIfNotExist As Boolean) As Director
	Dim _arrDirNames As Array[String]
	Dim _foundDir As Director
	
	_path.trim()
	_path.split("/",_arrDirNames)
	_foundDir = null
	
	for i=0 to _arrDirNames.ubound
		_arrDirNames[i].trim()
		if _foundDir == null then
			_foundDir = Stage.FindDirector(_arrDirNames[i])
			if _createIfNotExist AND _foundDir == null then _foundDir = Stage.RootDirector.AddDirector(TL_NEXT)
		else
			Dim _nextFoundDir As Director = _foundDir.FindSubDirector(_arrDirNames[i])
			if _createIfNotExist AND _nextFoundDir == null then
				_nextFoundDir = _foundDir.AddDirector(TL_DOWN)
				_nextFoundDir.name = _arrDirNames[i]
			end if
			_foundDir = _nextFoundDir
		end if
	next
	
	GetDirectorByPath = _foundDir 
end function
