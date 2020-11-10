Dim t as integer = -1

sub OnInitParameters()
	RegisterParameterDouble("duration", "Duration (frames)", 0.0, 0, 999999)
	RegisterParameterDouble("begin_value", "Begin value", 0.0, -999999, 999999)
	RegisterParameterDouble("end_value", "End value", 100.0, -999999, 999999)
	RegisterParameterSliderInt("begin_weight", "Weight begin", 35, 35, 100, 100)
	RegisterParameterSliderInt("end_weight", "Weight end", 35, 35, 100, 100)
	RegisterPushButton("anim", "Anim!", 1)
end sub

sub OnExecPerField()
	if t >= 0 then
		t += 1
		if t <= GetParameterDouble("duration") then
			this.position.x = Besizer(100*t / GetParameterDouble("duration"), GetParameterDouble("begin_value"), GetParameterDouble("end_value"), GetParameterInt("begin_weight"), GetParameterInt("end_weight"))
		else
			t = -1
		end if
	end if
end sub

sub OnExecAction(buttonId As Integer)
	' start animation
	if buttonId == 1 then t = 0
	println("======")
end sub

''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Dim info As String = "Creator: Dmitry Dudin.
Version 0.1 (3 december 2018)"

'CLAMP
Function ClampDbl(value as double, min  as double, max as double) as Double
	if value < min then value = min
	if value > max then value = max
	ClampDbl = value
End Function

'BESIER
Function Besizer(procent as double, begin_value as double, end_value as double, begin_weight as double, end_weight as double) as Double
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

