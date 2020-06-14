RegisterPluginVersion(1,1,0)

Dim c_source, c_target As Container
Dim arr_source, arr_target As Array[Container]

Dim new_active As Boolean
Dim new_pos, new_rot, new_scale As Vertex
Dim new_alpha As Double
Dim new_text As String

Structure Prev
	active As Boolean
	pos As Vertex
	rot As Vertex
	scale As Vertex
	alpha As Double
	text As String
End Structure
Dim arr_prev As Array[Prev]

sub OnInitParameters()
	RegisterParameterContainer("source", "Source")
	RegisterParameterContainer("target", "Target")
	RegisterPushButton("copy", "Make copy", 1)
	RegisterParameterBool("active", "Sync active", false)
	RegisterParameterBool("pos", "Sync position", false)
	RegisterParameterBool("rot", "Sync rotation", false)
	RegisterParameterBool("scale", "Sync scale", false)
	RegisterParameterBool("alpha", "Sync alpha", false)
	RegisterParameterBool("text", "Sync text", false)
end sub

sub OnInit()
	c_source = GetParameterContainer("source")
	c_target = GetParameterContainer("target")
	FillArrays()
	FillPrevs()
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

Sub FillArrays()
	arr_source.Clear()
	for i=0 to c_source.ChildContainerCount - 1
		PushContainers(arr_source, c_source.GetChildContainerByIndex(i))
	next
	arr_target.Clear()
	for i=0 to c_target.ChildContainerCount - 1
		PushContainers(arr_target, c_target.GetChildContainerByIndex(i))
	next
End Sub

Sub PushContainers(arr_c as Array[Container], c As Container)
	if c.ChildContainerCount > 0 then
		arr_c.Push(c)
		for i=0 to c.ChildContainerCount - 1
			PushContainers(arr_c, c.GetChildContainerByIndex(i))
		next
	else
		arr_c.Push(c)
	end if
End Sub

Sub FillPrevs()
	arr_prev.Clear()
	for i=0 to arr_source.ubound
		Dim this_prev As Prev
		this_prev.active = arr_source[i].active
		this_prev.pos    = arr_source[i].position.xyz
		this_prev.rot    = arr_source[i].rotation.xyz
		this_prev.scale  = arr_source[i].scaling.xyz
		this_prev.alpha  = arr_source[i].alpha.value
		this_prev.text   = arr_source[i].geometry.text
		arr_prev.Push(this_prev)
	next
End Sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		' copy
		c_target.DeleteChildren()
		for i = c_source.ChildContainerCount-1 to 0 step -1
			c_source.GetChildContainerByIndex(i).CopyTo(c_target, TL_DOWN)
		next
		FillArrays()
		FillPrevs()
		scene.UpdateSceneTree()
	end if
end sub

sub OnExecPerField()
	if arr_source.size <> arr_target.size then exit sub
	
	for i=0 to arr_source.ubound
		' active
		if GetParameterBool("active") then
			new_active = arr_source[i].active
			if arr_prev[i].active <> new_active then
				arr_target[i].active = new_active
				arr_prev[i].active = new_active
			end if
		end if
		
		'position
		if GetParameterBool("pos") then
			new_pos = arr_source[i].position.xyz
			if arr_prev[i].pos <> new_pos then
				arr_target[i].position.xyz = new_pos
				arr_prev[i].pos = new_pos
			end if
		end if
		
		'rotation
		if GetParameterBool("rot") then
			new_rot = arr_source[i].rotation.xyz
			if arr_prev[i].rot <> new_rot then
				arr_target[i].rotation.xyz = new_rot
				arr_prev[i].rot = new_rot
			end if
		end if
		
		'scale
		if GetParameterBool("scale") then
			new_scale = arr_source[i].scaling.xyz
			if arr_prev[i].scale <> new_scale then
				arr_target[i].scaling.xyz = new_scale
				arr_prev[i].scale = new_scale
			end if
		end if
		
		'alpha
		if GetParameterBool("alpha") then
			new_alpha = arr_source[i].alpha.value
			if arr_prev[i].alpha <> new_alpha then
				arr_target[i].alpha.value = new_alpha
				arr_prev[i].alpha = new_alpha
			end if
		end if
		
		'text
		if GetParameterBool("text") then
			new_text = arr_source[i].geometry.text
			if arr_prev[i].text <> new_text then
				arr_target[i].geometry.text = new_text
				arr_prev[i].text = new_text
			end if
		end if
	next
end sub
