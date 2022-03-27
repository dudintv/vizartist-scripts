RegisterPluginVersion(1,0,1)

Dim c_origin, c_target As Container
Dim prev_v_origin, prev_v_target, new_v_origin, new_v_target As Vertex
Dim up, v_lookat, v_tangent As Vertex
Dim rot_matrix As Matrix
Dim look_axis As Integer
Dim dist, scale, scale_mult As Double
Dim scale_to_target, uniform_scale, set_rest_scale_to_1 As Boolean

sub OnInitParameters()
	RegisterParameterContainer("origin", "Origin")
	RegisterParameterContainer("target", "Target")
	Dim buttonNames As Array[String]
	buttonNames.push("X")
	buttonNames.push("Y")
	buttonNames.push("Z")
	RegisterRadioButton("look_axis", "Look axis", 1, buttonNames)
	RegisterParameterBool("scale_to_target", "Scale to target", false)
	RegisterParameterBool("uniform_scale", "Uniform scale", false)
	RegisterParameterBool("set_rest_scale_to_1", "Set rest scale axis to 1", false)
	RegisterParameterDouble("scale_mult", "Multiply scale, %", 100.0, -999999.0, 999999.0)
end sub

sub OnInit()
	c_origin = GetParameterContainer("origin")
	c_target = GetParameterContainer("target")
	look_axis = GetParameterInt("look_axis")
	scale_to_target = GetParameterBool("scale_to_target")
	scale_mult = GetParameterDouble("scale_mult")
	uniform_scale = GetParameterBool("uniform_scale")
	set_rest_scale_to_1 = GetParameterBool("set_rest_scale_to_1")
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

Function AreTwoVerticesEqual(_v1 As Vertex, _v2 As Vertex) As Boolean
	if _v1.x <> _v2.x then 
		AreTwoVerticesEqual = false
		exit function
	end if
	if _v1.y <> _v2.y then 
		AreTwoVerticesEqual = false
		exit function
	end if
	if _v1.z <> _v2.z then 
		AreTwoVerticesEqual = false
		exit function
	end if
	AreTwoVerticesEqual = true
End Function

sub OnExecPerField()
	new_v_origin = c_origin.LocalPosToWorldPos(c_origin.position.xyz)
	new_v_target = c_target.LocalPosToWorldPos(c_target.position.xyz)
	if AreTwoVerticesEqual(prev_v_origin, new_v_origin) AND AreTwoVerticesEqual(prev_v_target, new_v_target) then		
		exit sub		
	end if
	prev_v_origin = new_v_origin
	prev_v_target = new_v_target

	this.position.xyz = this.WorldPosToLocalPos(prev_v_origin)
	this.RecomputeMatrix()
	LootAt()
	
	if scale_to_target then
		Dim v_origin = this.position.xyz
		Dim v_target = GetPosWithinAnotherContainer(c_target, this.ParentContainer)
		dist = Distance(v_origin, v_target)
		println(dist)
		scale = dist * scale_mult /100.0 /100.0
		if uniform_scale then
			this.scaling.xyz = CVertex(scale, scale, scale)
		else
			if look_axis == 0 then
				this.scaling.x = scale
				if set_rest_scale_to_1 then
					this.scaling.y = 1.0
					this.scaling.z = 1.0
				end if
			elseif look_axis == 1 then
				this.scaling.y = scale
				if set_rest_scale_to_1 then
					this.scaling.x = 1.0
					this.scaling.z = 1.0
				end if
			elseif look_axis == 2 then
				this.scaling.z = scale
				if set_rest_scale_to_1 then
					this.scaling.x = 1.0
					this.scaling.y = 1.0
				end if
			end if
		end if
	end if
end sub

Sub LootAt()
	v_lookat = GetPosWithinAnotherContainer(c_target, this.ParentContainer) - this.position.xyz
	rot_matrix = GetRotationMatrix(v_lookat)	
	this.rotation.xyz = GetRotationFromMatrix(rot_matrix)
End Sub

Function GetRotationMatrix(lookat As Vertex) as Matrix
	up = CVertex(0,0,1)
	Dim m As Matrix
	m.LoadIdentity()
	
	lookat.Normalize()
	Dim xaxis = up cross lookat
	if xaxis.length < 0.0001 then
		up = CVertex(1,0,0)
		xaxis = up cross lookat
	end if
	xaxis.Normalize()
	
	Dim yaxis = lookat cross xaxis
	Dim zaxis = lookat
	
	if look_axis == 0 then
		' X
		'first column 0,1,2,3:
		m[0] = zaxis.x
		m[1] = zaxis.y
		m[2] = zaxis.z
		
		'second column 4,5,6,7:
		m[4] = xaxis.x
		m[5] = xaxis.y
		m[6] = xaxis.z
		
		'third column 8,9,10,11:
		m[8] = yaxis.x
		m[9] = yaxis.y
		m[10] = yaxis.z
	elseif look_axis == 1 then
		' Y
		'first column 0,1,2,3:
		m[0] = yaxis.x
		m[1] = yaxis.y
		m[2] = yaxis.z
		
		'second column 4,5,6,7:
		m[4] = zaxis.x
		m[5] = zaxis.y
		m[6] = zaxis.z
		
		'third column 8,9,10,11:
		m[8] = xaxis.x
		m[9] = xaxis.y
		m[10] = xaxis.z
	elseif look_axis == 2 then
		' Z
		'first column 0,1,2,3:
		m[0] = xaxis.x
		m[1] = xaxis.y
		m[2] = xaxis.z
		
		'second column 4,5,6,7:
		m[4] = yaxis.x
		m[5] = yaxis.y
		m[6] = yaxis.z
		
		'third column 8,9,10,11:
		m[8] = zaxis.x
		m[9] = zaxis.y
		m[10] = zaxis.z
	end if
	'fourth colum is 12,13,14,15 (no need to calc)
	
	GetRotationMatrix = m
end function

Function GetRotationFromMatrix(_m As Matrix) As Vertex
	Dim _v As Vertex
	_v.x = atan2(-_m[9], _m[10])
	Dim cosY = Sqrt(_m[0]*_m[0] + _m[4]*_m[4])
	_v.y = atan2(_m[8], cosY)
	Dim sinX = sin(_v.x)
	Dim cosX = cos(_v.x)
	_v.z = atan2(cosX*_m[1] + sinX*_m[2], cosX*_m[5] + sinX*_m[6])
	
	'degrees = radians * (180/pi)
	_v.x = _v.x * (180/3.1415926535)
	_v.y = _v.y * (180/3.1415926535)
	_v.z = _v.z * (180/3.1415926535)
	GetRotationFromMatrix = _v
End Function

Function CrossVectors(v1 As Vertex, v2 As Vertex) As Vertex
	v1.Normalize()
	v2.Normalize()
	Dim cx, cy, cz As Double
	cx = v1.y*v2.z - v1.z*v2.y
	cy = v1.z*v2.x - v1.x*v2.z
	cz = v1.x*v2.y - v1.y*v2.x
	CrossVectors = CVertex(cx, cy, cz)
End Function

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

Function GetPosWithinAnotherContainer(_c_pos As Container, _c_con As Container) As Vertex
	Dim _m As Matrix = _c_con.matrix
	_m.Invert()
	Dim _v_glob_pos As Vertex = GetGlobalPos(_c_pos)
	_v_glob_pos *= _m
	GetPosWithinAnotherContainer = _v_glob_pos
End Function
