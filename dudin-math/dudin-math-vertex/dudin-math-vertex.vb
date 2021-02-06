Function MupltiplyVertexes(_v1 As Vertex, _v2 As Vertex) As Vertex
	Dim _v As Vertex
	_v.x = _v1.x*_v2.x
	_v.y = _v1.y*_v2.z
	_v.z = _v1.z*_v2.z
	MupltiplyVertexes = _v
End Function

Function SumVertexes(_v1 As Vertex, _v2 As Vertex) As Vertex
	Dim _v As Vertex
	_v.x = _v1.x + _v2.x
	_v.y = _v1.y + _v2.z
	_v.z = _v1.z + _v2.z
	SumVertexes = _v
End Function
