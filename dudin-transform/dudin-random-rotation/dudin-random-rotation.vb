Dim rot_x, rot_y, rot_z, rot_speed As Double
Dim max_x, max_y, max_z, max_speed As Double
Dim x, y, z, speed_x, speed_y, speed_z As Double
Dim dir_x, dir_y, dir_z As Integer

sub OnInitParameters()
	RegisterParameterDouble("rot_x", "Rotation X", 0, 0, 999999)
	RegisterParameterDouble("rot_y", "Rotation X", 0, 0, 999999)
	RegisterParameterDouble("rot_z", "Rotation X", 0, 0, 999999)
	RegisterParameterDouble("rot_speed", "Rotation Speed", 0, 0, 999999)
end sub

sub OnInit()
	rot_x = GetParameterDouble("rot_x")
	rot_y = GetParameterDouble("rot_y")
	rot_z = GetParameterDouble("rot_z")
	rot_speed = GetParameterDouble("rot_speed")
	x = 0
	y = 0
	z = 0
	speed_x = (rot_speed/2.0 + (rot_speed/2.0)*Random())/100.0
	speed_y = (rot_speed/2.0 + (rot_speed/2.0)*Random())/100.0
	speed_z = (rot_speed/2.0 + (rot_speed/2.0)*Random())/100.0
	dir_x = 1
	dir_y = 1
	dir_z = 1
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	if rot_x > 0 then
		if x >= 100 then dir_x = -1
		if x <= 0 then dir_x = 1
		x += dir_x*speed_x
	end if
	
	if rot_y > 0 then
		if y >= 100 then dir_y = -1
		if y <= 0 then dir_y = 1
		y += dir_y*speed_y
	end if

	if rot_z > 0 then	
		if z >= 100 then dir_z = -1
		if z <= 0 then dir_z = 1
		z += dir_z*speed_z
	end if
	
	this.rotation.x = rot_x * (Besizer(x, 0, 1, 50, 50) - 0.5)
	this.rotation.y = rot_y * (Besizer(y, 0, 1, 50, 50) - 0.5)
	this.rotation.z = rot_z * (Besizer(z, 0, 1, 50, 50) - 0.5)
end sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

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
