Sub GetAllContainersInScene(_arrc As Array[Container])
	_arrc.Clear()
	Dim _c As Container = Scene.RootContainer
	Dim _arrcChilds As Array[Container]
	Do
		_c.GetContainerAndSubContainers(_arrcChilds, false)
		for _i=0 to _arrcChilds.ubound
			_arrc.Push(_arrcChilds[_i])
		next
		_c = _c.NextContainer
	Loop While _c <> null
End Sub
