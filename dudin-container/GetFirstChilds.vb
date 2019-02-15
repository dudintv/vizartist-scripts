Sub GetFirstChilds(root as Container, ByRef arr_childs As Array[Container])
	arr_childs.Clear()
	Dim _c As Container
	_c = this.FirstChildContainer
	Do 
		arr_childs.Push(c)
		_c = _c.NextContainer
	Loop While _c <> null
End Sub