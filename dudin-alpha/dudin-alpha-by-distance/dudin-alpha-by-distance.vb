RegisterPluginVersion(1,3,0)
Dim distance, alfa As Double
Dim dist_min, dist_max As Double
Dim inverse As Boolean
Dim dist_mode, anim_mode As Integer
Dim easy_near, easy_far As Double

Dim mode_anim_names As Array[String]
mode_anim_names.Push("Linear")
mode_anim_names.Push("Smooth")

Dim mode_dist_names As Array[String]
mode_dist_names.Push("Center")
mode_dist_names.Push("BBox")
Dim DIST_MODE_CENTER As Integer = 0
Dim DIST_MODE_BB As Integer = 1

sub OnInitParameters()
	RegisterRadioButton("dist_mode", "Distance between camera and", 0, mode_dist_names)
	RegisterParameterDouble("dist_min", "Distance near clamp", 0, 0, 999999999)
	RegisterParameterDouble("dist_max", "Distance far clamp", 1000, 0, 999999999)
	RegisterRadioButton("anim_mode", "Mode", 0, mode_anim_names)
	RegisterParameterDouble("easy_near", "Easy near power", 50, 30, 100)
	RegisterParameterDouble("easy_far", "Easy far power", 50, 30, 100)
	RegisterParameterBool("inverse", "Inverse", false)
end sub

sub OnInit()
	dist_mode = GetParameterInt("dist_mode")
	dist_min = GetParameterDouble("dist_min")
	dist_max = GetParameterDouble("dist_max")
	inverse = GetParameterBool("inverse")
	anim_mode = GetParameterInt("anim_mode")
	easy_near = GetParameterDouble("easy_near")
	easy_far = GetParameterDouble("easy_far")
	SendGuiParameterShow("easy_near", GetParameterInt("anim_mode"))
	SendGuiParameterShow("easy_far", GetParameterInt("anim_mode"))
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	if dist_mode == DIST_MODE_BB then
		distance = DistanceToBB(Scene.CurrentCamera.Position.xyz, this)
	else
		distance = Distance(Scene.CurrentCamera.Position.xyz, this.LocalPosToWorldPos(this.position.xyz))
	end if
	alfa = LinearWithLimits(distance, dist_min, dist_max, 0, 100)
	if inverse then alfa = 100 - alfa
	if anim_mode == 1 then
		alfa = Besizer(alfa, 0, 100, easy_near, easy_far)
	end if
	this.alpha.value = alfa
end sub

''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''


Dim v1, v2 As Vertex
Dim v_dists As Vertex
Dim updated_point As Vertex
Dim inverted_matrix As Matrix
Function DistanceToBB(point As Vertex, c As Container) As Double
	inverted_matrix = c.Matrix
	inverted_matrix.Invert()
	updated_point = point
	updated_point *= inverted_matrix
	c.GetBoundingBox(v1, v2)
	v_dists = CVertex(-1, -1, -1)
	if updated_point.x < v1.x then v_dists.x = v1.x - updated_point.x
	if updated_point.x > v2.x then v_dists.x = updated_point.x - v2.x
	if updated_point.y < v1.y then v_dists.y = v1.y - updated_point.y
	if updated_point.y > v2.y then v_dists.y = updated_point.y - v2.y
	if updated_point.z < v1.z then v_dists.z = v1.z - updated_point.z
	if updated_point.z > v2.z then v_dists.z = updated_point.z - v2.z
	
	if v_dists.x < 0 AND v_dists.y < 0 AND v_dists.z < 0 then
		DistanceToBB = 0
	else
		DistanceToBB = Max(Max(v_dists.x, v_dists.y), v_dists.z)
	end if
End Function

Function LinearWithLimits(input As Double, min_input As Double, max_input As Double, min_out As Double, max_out As Double) As Double
	Dim a, b, y As Double
	a = (max_out - min_out)/(max_input - min_input)
	b = min_out - a * min_input
	y = a*input + b
	if a == 0 then
		if input >= max_input then y = max_out
		if input <  min_input then y = min_out
	else
		y = Min(y, max_out)
		y = Max(y, min_out)
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
