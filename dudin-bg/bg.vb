RegisterPluginVersion(1,9,0)

Dim info As String = "
Developer: Dmitry Dudin, dudin.tv
"

Dim c_bg As Container = this
Dim c_source, c_exact_source, child, c_min_x, c_min_y, c_min_z, c_max_child As Container
Dim mode_size, mode, mode_min_x, mode_min_y, mode_min_z, fon_change_mode, max_size_mode, pauseAxis As Integer
Dim x,y,z, x_multy, y_multy, z_multy, x_padding, y_padding, z_padding, x_min, y_min, z_min As Double
Dim pos As Vertex
Dim size, child_size, min_size As Vertex
Dim newSize, newPosition, v1, v2, vSource1, vSource2 As Vertex
Dim newPosX, newPosY As Double
Dim sizeTreshold, prevNewValue, newValue As Double
Dim animTreshold As Double = 0.001

Dim arr_pause_axis As Array[String]
arr_pause_axis.Push("X")
arr_pause_axis.Push("Y")

Dim arr_axis As Array[String]
arr_axis.Push("X")
arr_axis.Push("Y")
arr_axis.Push("Z")

Dim arr_s As Array[String]
arr_s.Push("X")
arr_s.Push("Y")
arr_s.Push("Z")
arr_s.Push("XY")

Dim MODE_X  As Integer = 0
Dim MODE_Y  As Integer = 1
Dim MODE_Z  As Integer = 2
Dim MODE_XY As Integer = 3

Dim arr_ss As Array[String]
arr_ss.Push("Source container size")
arr_ss.Push("Max of childs")
arr_ss.Push("Child by index")
Dim arr_sss As Array[String]
arr_sss.Push("Off")
arr_sss.Push("Number")
arr_sss.Push("Container")
Dim arr_sFonShangeMode As Array[String]
arr_sFonShangeMode.Push("Scaling")
arr_sFonShangeMode.Push("Geometry")

Dim arrPauseMode As Array[String]
arrPauseMode.Push("none")
arrPauseMode.Push("><")
arrPauseMode.Push("<>")
arrPauseMode.Push(">< and <>")
Dim PAUSE_MODE_NONE As Integer = 0
Dim PAUSE_MODE_LESS As Integer = 1
Dim PAUSE_MODE_MORE As Integer = 2
Dim PAUSE_MODE_BOTH As Integer = 3

Dim iPauseDownTicks As Integer

