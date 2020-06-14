RegisterPluginVersion(1,0,0)

Dim c_source, c_target As Container
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
	FillPrevs()
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

Sub FillPrevs()
	for i=0 to c_source.ChildContainerCount - 1
		Dim this_prev As Prev
		this_prev.active = c_source.GetChildContainerByIndex(i).active
		this_prev.pos    = c_source.GetChildContainerByIndex(i).position.xyz
		this_prev.rot    = c_source.GetChildContainerByIndex(i).rotation.xyz
		this_prev.scale  = c_source.GetChildContainerByIndex(i).scaling.xyz
		this_prev.alpha  = c_source.GetChildContainerByIndex(i).alpha.value
		this_prev.text   = c_source.GetChildContainerByIndex(i).geometry.text
		arr_prev.Push(this_prev)
	next
End Sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		' copy
		c_target.DeleteChildren()
		arr_prev.Clear()
		for i = c_source.ChildContainerCount-1 to 0 step -1
			c_source.GetChildContainerByIndex(i).CopyTo(c_target, TL_DOWN)
		next
		FillPrevs()
		scene.UpdateSceneTree()
	end if
end sub

sub OnExecPerField()
	if c_source.ChildContainerCount <> c_target.ChildContainerCount then exit sub
	
	for i=0 to c_source.ChildContainerCount - 1
		' active
		if GetParameterBool("active") then
			new_active = c_source.GetChildContainerByIndex(i).active
			if arr_prev[i].active <> new_active then
				c_target.GetChildContainerByIndex(i).active = new_active
				arr_prev[i].active = new_active
			end if
		end if
		
		'position
		if GetParameterBool("pos") then
			new_pos = c_source.GetChildContainerByIndex(i).position.xyz
			if arr_prev[i].pos <> new_pos then
				c_target.GetChildContainerByIndex(i).position.xyz = new_pos
				arr_prev[i].pos = new_pos
			end if
		end if
		
		'rotation
		if GetParameterBool("rot") then
			new_rot = c_source.GetChildContainerByIndex(i).rotation.xyz
			if arr_prev[i].rot <> new_rot then
				c_target.GetChildContainerByIndex(i).rotation.xyz = new_rot
				arr_prev[i].rot = new_rot
			end if
		end if
		
		'scale
		if GetParameterBool("scale") then
			new_scale = c_source.GetChildContainerByIndex(i).scaling.xyz
			if arr_prev[i].scale <> new_scale then
				c_target.GetChildContainerByIndex(i).scaling.xyz = new_scale
				arr_prev[i].scale = new_scale
			end if
		end if
		
		'alpha
		if GetParameterBool("alpha") then
			new_alpha = c_source.GetChildContainerByIndex(i).alpha.value
			if arr_prev[i].alpha <> new_alpha then
				c_target.GetChildContainerByIndex(i).alpha.value = new_alpha
				arr_prev[i].alpha = new_alpha
			end if
		end if
		
		'text
		if GetParameterBool("text") then
			new_text = c_source.GetChildContainerByIndex(i).geometry.text
			if arr_prev[i].text <> new_text then
				c_target.GetChildContainerByIndex(i).geometry.text = new_text
				arr_prev[i].text = new_text
			end if
		end if
	next
end sub
