Function FindAllSubContainersByMatch(_parent As Container, _name As String) As Array[Container]
	Dim _arr_childs As Array[Container]
	_parent.GetContainerAndSubContainers(_arr_childs, false)
	_arr_childs.Erase(0)
	for _i=0 to _arr_childs.ubound
		if NOT _arr_childs[_i].name.Match(_name) then
			_arr_childs.Erase(_i)
			_i -= 1
		end if
	next
	FindAllSubContainersByMatch = _arr_childs
End Function