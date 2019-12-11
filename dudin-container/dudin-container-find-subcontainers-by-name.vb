Function GetSubContainerByName(_c As Container, _name As String) As Array[Container]
	Dim _arr As Array[Container]
	_c.GetContainerAndSubContainers(_arr, false)
	_arr.Erase(0)
	for i=0 to _arr.ubound
		if _arr[i].name <> _name then
			_arr.Erase(i)
			i += 1
		end if
	next
	GetSubContainerByName = _arr
End Function
