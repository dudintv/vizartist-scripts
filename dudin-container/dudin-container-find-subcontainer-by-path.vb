Function FindSubContainerByPath(ByVal _c As Container, _path as String) As Container
	Dim _cCurrent As Container
	Dim _arrsSubContainerPath As Array[String]
	_path.split(".", _arrsSubContainerPath)
	for _pathIndex=0 to _arrsSubContainerPath.ubound
		_arrsSubContainerPath[_pathIndex].Trim()
		if _arrsSubContainerPath[_pathIndex] == "" then _arrsSubContainerPath.Erase(_pathIndex)
	next
	for _pathIndex=0 to _arrsSubContainerPath.ubound
		_cCurrent = _c.FindSubContainer(_arrsSubContainerPath[_pathIndex])
		if _cCurrent == null then
			FindSubContainerByPath = _c
			Exit Function
		end if
		_c = _cCurrent
	next
	FindSubContainerByPath = _c
End Function
