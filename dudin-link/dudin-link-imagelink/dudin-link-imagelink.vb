RegisterPluginVersion(1,0,0)

Dim p_imagelink As PluginInstance
Dim arr_c_links As Array[Container]

Dim prev_active As Boolean = false
Dim prev_MapType As Integer = 0
Dim prev_MapPosition, prev_MapRotation, prev_MapScaling As Vertex

sub OnInit()
	p_imagelink = this.GetFunctionPluginInstance("ImageLink")
	arr_c_links.Clear()
	for i=1 to 10
		if p_imagelink.GetParameterContainer("container_" & CStr(i)) <> null then
			arr_c_links.Push( p_imagelink.GetParameterContainer("container_" & CStr(i)) )
		end if
	next
end sub

sub OnExecPerField()
	'if NeedUpdate() then
	if p_imagelink.active then
		for i=0 to arr_c_links.ubound
			arr_c_links[i].Texture.MapPosition = this.Texture.MapPosition
			arr_c_links[i].Texture.MapRotation = this.Texture.MapRotation
			arr_c_links[i].Texture.MapScaling  = this.Texture.MapScaling
			arr_c_links[i].Texture.MapType     = this.Texture.MapType
		next
	end if
end sub

Dim changed As Boolean = false
Function NeedUpdate() As Boolean
	changed = false
	changed = changed OR (prev_active <> p_imagelink.active AND p_imagelink.active)
	changed = changed OR prev_MapType <> this.Texture.MapType
	changed = changed OR prev_MapPosition.x <> this.Texture.MapPosition.x
	changed = changed OR prev_MapPosition.y <> this.Texture.MapPosition.y
	changed = changed OR prev_MapRotation.z <> this.Texture.MapRotation.z
	changed = changed OR prev_MapScaling.x  <> this.Texture.MapScaling.x
	changed = changed OR prev_MapScaling.y  <> this.Texture.MapScaling.y
	changed = changed OR prev_MapScaling.z  <> this.Texture.MapScaling.z
	
	prev_active = p_imagelink.active
	prev_MapType = this.Texture.MapType
	prev_MapPosition.x = this.Texture.MapPosition.x
	prev_MapPosition.y = this.Texture.MapPosition.y
	prev_MapRotation.z = this.Texture.MapRotation.z
	prev_MapScaling.x  = this.Texture.MapScaling.x
	prev_MapScaling.y  = this.Texture.MapScaling.y
	prev_MapScaling.z  = this.Texture.MapScaling.z
	
	NeedUpdate = changed
End Function
