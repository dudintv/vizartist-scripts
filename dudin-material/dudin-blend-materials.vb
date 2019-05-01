Dim arr_material As Array[Material]
Dim arr_value As Array[Double]
Dim arr_speed As Array[Double]
Dim arr_c As Array[Container]
Dim speed As Double
Dim find_name As String

Dim color1 As Color = CColor(255,255,255,100)
Dim color2 As Color = CColor(255,255,255,100)
sub OnInitParameters()
	RegisterPluginVersion(0, 1, 0)
	RegisterParameterColor("color1", "Color 1", color1)
	RegisterParameterColor("color2", "Color 2", color2)
	RegisterParameterDouble("speed", "Speed", 100.0, 0, 1000.0)
	RegisterParameterString("name", "Name subs with color", "", 30, 100, "")
end sub

sub OnInit()
	color1 = GetParameterColor("color1")
	color2 = GetParameterColor("color2")
	speed = GetParameterDouble("speed")/100.0
	find_name = GetParameterString("name")
	
	arr_c.Clear()
	Dim c As Container
	c = this.FirstChildContainer
	Do 
		arr_c.Push(c)
		c = c.NextContainer
	Loop While c <> null
	
	arr_material.Clear()
	arr_value.Clear()
	arr_speed.Clear()
	for i=0 to arr_c.Ubound
		arr_material.Push(arr_c[i].FindSubContainer(find_name).Material)
		arr_value.Push(0)
		arr_speed.Push(CDbl(speed*Random(100)/100.0 + speed))
	next
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	for i=0 to arr_material.Ubound
		arr_value[i] += arr_speed[i]
		if arr_value[i] > 99.999 or arr_value[i] < 0.001 then
			if arr_value[i] > 99.999 then arr_value[i] = 100
			if arr_value[i] < 0.001 then arr_value[i] = 0
			arr_speed[i] = (-1 * Sign(arr_speed[i])) * (speed*Random(100)/100.0 + speed)
		end if
		arr_material[i].Diffuse = CalculateColor(color1, color2, arr_value[i])
	next
end sub

Function CalculateColor(start_color As Color, end_color As Color, procent As double) As Color
	Dim out_color As Color
	out_color.Red   = start_color.Red   + (end_color.Red   - start_color.Red)*procent/100.0
	out_color.Green = start_color.Green + (end_color.Green - start_color.Green)*procent/100.0
	out_color.Blue  = start_color.Blue  + (end_color.Blue  - start_color.Blue)*procent/100.0
	out_color.Alpha = start_color.Alpha + (end_color.Alpha - start_color.Alpha)*procent/100.0
	CalculateColor = out_color
End Function
