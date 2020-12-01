Function FindSubContainerByMatch(_parent As Container, _name As String) As Container
	Dim _arr_childs As Array[Container]
	_parent.GetContainerAndSubContainers(_arr_childs, false)
	for _i=1 to _arr_childs.ubound
		if _arr_childs[_i].name.Match(_name) then
			FindSubContainerByMatch = _arr_childs[_i]
			Exit Function
		end if
	next
End Function
