RegisterPluginVersion(1,1,0)
Dim distance, alfa As Double
Dim dist_min, dist_max As Double
Dim inverse As Boolean
Dim mode As Integer
Dim easy_near, easy_far As Double

Dim mode_names As Array[String]
mode_names.Push("Linear")
mode_names.Push("Smooth")
sub OnInitParameters()
	RegisterParameterDouble("dist_min", "Distance near clamp", 0, 0, 999999999)
	RegisterParameterDouble("dist_max", "Distance far clamp", 1000, 0, 999999999)
	RegisterParameterBool("inverse", "Inverse", false)
	RegisterRadioButton("mode", "Mode", 0, mode_names)
	RegisterParameterDouble("easy_near", "Easy near power", 50, 30, 100)
	RegisterParameterDouble("easy_far", "Easy far power", 50, 30, 100)
end sub

sub OnInit()
	dist_min = GetParameterDouble("dist_min")
	dist_max = GetParameterDouble("dist_max")
	inverse = GetParameterBool("inverse")
	mode = GetParameterInt("mode")
	easy_near = GetParameterDouble("easy_near")
	easy_far = GetParameterDouble("easy_far")
	SendGuiParameterShow("easy_near", GetParameterInt("mode"))
	SendGuiParameterShow("easy_far", GetParameterInt("mode"))
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	distance = Distance(this.LocalPosToWorldPos(this.position.xyz), Scene.CurrentCamera.Position.xyz)
	alfa = LinearWithLimits(distance, dist_min, dist_max, 0, 100)
	if inverse then alfa = 100 - alfa
	if mode == 1 then
		alfa = Besizer(alfa, 0, 100, easy_near, easy_far)
	end if
	this.alpha.value = alfa
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
