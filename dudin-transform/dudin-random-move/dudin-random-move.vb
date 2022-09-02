Dim posX, posY, posZ, posSpeed As Double
Dim maxX, maxY, maxZ, maxSpeed As Double
Dim x, y, z, speedX, speedY, speedZ As Double
Dim dirX, dirY, dirZ As Integer

sub OnInitParameters()
	RegisterParameterDouble("posX", "X amplitude", 0, 0, 999999)
	RegisterParameterDouble("posY", "Y amplitude", 0, 0, 999999)
	RegisterParameterDouble("posZ", "Z amplitude", 0, 0, 999999)
	RegisterParameterDouble("posSpeed", "Position Speed", 0, 0, 999999)
end sub

sub OnInit()
	posX = GetParameterDouble("posX")
	posY = GetParameterDouble("posY")
	posZ = GetParameterDouble("posZ")
	posSpeed = GetParameterDouble("posSpeed")
	x = 0
	y = 0
	z = 0
	speedX = (posSpeed/2.0 + (posSpeed/2.0)*Random())/100.0
	speedY = (posSpeed/2.0 + (posSpeed/2.0)*Random())/100.0
	speedZ = (posSpeed/2.0 + (posSpeed/2.0)*Random())/100.0
	dirX = 1
	dirY = 1
	dirZ = 1
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	if posX > 0 then
		if x >= 100 then dirX = -1
		if x <= 0 then dirX = 1
		x += dirX*speedX
	end if
	
	if posY > 0 then
		if y >= 100 then dirY = -1
		if y <= 0 then dirY = 1
		y += dirY*speedY
	end if

	if posZ > 0 then	
		if z >= 100 then dirZ = -1
		if z <= 0 then dirZ = 1
		z += dirZ*speedZ
	end if
	
	this.position.x = posX * (Besizer(x, 0, 1, 50, 50) - 0.5)
	this.position.y = posY * (Besizer(y, 0, 1, 50, 50) - 0.5)
	this.position.z = posZ * (Besizer(z, 0, 1, 50, 50) - 0.5)
end sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

'CLAMP
Function ClampDbl(value as double, min  as double, max as double) as Double
	if value < min then value = min
	if value > max then value = max
	ClampDbl = value
End Function

'BESIER
Function Besizer(ByVal procent as double, ByVal beginValue as double, ByVal end_value as double, ByVal beginWeight as double, ByVal endWeight as double) as Double
	Dim a, b, c, d, tBesierValue As Double
	procent      = ClampDbl(procent,       0, 100)/100.0
	beginWeight = ClampDbl(beginWeight, 35, 100)/100.0
	endWeight   = ClampDbl(endWeight,   35, 100)/100.0
	
	a = 3*beginWeight - 3*(1.0 - endWeight) + 1
	b = - 6*beginWeight + 3*(1.0 - endWeight)
	c = 3*beginWeight
	d = -procent
	
	tBesierValue = (sqrt((-27*a^2*d + 9*a*b*c - 2*b^3)^2 + 4*(3*a*c - b^2)^3) - 27*a^2*d + 9*a*b*c - 2*b^3)^(1.0/3)/(3*2^(1.0/3)*a) - (2^(1.0/3)*(3*a*c - b^2))/(3*a*(sqrt((-27*a^2*d + 9*a*b*c - 2*b^3)^2 + 4*(3*a*c - b^2)^3) - 27*a^2*d + 9*a*b*c - 2*b^3)^(1.0/3)) - b/(3*a)
	Besizer = beginValue + (end_value - beginValue)*( 3*(1-tBesierValue)*tBesierValue^2 + tBesierValue^3 ) 
End Function
