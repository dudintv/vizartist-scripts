RegisterPluginVersion(1,1,0)
Dim amplitude, scale_speed, speed, value, base_scale, pause, pause_random As Double
Dim max, max_speed As Double
Dim dir, prev_dir As Integer

sub OnInitParameters()
	RegisterParameterDouble("base_scale", "Base scale", 1.0, 0, 999999)
	RegisterParameterDouble("amplitude", "Scale amplitude %", 20.0, 0, 100)
	RegisterParameterDouble("scale_speed", "Speed", 0, 0, 999999)
	RegisterParameterDouble("pause", "Pause (sec)", 0, 0, 999999)
	RegisterParameterDouble("pause_random", "Pause random %", 0, 0, 100)
end sub

sub OnInit()
	scale_speed = GetParameterDouble("scale_speed")
	base_scale = GetParameterDouble("base_scale")
	amplitude = base_scale * GetParameterDouble("amplitude")/100.0
	value = 0
	speed = (scale_speed/2.0 + (scale_speed/2.0)*Random())/100.0
	dir = 1
	pause_random = GetParameterDouble("pause_random")/100.0
	pause = (  (1-pause_random/2.0)*GetParameterDouble("pause") + pause_random*GetParameterDouble("pause")*Random()  )*50.0
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	prev_dir = dir
	if value >= 100+pause then dir = -1
	if value <= 0-pause then dir = 1
	value += dir*speed
	if prev_dir <> dir then pause = (  (1-pause_random/2.0)*GetParameterDouble("pause") + pause_random*GetParameterDouble("pause")*Random()  )*50.0
	
	this.scaling.xyz = base_scale + 2*amplitude * (Besizer(value, 0, 1, 50, 50)-0.5)
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
