RegisterPluginVersion(1, 0, 0)

Dim c_cur_source, c_new As Container
Dim c_source, c_target, c_to_place As Container
Dim arr_sources, arr_targets As Array[Container]
Dim filling As Double
Dim is_randomized As Boolean
Dim pos_x, pos_y, pos_z, pos_step As Double
Dim rot_x, rot_y, rot_z, rot_step As Double
Dim scale_xyz, scale_step As Double

sub OnInitParameters()
	RegisterParameterContainer("source","Source of objects")
	RegisterParameterContainer("target","Targets")
	RegisterParameterContainer("to_place","To place")
	
	RegisterParameterDouble("filling", "Filling, %", 100, 0, 100)
	
	RegisterParameterBool("is_randomized", "Rand. transform", false)
	RegisterParameterDouble("pos_x", "└ pos x", 0, 0, 999999)
	RegisterParameterDouble("pos_y", "└ pos y", 0, 0, 999999)
	RegisterParameterDouble("pos_z", "└ pos z", 0, 0, 999999)
	RegisterParameterDouble("pos_step", "   * pos step", 0, 0, 999999)
	RegisterParameterDouble("rot_x", "└ rot x", 0, 0, 999999)
	RegisterParameterDouble("rot_y", "└ rot y", 0, 0, 999999)
	RegisterParameterDouble("rot_z", "└ rot z", 0, 0, 999999)
	RegisterParameterDouble("rot_step", "   * rot step", 0, 0, 999999)
	RegisterParameterDouble("scale_xyz", "└ scale xyz", 1, 0, 999999)
	RegisterParameterDouble("scale_step", "   * scale step", 0, 0, 999999)
	
	RegisterPushButton("fill", "Fill", 1)
	RegisterPushButton("clear", "Clear [to_place]", 2)
end sub

Sub FillArrs()
	arr_sources.Clear()
	for i=0 to c_source.ChildContainerCount-1
		if c_source.GetChildContainerByIndex(i).Active then
			arr_sources.Push(c_source.GetChildContainerByIndex(i))
		end if
	next
	arr_targets.Clear()
	for i=0 to c_target.ChildContainerCount-1
		if c_target.GetChildContainerByIndex(i).Active then
			arr_targets.Push(c_target.GetChildContainerByIndex(i))
		end if
	next
End Sub
sub OnInit()
	c_source = GetParameterContainer("source")
	c_target = GetParameterContainer("target")
	c_to_place = GetParameterContainer("to_place")
	FillArrs()
	filling = GetParameterDouble("filling")
	
	is_randomized = GetParameterBool("is_randomized")
	pos_x = GetParameterDouble("pos_x")
	pos_y = GetParameterDouble("pos_y")
	pos_z = GetParameterDouble("pos_z")
	pos_step = GetParameterDouble("pos_step")
	rot_x = GetParameterDouble("rot_x")
	rot_y = GetParameterDouble("rot_y")
	rot_z = GetParameterDouble("rot_z")
	rot_step = GetParameterDouble("rot_step")
	scale_xyz = GetParameterDouble("scale_xyz")
	scale_step = GetParameterDouble("scale_step")
	
	SendGuiParameterShow("pos_x",(Integer)GetParameterBool("is_randomized"))
	SendGuiParameterShow("pos_y",(Integer)GetParameterBool("is_randomized"))
	SendGuiParameterShow("pos_z",(Integer)GetParameterBool("is_randomized"))
	SendGuiParameterShow("pos_step",(Integer)GetParameterBool("is_randomized"))
	SendGuiParameterShow("rot_x",(Integer)GetParameterBool("is_randomized"))
	SendGuiParameterShow("rot_y",(Integer)GetParameterBool("is_randomized"))
	SendGuiParameterShow("rot_z",(Integer)GetParameterBool("is_randomized"))
	SendGuiParameterShow("rot_step",(Integer)GetParameterBool("is_randomized"))
	SendGuiParameterShow("scale_xyz",(Integer)GetParameterBool("is_randomized"))
	SendGuiParameterShow("scale_step",(Integer)GetParameterBool("is_randomized"))
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecAction(buttonId As Integer)
	FillArrs()
	if buttonId == 1 then
		for i=0 to arr_targets.ubound
			if Random(10000)/100.0 < filling then
				c_cur_source = arr_sources[Random(arr_sources.size)]
				c_new = c_cur_source.CopyTo(c_to_place, TL_DOWN)
				c_new.position.xyz = c_to_place.WorldPosToLocalPos( c_target.LocalPosToWorldPos(arr_targets[i].position.xyz) ) + NewPosition()
				c_new.rotation.xyz = arr_targets[i].rotation.xyz + NewRotation()
				c_new.scaling.xyz  = arr_targets[i].scaling.xyz + NewScale()
			end if
		next
	elseif buttonId == 2 then
		for i=0 to c_to_place.ChildContainerCount-1
			c_to_place.GetChildContainerByIndex(i).Delete()
			i-=1
		next
	end if
	Scene.UpdateSceneTree()
end sub

Function NewPosition() As Vertex
	if NOT GetParameterBool("is_randomized") then
		NewPosition = 0
		Exit Function
	end if
	Dim new_v As Vertex
	if pos_step <= 0.0001 then
		new_v.x = pos_x*Random() - pos_x/2.0
		new_v.y = pos_y*Random() - pos_y/2.0
		new_v.z = pos_z*Random() - pos_z/2.0
	else
		new_v.x = pos_step*CInt((pos_x*Random() - pos_x/2.0)/pos_step)
		new_v.y = pos_step*CInt((pos_y*Random() - pos_y/2.0)/pos_step)
		new_v.z = pos_step*CInt((pos_z*Random() - pos_z/2.0)/pos_step)
	end if
	NewPosition = new_v
End Function

Function NewRotation() As Vertex
	if NOT GetParameterBool("is_randomized") then
		NewRotation = 0
		Exit Function
	end if
	Dim new_v As Vertex
	if rot_step <= 0.0001 then
		new_v.x = rot_x*Random() - rot_x/2.0
		new_v.y = rot_y*Random() - rot_y/2.0
		new_v.z = rot_z*Random() - rot_z/2.0
	else
		new_v.x = rot_step*CInt((rot_x*Random() - rot_x/2.0)/rot_step)
		new_v.y = rot_step*CInt((rot_y*Random() - rot_y/2.0)/rot_step)
		new_v.z = rot_step*CInt((rot_z*Random() - rot_z/2.0)/rot_step)
	end if
	NewRotation = new_v
End Function

Function NewScale() As Vertex
	if NOT GetParameterBool("is_randomized") then
		NewScale = 0
		Exit Function
	end if
	Dim new_v As Vertex
	if scale_step <= 0.0001 then
		new_v = Random()*scale_xyz - scale_xyz/2.0
	else
		new_v = scale_step*CInt((Random()*scale_xyz - scale_xyz/2.0)/scale_step)
	end if
	NewScale = new_v
End Function
