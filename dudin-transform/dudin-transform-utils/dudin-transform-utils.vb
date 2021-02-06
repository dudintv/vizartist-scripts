Function GetGlobalPos(_c As Container) As Vertex
	if _c.ParentContainer <> null then
		Dim _v as Vertex = _c.position.xyz
		_v *= _c.ParentContainer.matrix
		GetGlobalPos = _v
	elseif _c <> null then
		GetGlobalPos = _c.position.xyz
	else
		GetGlobalPos = CVertex(0,0,0)
	end if
End Function

Function GetGlobalBountingBox(_c As Container, ByRef _v1 As Vertex, ByRef _v2 As Vertex) As Vertex
	if _c <> null then
		_c.GetBoundingBox(_v1, _v2)
		_v1 *= _c.matrix
		_v2 *= _c.matrix
		GetGlobalBountingBox = _v2 - _v1
	else
		GetGlobalBountingBox = CVertex(0,0,0)
	end if
End Function

Sub SetPosAsGlobal(_c As Container, ByVal _v As Vertex)
	Dim _m As Matrix
	if _c.ParentContainer <> null then
		_m = _c.ParentContainer.matrix
		_m.Invert()
		_v *= _m
		_c.position.xyz = _v
	else
		_c.position.xyz = _v
	end if
End Sub

' -------------------------------------------------

Function GetPosWithinAnotherContainer(_c_pos As Container, _c_con As Container) As Vertex
	Dim _m As Matrix = _c_con.matrix
	_m.Invert()
	Dim _v_glob_pos As Vertex = GetGlobalPos(_c_pos)
	_v_glob_pos *= _m
	GetPosWithinAnotherContainer = _v_glob_pos
End Function

Function GetBountingBoxWithinAnotherContainer(_c_bb As Container, _c_con As Container, ByRef _v1 As Vertex, ByRef _v2 As Vertex) As Vertex
	if _c_bb <> null then
		_c_bb.GetBoundingBox(_v1, _v2)
		_v1 *= _c_bb.matrix
		_v2 *= _c_bb.matrix
		Dim _m As Matrix = _c_con.matrix
		_m.Invert()
		_v1 *= _m
		_v2 *= _m
		GetBountingBoxWithinAnotherContainer = _v2 - _v1
	else
		GetBountingBoxWithinAnotherContainer = CVertex(0,0,0)
	end if
End Function

Function ProjectVertexFromOneContainerToAnother(ByVal _v As Vertex, _c_from As Container, _c_to As Container) As Vertex
	Dim _m_from As Matrix = _c_from.matrix
	Dim _m_to As Matrix = _c_to.matrix
	_m_to.Invert()
	
	_v *= _c_from.matrix
	_v *= _m_to
	ProjectVertexFromOneContainerToAnother = _v
End Function