sub OnInitParameters()
    RegisterInfoText(info)
	RegisterParameterContainer("source","Source container:")
	RegisterRadioButton("fon_change_mode", "└ How to change bg size", 0, arr_sFonShangeMode)
	RegisterRadioButton("mode_size", "Get size from: ", 0, arr_ss)
	RegisterRadioButton("max_size_mode", "└ max size by asix:", 0, arr_axis)
	RegisterParameterInt("num_child", "└ Child index (0=none)", 1, 0, 100)
	RegisterRadioButton("mode", "└ Axis to consider:", 0, arr_s)
	RegisterParameterDouble("x_multy", "   └ Mult X", 1.0, 0.0, 10000000.0)
	RegisterParameterDouble("x_padding", "   └ Add X", 0.0, -100000.0, 10000000.0)
	RegisterParameterDouble("y_multy", "   └ Mult Y", 1.0, 0.0, 10000000.0)
	RegisterParameterDouble("y_padding", "   └ Add Y", 0.0, -100000.0, 10000000.0)
	RegisterParameterDouble("z_multy", "   └ Mult Z", 1.0, 0.0, 10000000.0)
	RegisterParameterDouble("z_padding", "   └ Add Z", 0.0, -100000.0, 10000000.0)
	
	RegisterRadioButton("x_min_mode", "Min BG X-axis mode", 0, arr_sss)
	RegisterParameterDouble("x_min", "└ Min X value", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("x_min_c", "└ Min X-axis container")
	
	RegisterRadioButton("y_min_mode", "Min BG Y-axis mode", 0, arr_sss)
	RegisterParameterDouble("y_min", "└ Min Y value", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("y_min_c", "└ Min Y-axis container")
	
	RegisterRadioButton("z_min_mode", "Min BG Z-axis mode", 0, arr_sss)
	RegisterParameterDouble("z_min", "└ Min Z value", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("z_min_c", "└ Min Z-axis container")
	
	RegisterParameterBool("hide_by_zero", "Hide bg if size close to zero", TRUE)
	RegisterParameterDouble("treshold", "└ Zero-size of source container", 0.1, 0.0, 1000.0)
	
	
	RegisterParameterBool("position_x", "Autofollow by X axis", FALSE)
	RegisterParameterDouble("position_x_shift", "└ X shift", 0, -99999, 99999)
	RegisterParameterBool("position_y", "Autofollow by Y axis", FALSE)
	RegisterParameterDouble("position_y_shift", "└ Y shift", 0, -99999, 99999)
	
	
	RegisterParameterDouble("inertion", "Animation inertion", 2.0, 1.0, 100.0)
	RegisterRadioButton("pause_mode", "└ Pause direction", 0, arrPauseMode)
	RegisterRadioButton("pause_axis", "   └ Considering size axis", 0, arr_pause_axis)
	RegisterParameterInt("pause_less", "   └ Pause >< (frames)", 0, 0, 1000)
	RegisterParameterInt("pause_more", "   └ Pause <> (frames)", 0, 0, 1000)
end sub

sub OnParameterChanged(parameterName As String)
	c_source = GetParameterContainer("source")
	max_size_mode = GetParameterInt("max_size_mode")
end sub
 
sub OnGuiStatus()
	mode_size = GetParameterInt("mode_size")
	If mode_size == 0 Then
		SendGuiParameterShow("num_child",HIDE)
		SendGuiParameterShow("max_size_mode",HIDE)
	ElseIf mode_size == 1 Then
		SendGuiParameterShow("num_child",HIDE)
		SendGuiParameterShow("max_size_mode",SHOW)
	ElseIf mode_size == 2 Then
		SendGuiParameterShow("num_child",SHOW)
		SendGuiParameterShow("max_size_mode",HIDE)
	End If
	
	SendGuiParameterShow( "treshold", CInt(GetParameterBool("hide_by_zero")) )
	
	Dim showXMulti as Integer = HIDE
	Dim showYMulti as Integer = HIDE
	Dim showZMulti as Integer = HIDE
	Dim showXPadding as Integer = HIDE
	Dim showYPadding as Integer = HIDE
	Dim showZPadding as Integer = HIDE
	Dim showXMinMode as Integer = HIDE
	Dim showXMin as Integer = HIDE
	Dim showXMinC as Integer = HIDE
	Dim showYMinMode as Integer = HIDE
	Dim showYMin as Integer = HIDE
	Dim showYMinC as Integer = HIDE
	Dim showZMinMode as Integer = HIDE
	Dim showZMin as Integer = HIDE
	Dim showZMinC as Integer = HIDE
	Dim showPauseAxis as Integer = HIDE
	
	mode = GetParameterInt("mode")
	Select Case mode
	Case MODE_X
		showXMulti = SHOW
		showXPadding = SHOW
		showXMinMode = SHOW
		showXMin = SHOW
		showXMinC = SHOW
	Case MODE_Y
		showYMulti = SHOW
		showYPadding = SHOW
		showYMinMode = SHOW
		showYMin = SHOW
		showYMinC = SHOW
	Case MODE_Z
		showZMulti = SHOW
		showZPadding = SHOW
		showZMinMode = SHOW
		showZMin = SHOW
		showZMinC = SHOW
	Case MODE_XY
		showXMulti = SHOW
		showYMulti = SHOW
		showXPadding = SHOW
		showYPadding = SHOW
		showXMinMode = SHOW
		showXMin = SHOW
		showXMinC = SHOW
		showYMinMode = SHOW
		showYMin = SHOW
		showYMinC = SHOW
		showPauseAxis = SHOW
	End Select
	
	SendGuiParameterShow("x_multy", showXMulti)
	SendGuiParameterShow("y_multy", showYMulti)
	SendGuiParameterShow("z_multy", showZMulti)
	SendGuiParameterShow("x_padding", showXPadding)
	SendGuiParameterShow("y_padding", showYPadding)
	SendGuiParameterShow("z_padding", showZPadding)
	SendGuiParameterShow("x_min_mode", showXMinMode)
	SendGuiParameterShow("x_min", showXMin)
	SendGuiParameterShow("x_min_c", showXMinC)
	SendGuiParameterShow("y_min_mode", showYMinMode)
	SendGuiParameterShow("y_min", showYMin)
	SendGuiParameterShow("y_min_c", showYMinC)
	SendGuiParameterShow("z_min_mode", showZMinMode)
	SendGuiParameterShow("z_min", showZMin)
	SendGuiParameterShow("z_min_c", showZMinC)
	SendGuiParameterShow("pause_axis", showPauseAxis)
	
	SendGuiParameterShow("pause_mode", CInt( GetParameterDouble("inertion") > 1 ))
	SendGuiParameterShow("pause_less", CInt( (GetParameterInt("pause_mode") == PAUSE_MODE_LESS OR GetParameterInt("pause_mode") == PAUSE_MODE_BOTH) AND GetParameterDouble("inertion") > 1 )) 
	SendGuiParameterShow("pause_more", CInt( (GetParameterInt("pause_mode") == PAUSE_MODE_MORE OR GetParameterInt("pause_mode") == PAUSE_MODE_BOTH) AND GetParameterDouble("inertion") > 1 )) 
	
	If mode == MODE_X OR mode == MODE_XY Then
		mode_min_x = GetParameterInt("x_min_mode")
		If mode_min_x == 0 Then
			SendGuiParameterShow("x_min",HIDE)
			SendGuiParameterShow("x_min_c",HIDE)
		ElseIf mode_min_x == 1 Then
			SendGuiParameterShow("x_min",SHOW)
			SendGuiParameterShow("x_min_c",HIDE)
		ElseIf mode_min_x == 2 Then
			SendGuiParameterShow("x_min",HIDE)
			SendGuiParameterShow("x_min_c",SHOW)
		End If
	End If
	
	If mode == MODE_Y OR mode == MODE_XY Then
		mode_min_y = GetParameterInt("y_min_mode")
		If mode_min_y == 0 Then
			SendGuiParameterShow("y_min",HIDE)
			SendGuiParameterShow("y_min_c",HIDE)
		ElseIf mode_min_y == 1 Then
			SendGuiParameterShow("y_min",SHOW)
			SendGuiParameterShow("y_min_c",HIDE)
		ElseIf mode_min_y == 1 Then
			SendGuiParameterShow("y_min",HIDE)
			SendGuiParameterShow("y_min_c",SHOW)
		End If
	End If
	
	If mode == MODE_Z Then
		mode_min_z = GetParameterInt("z_min_mode")
		If mode_min_z == 0 Then
			SendGuiParameterShow("z_min",HIDE)
			SendGuiParameterShow("z_min_c",HIDE)
		ElseIf mode_min_z == 1 Then
			SendGuiParameterShow("z_min",SHOW)
			SendGuiParameterShow("z_min_c",HIDE)
		ElseIf mode_min_z == 2 Then
			SendGuiParameterShow("z_min",HIDE)
			SendGuiParameterShow("z_min_c",SHOW)
		End If
	End If
	
	SendGuiParameterShow( "position_x_shift", CInt( GetParameterBool("position_x") ) )
	SendGuiParameterShow( "position_y_shift", CInt( GetParameterBool("position_y") ) )
end sub
 
sub OnExecPerField()
	If c_source == null Then exit sub
	
	mode = GetParameterInt("mode")
	sizeTreshold = GetParameterDouble("treshold")/100.0
	fon_change_mode = GetParameterInt("fon_change_mode")
	
	GetSourceSize()
	CalcMinSize()
	
	'-------------------------------------------------------------------------------------------------
	x_multy = GetParameterDouble("x_multy")
	y_multy = GetParameterDouble("y_multy")
	z_multy = GetParameterDouble("z_multy")
	x_padding = GetParameterDouble("x_padding")
	y_padding = GetParameterDouble("y_padding")
	z_padding = GetParameterDouble("z_padding")
	
	'processing X
	If mode == MODE_X OR mode == MODE_XY Then
		If size.X < sizeTreshold Then
			If GetParameterBool("hide_by_zero") Then
				c_bg.Active = false
				'Exit Sub
			Else
				c_bg.Active = true
				newSize.X = 0
			End If
		Else
			c_bg.Active = true
			x = size.X/100.0 * x_multy + x_padding/10.0
			If x < x_min Then x = x_min
			newSize.X = x
		End If
	End If
	
	'processing Y
	If mode == MODE_Y OR mode == MODE_XY Then
		If size.Y < sizeTreshold Then
			If GetParameterBool("hide_by_zero") Then
				c_bg.Active = false
				'Exit Sub
			Else
				c_bg.Active = true
				newSize.Y = 0
			End If
		Else
			c_bg.Active = true
			y = size.Y/100.0 * y_multy + y_padding/100.0
			If y < y_min Then y = y_min
			newSize.Y = y
		End If
	End If
	
	'processing Z
	If mode == MODE_Z Then
		If size.Z < sizeTreshold Then
			If GetParameterBool("hide_by_zero") Then
				c_bg.Active = false
				Exit Sub
			Else
				c_bg.Active = true
				newSize.Z = 0
			End If
		Else
			c_bg.Active = true
			z = size.Z/100.0 * z_multy + z_padding/100.0
			If z < z_min Then z = z_min
			newSize.Z = z
		End If
	End If
	
	'-------------------------------------------------------------------------------------------------
	CalcPosition()
	ApplyTransformationWithInertion()
	c_bg.RecomputeMatrix()
End Sub

Sub GetSourceSize()
	'mode logic ("Source container size", "Max child", "Child by index")
	mode_size = GetParameterInt("mode_size")
	If mode_size == 0 Then
		'mode: "Source container size"
		c_exact_source = c_source
	ElseIf mode_size == 1 Then
		'mode: "Max child"
		child = c_source.FirstChildContainer
		c_max_child = child
		size = GetLocalSize (child, c_bg)
		child = child.NextContainer
		Do While child <> null
			child.RecomputeMatrix()
			child_size = GetLocalSize (child, c_bg)
			If max_size_mode == 0 AND child_size.X > size.X Then c_max_child = child
			If max_size_mode == 1 AND child_size.Y > size.Y Then c_max_child = child
			If max_size_mode == 2 AND child_size.Z > size.Z Then c_max_child = child
			if c_max_child == child then size = child_size
			child = child.NextContainer
		Loop
		c_exact_source = c_max_child
	ElseIf mode_size == 2 Then
		'mode: "Child by index"
		c_exact_source = c_source.GetChildContainerByIndex(GetParameterInt("num_child") - 1)
		c_exact_source.RecomputeMatrix()
	End If
	
	size = GetLocalSize (c_exact_source, c_bg)
End Sub

Sub CalcMinSize()
	c_min_x = GetParameterContainer("x_min_c")
	c_min_y = GetParameterContainer("y_min_c")
	c_min_z = GetParameterContainer("z_min_c")
	
	If mode_min_x == 1 Then
		'Min size from value
		x_min = GetParameterDouble("x_min")
	ElseIf mode_min_x == 2 AND c_min_x <> null Then
		'Min size from container
		min_size = GetLocalSize (c_min_x, c_bg)
		If min_size.X > sizeTreshold AND min_size.Y > sizeTreshold Then
			x_min = min_size.X/100.0
		Else
			x_min = 0
		End If
	Else
		x_min = 0
	End If
	
	If mode_min_y == 1 Then
		'Min size from value
		y_min = GetParameterDouble("y_min")
	ElseIF mode_min_y == 2 AND c_min_y <> null Then
		'Min size from container
		min_size = GetLocalSize (c_min_y, c_bg)
		y_min = min_size.Y/100.0
		If min_size.X > sizeTreshold AND min_size.Y > sizeTreshold Then
			y_min = min_size.Y/100.0
		Else
			y_min = 0
		End If
	Else
		y_min = 0
	End If
End Sub

Sub PreCalcFinishGabarits()
	Dim cached_scalinx_x = c_bg.scaling.x
	Dim cached_scalinx_y = c_bg.scaling.y
	Dim cached_width = c_bg.geometry.GetParameterDouble("width")
	Dim cached_height = c_bg.geometry.GetParameterDouble("height")
	if fon_change_mode == 0 then
		c_bg.scaling.x = newSize.x
		c_bg.scaling.y = newSize.y
	elseif fon_change_mode == 1 then
		c_bg.geometry.SetParameterDouble(  "width", 100.0*newSize.x )
		c_bg.geometry.SetParameterDouble(  "height", 100.0*newSize.y )
	end if
	c_bg.RecomputeMatrix()
	
	' calculate the target bounding box like it's already finished
	c_bg.GetTransformedBoundingBox(v1, v2)
	
	c_bg.scaling.x = cached_scalinx_x
	c_bg.scaling.y = cached_scalinx_y
	c_bg.geometry.SetParameterDouble(  "width", cached_width )
	c_bg.geometry.SetParameterDouble(  "height", cached_height )
	c_bg.RecomputeMatrix()
End Sub

Sub CalcPosition()
	if GetParameterBool("position_x") OR GetParameterBool("position_y") then
		c_exact_source.GetTransformedBoundingBox(vSource1, vSource2)
		PreCalcFinishGabarits()
		
		if GetParameterBool("position_x") then
			vSource1 = c_source.LocalPosToWorldPos(vSource1)
			vSource1 = c_exact_source.parentContainer.WorldPosToLocalPos(vSource1)
			newPosition.x = vSource1.x - (v1.x-c_bg.position.x) + GetParameterDouble("position_x_shift")
		end if
		
		if GetParameterBool("position_y") then
			vSource2 = c_source.LocalPosToWorldPos(vSource2)
			vSource2 = c_exact_source.parentContainer.WorldPosToLocalPos(vSource2)
			newPosition.y = vSource2.y - (v2.y-c_bg.position.y) + GetParameterDouble("position_y_shift")
		end if
	end if
End Sub

Function HasPause() as Boolean
	pauseAxis = GetParameterInt("pause_axis")
	Dim currentValue As Double
	if mode == MODE_X OR (mode == MODE_XY AND pauseAxis == MODE_X) Then
		newValue = newSize.x
		if fon_change_mode == 0 then
			currentValue = c_bg.scaling.x
		elseif fon_change_mode == 1 then
			newValue *= 100
			currentValue = c_bg.geometry.GetParameterDouble("width")
		end if
	elseif mode == MODE_Y OR (mode == MODE_XY AND pauseAxis == MODE_Y) Then
		newValue = newSize.y
		if fon_change_mode == 0 then
			currentValue = c_bg.scaling.y
		elseif fon_change_mode == 1 then
			newValue *= 100
			currentValue = c_bg.geometry.GetParameterDouble("height")
		end if
	elseif mode == MODE_Z Then
		newValue = newSize.z
		if fon_change_mode == 0 then
			currentValue = c_bg.scaling.z
		elseif fon_change_mode == 1 then
			newValue *= 100
			currentValue = c_bg.geometry.GetParameterDouble("height")
		end if
	end if
	
	if prevNewValue <> newValue then
		If GetParameterInt("pause_mode") == PAUSE_MODE_NONE Then
			iPauseDownTicks = 0
		Else
			If (GetParameterInt("pause_mode") == PAUSE_MODE_MORE OR GetParameterInt("pause_mode") == PAUSE_MODE_BOTH) AND newValue > currentValue + animTreshold Then
				iPauseDownTicks = GetParameterInt("pause_more")
			End If
			If (GetParameterInt("pause_mode") == PAUSE_MODE_LESS OR GetParameterInt("pause_mode") == PAUSE_MODE_BOTH) AND newValue < currentValue - animTreshold Then
				iPauseDownTicks = GetParameterInt("pause_less")
			End If
		End If
		
		prevNewValue = newValue
	end if
	
	If iPauseDownTicks > 0 then
		iPauseDownTicks -= 1
		HasPause = true
	Else
		HasPause = false
	End if
End Function

Sub ApplyTransformationWithInertion()
	If HasPause() then exit sub
	
	if mode == MODE_X OR mode == MODE_XY Then
		if fon_change_mode == 0 then
			c_bg.scaling.x = GetAnimatedValue(c_bg.scaling.x, newSize.x)
		elseif fon_change_mode == 1 then
			c_bg.geometry.SetParameterDouble(  "width", GetAnimatedValue(c_bg.geometry.GetParameterDouble("width"), 100.0*newSize.x)  )
		end if
	end if
	if mode == MODE_Y OR mode == MODE_XY Then
		if fon_change_mode == 0 then
			c_bg.scaling.y = GetAnimatedValue(c_bg.scaling.y, newSize.y)
		elseif fon_change_mode == 1 then
			c_bg.geometry.SetParameterDouble(  "height", GetAnimatedValue(c_bg.geometry.GetParameterDouble("height"), 100.0*newSize.y)  )
		end if
	end if
	if mode == MODE_Z Then
		if fon_change_mode == 0 then
			c_bg.scaling.z = GetAnimatedValue(c_bg.scaling.z, newSize.z)
		elseif fon_change_mode == 1 then
			c_bg.geometry.SetParameterDouble(  "height", GetAnimatedValue(c_bg.geometry.GetParameterDouble("height"), 100.0*newSize.z)  )
		end if
	end if
	
	if GetParameterBool("position_x") then
		c_bg.position.x = GetAnimatedValue(c_bg.position.x, newPosition.x)
	end if
	
	if GetParameterBool("position_y") then
		c_bg.position.y = GetAnimatedValue(c_bg.position.y, newPosition.y)
	end if
End Sub

Function GetAnimatedValue(currentValue as Double, newValue as Double) As Double
	if newValue < (currentValue - animTreshold) OR newValue > (currentValue + animTreshold) Then
		GetAnimatedValue = currentValue + (newValue - currentValue)/GetParameterDouble("inertion")
	else
		GetAnimatedValue = newValue
	end if
End Function

Function GetLocalSize(_c_gabarit as Container, _c as Container) As Vertex
	_c_gabarit.GetTransformedBoundingBox(v1,v2)
	v1 = _c.WorldPosToLocalPos(v1)
	v2 = _c.WorldPosToLocalPos(v2)
	GetLocalSize = CVertex(v2.x-v1.x,v2.y-v1.y,v2.z-v1.z)
End Function
