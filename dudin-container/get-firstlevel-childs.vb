Sub GetFirstLevelChilds(root as Container, ByRef _arr_childs As Array[Container])
	_arr_childs.Clear()
	Dim _c As Container
	_c = this.FirstChildContainer
	Do 
		_arr_childs.Push(c)
		_c = _c.NextContainer
	Loop While _c <> null
End Sub
