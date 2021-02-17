RegisterPluginVersion(1,8,0)

Dim info As String = "
Developer: Dmitry Dudin, dudin.tv
"

Dim c_bg As Container = this
Dim c_source, c_exact_bg, child, c_min_x, c_min_y, c_min_z, c_max_child As Container
Dim mode_size, mode, mode_min_x, mode_min_y, mode_min_z, fon_change_mode, max_size_mode As Integer
Dim x,y,z, x_multy, y_multy, z_multy, x_padding, y_padding, z_padding, x_min, y_min, z_min As Double
Dim pos As Vertex
Dim size, child_size, min_size As Vertex
Dim newSize, newPosition, v1, v2, vSource1, vSource2 As Vertex
Dim newPosX, newPosY As Double
Dim sizeTreshold, animTreshold As Double

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
arr_sss.Push("Number")
arr_sss.Push("Container")
Dim arr_sFonShangeMode As Array[String]
arr_sFonShangeMode.Push("Scaling")
arr_sFonShangeMode.Push("Geometry")

 
sub OnInitParameters()
    RegisterInfoText(info)
	RegisterParameterContainer("source","Source container:")
	RegisterRadioButton("fon_change_mode", "└ How to change its size", 0, arr_sFonShangeMode)
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
	RegisterParameterDouble("y_min", "└ Min Z value", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("y_min_c", "└ Min Y-axis container")
	
	RegisterRadioButton("z_min_mode", "Min BG Z-axis mode", 0, arr_sss)
	RegisterParameterDouble("z_min", "└ Min Z value", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("z_min_c", "└ Min Z-axis container")
	
	RegisterParameterBool("hide_by_zero", "Hide bg if size close to zero", TRUE)
	RegisterParameterDouble("treshold", "└ Zero-size of source container", 0.1, 0.0, 1000.0)
	RegisterParameterDouble("inertion", "Animation inertion", 2.0, 1.0, 100.0)
	
	RegisterParameterBool("position_x", "Autofollow by X axis", FALSE)
	RegisterParameterDouble("position_x_shift", "└ X shift", 0, -99999, 99999)
	RegisterParameterBool("position_y", "Autofollow by Y axis", FALSE)
	RegisterParameterDouble("position_y_shift", "└ Y shift", 0, -99999, 99999)
end sub

sub OnParameterChanged(parameterName As String)
	c_source = GetParameterContainer("source")
	max_size_mode = GetParameterInt("max_size_mode")
end sub
 
sub OnGuiStatus()
	fon_change_mode = GetParameterInt("fon_change_mode")
	
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
 
	mode = GetParameterInt("mode")
	
	Select Case mode
	Case MODE_X
		SendGuiParameterShow("x_multy",SHOW)
		SendGuiParameterShow("y_multy",HIDE)
		SendGuiParameterShow("z_multy",HIDE)
		SendGuiParameterShow("x_padding",SHOW)
		SendGuiParameterShow("y_padding",HIDE)
		SendGuiParameterShow("z_padding",HIDE)
		SendGuiParameterShow("x_min_mode",SHOW)
		SendGuiParameterShow("x_min",SHOW)
		SendGuiParameterShow("x_min_c",SHOW)
		SendGuiParameterShow("y_min_mode",HIDE)
		SendGuiParameterShow("y_min",HIDE)
		SendGuiParameterShow("y_min_c",HIDE)
		SendGuiParameterShow("z_min_mode",HIDE)
		SendGuiParameterShow("z_min",HIDE)
		SendGuiParameterShow("z_min_c",HIDE)
	Case MODE_Y
		SendGuiParameterShow("x_multy",HIDE)
		SendGuiParameterShow("y_multy",SHOW)
		SendGuiParameterShow("z_multy",HIDE)
		SendGuiParameterShow("x_padding",HIDE)
		SendGuiParameterShow("y_padding",SHOW)
		SendGuiParameterShow("z_padding",HIDE)
		SendGuiParameterShow("x_min_mode",HIDE)
		SendGuiParameterShow("x_min",HIDE)
		SendGuiParameterShow("x_min_c",HIDE)
		SendGuiParameterShow("y_min_mode",SHOW)
		SendGuiParameterShow("y_min",SHOW)
		SendGuiParameterShow("y_min_c",SHOW)
		SendGuiParameterShow("z_min_mode",HIDE)
		SendGuiParameterShow("z_min",HIDE)
		SendGuiParameterShow("z_min_c",HIDE)
	Case MODE_Z
		SendGuiParameterShow("x_multy",HIDE)
		SendGuiParameterShow("y_multy",HIDE)
		SendGuiParameterShow("z_multy",SHOW)
		SendGuiParameterShow("x_padding",HIDE)
		SendGuiParameterShow("y_padding",HIDE)
		SendGuiParameterShow("z_padding",SHOW)
		SendGuiParameterShow("x_min_mode",HIDE)
		SendGuiParameterShow("x_min",HIDE)
		SendGuiParameterShow("x_min_c",HIDE)
		SendGuiParameterShow("y_min_mode",SHOW)
		SendGuiParameterShow("y_min",HIDE)
		SendGuiParameterShow("y_min_c",HIDE)
		SendGuiParameterShow("z_min_mode",SHOW)
		SendGuiParameterShow("z_min",SHOW)
		SendGuiParameterShow("z_min_c",SHOW)
	Case MODE_XY
		SendGuiParameterShow("x_multy",SHOW)
		SendGuiParameterShow("y_multy",SHOW)
		SendGuiParameterShow("z_multy",HIDE)
		SendGuiParameterShow("x_padding",SHOW)
		SendGuiParameterShow("y_padding",SHOW)
		SendGuiParameterShow("z_padding",HIDE)
		SendGuiParameterShow("x_min_mode",SHOW)
		SendGuiParameterShow("x_min",SHOW)
		SendGuiParameterShow("x_min_c",SHOW)
		SendGuiParameterShow("y_min_mode",SHOW)
		SendGuiParameterShow("y_min",SHOW)
		SendGuiParameterShow("y_min_c",SHOW)
		SendGuiParameterShow("z_min_mode",HIDE)
		SendGuiParameterShow("z_min",HIDE)
		SendGuiParameterShow("z_min_c",HIDE)
	End Select
	
	If mode == MODE_X OR mode == MODE_XY Then
		mode_min_x = GetParameterInt("x_min_mode")
		If mode_min_x == 0 Then
			SendGuiParameterShow("x_min",SHOW)
			SendGuiParameterShow("x_min_c",HIDE)
		ElseIf mode_min_x == 1 Then
			SendGuiParameterShow("x_min",HIDE)
			SendGuiParameterShow("x_min_c",SHOW)
		End If
	End If
	
	If mode == MODE_Y OR mode == MODE_XY Then
		mode_min_y = GetParameterInt("y_min_mode")
		If mode_min_y == 0 Then
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
			SendGuiParameterShow("z_min",SHOW)
			SendGuiParameterShow("z_min_c",HIDE)
		ElseIf mode_min_z == 1 Then
			SendGuiParameterShow("z_min",HIDE)
			SendGuiParameterShow("z_min_c",SHOW)
		End If
	End If
	
	SendGuiParameterShow( "position_x_shift", CInt( GetParameterBool("position_x") ) )
	SendGuiParameterShow( "position_y_shift", CInt( GetParameterBool("position_y") ) )
end sub
 
Function GetLocalSize (_c_gabarit as Container, _c as Container) As Vertex
	_c_gabarit.GetTransformedBoundingBox(v1,v2)
	v1 = _c.WorldPosToLocalPos(v1)
	v2 = _c.WorldPosToLocalPos(v2)
	GetLocalSize = CVertex(v2.x-v1.x,v2.y-v1.y,v2.z-v1.z)
End Function
 
sub OnExecPerField()
	If c_source == null Then exit sub
	
	mode = GetParameterInt("mode")
	sizeTreshold = GetParameterDouble("treshold")/100.0
	fon_change_mode = GetParameterInt("fon_change_mode")
	
	'mode logic ("Source container size", "Max child", "Child by index")
	mode_size = GetParameterInt("mode_size")
	If mode_size == 0 Then
		'mode: "Source container size"
		size = GetLocalSize (c_source, c_bg)
	ElseIf mode_size == 1 Then
		'mode: "Max child"
		child = c_source.FirstChildContainer
		child.RecomputeMatrix()
		c_max_child = child
		child = child.NextContainer
		Do While child <> null
			child.RecomputeMatrix()
			child_size = GetLocalSize (child, c_bg)
			If max_size_mode == 0 AND child_size.X > size.X Then c_max_child = child
			If max_size_mode == 1 AND child_size.Y > size.Y Then c_max_child = child
			If max_size_mode == 2 AND child_size.Z > size.Z Then c_max_child = child
			child = child.NextContainer
		Loop
		size = GetLocalSize (c_max_child, c_bg)
	ElseIf mode_size == 2 Then
		'mode: "Child by index"
		child = c_source.GetChildContainerByIndex(GetParameterInt("num_child") - 1)
		child.RecomputeMatrix()
		size = GetLocalSize (child, c_bg)
	End If
 
 
	x_multy = GetParameterDouble("x_multy")
	y_multy = GetParameterDouble("y_multy")
	z_multy = GetParameterDouble("z_multy")
	x_padding = GetParameterDouble("x_padding")
	y_padding = GetParameterDouble("y_padding")
	z_padding = GetParameterDouble("z_padding")
	c_min_x = GetParameterContainer("x_min_c")
	c_min_y = GetParameterContainer("y_min_c")
	c_min_z = GetParameterContainer("z_min_c")
	
	If mode_min_x == 1 Then
		'Max size from container
		If c_min_x <> null Then
			min_size = GetLocalSize (c_min_x, c_bg)
			If min_size.X > sizeTreshold AND min_size.Y > sizeTreshold Then
				x_min = min_size.X/100.0
			Else
				x_min = 0
			End If
		Else
			x_min = 0
		End If
	Else
		'Max size from value
		x_min = GetParameterDouble("x_min")
	End If
	
	If mode_min_y == 1 Then
		'Max size from container
		If c_min_y <> null Then
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
	Else
		'Max size from value
		y_min = GetParameterDouble("y_min")
	End If
	
	
	
	'-------------------------------------------------------------------------------------------------
	'processing X
	If mode == MODE_X OR mode == MODE_XY Then
		If size.X < sizeTreshold Then
			If GetParameterBool("hide_by_zero") Then
				c_bg.Active = false
				exit sub
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
				exit sub
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
				exit sub
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
	'positioning
	
	if mode_size == 0 Then
		c_exact_bg = c_source
	elseIf mode_size == 1 then
		c_exact_bg = c_max_child
	elseIf mode_size == 2 then
		c_exact_bg = child
	end if
	
	if GetParameterBool("position_x") OR GetParameterBool("position_y") then
		c_exact_bg.GetTransformedBoundingBox(vSource1, vSource2)
		c_bg.GetTransformedBoundingBox(v1, v2)	
		
		if GetParameterBool("position_x") then
			vSource1 = c_source.LocalPosToWorldPos(vSource1)
			vSource1 = c_bg.WorldPosToLocalPos(vSource1)
			newPosition.x = vSource1.x - (v1.x-c_bg.position.x) + GetParameterDouble("position_x_shift")
		end if
		
		if GetParameterBool("position_y") then
			vSource2 = c_source.LocalPosToWorldPos(vSource2)
			vSource2 = c_bg.WorldPosToLocalPos(vSource2)
			newPosition.y = vSource2.y - (v2.y-c_bg.position.y) + GetParameterDouble("position_y_shift")
		end if
	end if
 	
 
	'-------------------------------------------------------------------------------------------------
	'animation & real size changing
 
	animTreshold = 0.001
	
	if fon_change_mode == 1 then
		newSize.x = 100*newSize.x
		newSize.y = 100*newSize.y
		newSize.z = 100*newSize.z
	end if
	
	if mode == MODE_X OR mode == MODE_XY Then
		if fon_change_mode == 0 then
			if newSize.x<(c_bg.scaling.x-animTreshold) OR newSize.x>(c_bg.scaling.x+animTreshold) Then
				c_bg.scaling.x -= (c_bg.scaling.x - newSize.x)/GetParameterDouble("inertion")
			else
				c_bg.scaling.x = newSize.x
			end if
		elseif fon_change_mode == 1 then
			if newSize.x<(c_bg.geometry.GetParameterDouble("width")-animTreshold) OR newSize.x>(c_bg.geometry.GetParameterDouble("width")+animTreshold) Then
				c_bg.geometry.SetParameterDouble(  "width", c_bg.geometry.GetParameterDouble("width") - (c_bg.geometry.GetParameterDouble("width") - newSize.x)/GetParameterDouble("inertion")  )
			else
				c_bg.geometry.SetParameterDouble(  "width", newSize.x  )
			end if
		end if
	end if
	if mode == MODE_Y OR mode == MODE_XY Then
		if fon_change_mode == 0 then
			if newSize.y<(c_bg.scaling.y-animTreshold) OR newSize.y>(c_bg.scaling.y+animTreshold) Then
				c_bg.scaling.y -= (c_bg.scaling.y - newSize.y)/GetParameterDouble("inertion")
			else
				c_bg.scaling.y = newSize.y
			end if
		elseif fon_change_mode == 1 then
			if newSize.y<(c_bg.geometry.GetParameterDouble("height")-animTreshold) OR newSize.y>(c_bg.geometry.GetParameterDouble("height")+animTreshold) Then
				c_bg.geometry.SetParameterDouble(  "height", c_bg.geometry.GetParameterDouble("height") - (c_bg.geometry.GetParameterDouble("height") - newSize.y)/GetParameterDouble("inertion")  )
			else
				c_bg.geometry.SetParameterDouble(  "height", newSize.y  )
			end if
		end if
	end if
	if mode == MODE_Z Then
		if fon_change_mode == 0 then
			if newSize.z<(c_bg.scaling.z-animTreshold) OR newSize.z>(c_bg.scaling.z+animTreshold) Then
				c_bg.scaling.z -= (c_bg.scaling.z - newSize.z)/GetParameterDouble("inertion")
			else
				c_bg.scaling.z = newSize.z
			end if
		elseif fon_change_mode == 1 then
			if newSize.z<(c_bg.geometry.GetParameterDouble("height")-animTreshold) OR newSize.y>(c_bg.geometry.GetParameterDouble("height")+animTreshold) Then
				c_bg.geometry.SetParameterDouble(  "height", c_bg.geometry.GetParameterDouble("height") - (c_bg.geometry.GetParameterDouble("height") - newSize.z)/GetParameterDouble("inertion")  )
			else
				c_bg.geometry.SetParameterDouble(  "height", newSize.z  )
			end if
		end if
	end if
	
	if GetParameterBool("position_x") then
		if newPosition.x < (c_bg.position.x-animTreshold) OR newPosition.x > (c_bg.position.x+animTreshold) Then
			c_bg.position.x -= (c_bg.position.x - newPosition.x)/GetParameterDouble("inertion")
		else
			c_bg.position.x = newPosition.x
		end if
	end if
	
	if GetParameterBool("position_y") then
		if newPosition.y < (c_bg.position.y-animTreshold) OR newPosition.y > (c_bg.position.y+animTreshold) Then
			c_bg.position.y -= (c_bg.position.y - newPosition.y)/GetParameterDouble("inertion")
		else
			c_bg.position.y = newPosition.y
		end if
	end if
	
	
	'-------------------------------------------------------------------------------------------------
	c_bg.RecomputeMatrix()
End Sub
