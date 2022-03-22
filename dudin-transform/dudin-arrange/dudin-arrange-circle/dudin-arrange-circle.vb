RegisterPluginVersion(1,0,0)
Dim c_center As Container
Dim arr_c_items As Array[Container]
Dim radius, shift, step, center_angle, angle, rad, x, z, rotate_item, manual_radius As Double
Dim rotate_along_circle, radius_by_manual As Boolean
Dim prev_child_count As Integer

sub OnInitParameters()
	RegisterParameterContainer("center", "Center container")
	RegisterParameterBool("radius_by_manual", "Manual radius", false)
	RegisterParameterDouble("manual_radius", "Radius", 100, 0, 999999)
	RegisterParameterDouble("shift", "Shift", 0, -999999, 999999)
	RegisterParameterDouble("step", "Step", 0, 0, 999999)
	RegisterParameterBool("rotate_along_circle", "Rotate along circle", false)
	RegisterParameterDouble("rotate_item", "Rotate item", 0, -999999, 999999)
	RegisterParameterBool("invert", "Invert order", false)
	RegisterPushButton("init", "Init", 1)
end sub

sub OnInit()
	c_center = GetParameterContainer("center")
	arr_c_items.Clear()
	if GetParameterBool("invert") then
		for i=0 to this.ChildContainerCount - 1
			if (this.GetChildContainerByIndex(i).active) then arr_c_items.push(this.GetChildContainerByIndex(i))
		next
	else
		for i=this.ChildContainerCount - 1 to 0 Step -1
			if (this.GetChildContainerByIndex(i).active) then arr_c_items.push(this.GetChildContainerByIndex(i))
		next
	end if
	radius_by_manual = GetParameterBool("radius_by_manual")
	manual_radius = GetParameterDouble("manual_radius")
	shift = GetParameterDouble("shift")
	step = GetParameterDouble("step")
	rotate_item = GetParameterDouble("rotate_item")
	rotate_along_circle = GetParameterBool("rotate_along_circle")
end sub
sub OnParameterChanged(parameterName As String)
	SendGuiParameterShow("manual_radius", CInt(GetParameterBool("radius_by_manual")))
	OnInit()
end sub

sub OnExecPerField()
	if prev_child_count <> this.ChildContainerCount then
		OnInit()
		prev_child_count = this.ChildContainerCount
	end if
	if radius_by_manual Then
		radius = manual_radius
	else
		radius = Distance(this.LocalPosToWorldPos(this.position.xyz), c_center.LocalPosToWorldPos(c_center.position.xyz))
	end if
	center_angle = arr_c_items.size * step / 2.0
	for i=0 to arr_c_items.ubound
		angle = i*step/(radius/1000.0) + shift - center_angle/(radius/1000.0)
		rad = angle * (3.1415926535 / 180)
		x = radius * cos(rad) + c_center.position.x
		z = radius * sin(rad) + c_center.position.z
		arr_c_items[i].position.x = x
		arr_c_items[i].position.z = z
		if rotate_along_circle then arr_c_items[i].rotation.y = -angle + rotate_item
	next
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		OnInit()
	end if
end sub
