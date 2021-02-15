RegisterPluginVersion(1,3,0)
Dim this_normal, normal, to_camera As Vertex
Dim angle, out_alpha, besier_hidden, besier_visible, angle_visible, angle_hidden As Double
Dim inverse, mirror_around_90 As Boolean
Dim PI As Double = 3.1415926535

Dim axes_names As Array[String]
axes_names.Push(" X ")
axes_names.Push(" Y ")
axes_names.Push(" Z ")
Dim mode_names As Array[String]
mode_names.Push(" Linear ")
mode_names.Push(" Smooth ")
sub OnInitParameters()
	RegisterRadioButton("axis", "Direction Axis", 2, axes_names)
	RegisterParameterDouble("angle_visible", "Visible angle", 0, 0, 180)
	RegisterParameterDouble("angle_hidden", "Invisible angle", 90, 0, 180)
	RegisterParameterBool("mirror_around_90", "Mirror around 90", false)
	RegisterRadioButton("mode", "Mode", 0, mode_names)
	RegisterParameterDouble("besier_visible", "Easy visible", 100, 30, 100)
	RegisterParameterDouble("besier_hidden", "Easy hiden", 30, 30, 100)
	RegisterParameterBool("inverse", "Inverse", false)
end sub

sub OnInit()
	this_normal = CVertex( CInt(GetParameterInt("axis")==0), CInt(GetParameterInt("axis")==1), CInt(GetParameterInt("axis")==2) )
	angle_visible = GetParameterDouble("angle_visible")
	angle_hidden = GetParameterDouble("angle_hidden")
	mirror_around_90 = GetParameterBool("mirror_around_90")
	besier_hidden = GetParameterDouble("besier_hidden")
	besier_visible = GetParameterDouble("besier_visible")
	inverse = GetParameterBool("inverse")
	SendGuiParameterShow("besier_hidden", CInt(GetParameterInt("mode") == 1))
	SendGuiParameterShow("besier_visible", CInt(GetParameterInt("mode") == 1))
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	normal = RotateVertexByMatrix(this.Matrix, this_normal)
	to_camera = Scene.CurrentCamera.Position.xyz - this.LocalPosToWorldPos(this.position.xyz)
	angle = AngleBetweenVectors(normal, to_camera)*180.0/PI
	
	if mirror_around_90 and angle > 90 then
		angle = 180 - angle
	end if
	
	out_alpha = LinearWithLimits(angle, angle_hidden, angle_visible, 0, 100.0)   ' normilize and limit to [0..100] range
	if inverse then out_alpha = 100 - out_alpha
	if GetParameterInt("mode")==0 then
		this.Alpha.Value = out_alpha
	elseif GetParameterInt("mode")==1 then
		this.Alpha.Value = Besizer(out_alpha, 0, 100, besier_hidden, besier_visible)
	end if
end sub

''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Function LinearWithLimits(input As Double, min_input As Double, max_input As Double, min_out As Double, max_out As Double) As Double
	Dim a, b, y As Double
	a = (max_out - min_out)/(max_input - min_input)
	b = min_out - a * min_input
	y = a*input + b
	if a == 0 then
		if input >= max_input then y = max_out
		if input <  min_input then y = min_out
	else
		if y > max_out then y = max_out
		if y < min_out then y = min_out
	end if
	LinearWithLimits = y
End Function

'CLAMP
Function ClampDbl(value as double, min  as double, max as double) as Double
	if value < min then value = min
	if value > max then value = max
	ClampDbl = value
End Function

'BESIER
Function Besizer(ByVal procent as double, ByVal begin_value as double, ByVal end_value as double, ByVal begin_weight as double, ByVal end_weight as double) as Double
	Dim a, b, c, d, t_besier_value As Double
	procent      = ClampDbl(procent,       0, 100)/100.0
	begin_weight = ClampDbl(begin_weight, 35, 100)/100.0
	end_weight   = ClampDbl(end_weight,   35, 100)/100.0
	
	a = 3*begin_weight - 3*(1.0 - end_weight) + 1
	b = - 6*begin_weight + 3*(1.0 - end_weight)
	c = 3*begin_weight
	d = -procent
	
	t_besier_value = (sqrt((-27*a^2*d + 9*a*b*c - 2*b^3)^2 + 4*(3*a*c - b^2)^3) - 27*a^2*d + 9*a*b*c - 2*b^3)^(1.0/3)/(3*2^(1.0/3)*a) - (2^(1.0/3)*(3*a*c - b^2))/(3*a*(sqrt((-27*a^2*d + 9*a*b*c - 2*b^3)^2 + 4*(3*a*c - b^2)^3) - 27*a^2*d + 9*a*b*c - 2*b^3)^(1.0/3)) - b/(3*a)
	Besizer = begin_value + (end_value - begin_value)*( 3*(1-t_besier_value)*t_besier_value^2 + t_besier_value^3 ) 
End Function

Function RotateVertexByMatrix(m As Matrix, v As Vertex) As Vertex
	Dim new_vertex As Vertex
	Dim new_matrix As Matrix
	
	new_matrix.Rotate(m.GetRotation())
	m = new_matrix

	new_vertex.x = m[0]*v.x + m[4]*v.y + m[8]*v.z + m[12]
	new_vertex.y = m[1]*v.x + m[5]*v.y + m[9]*v.z + m[13]
	new_vertex.z = m[2]*v.x + m[6]*v.y + m[10]*v.z + m[14]
	RotateVertexByMatrix = new_vertex
End Function
