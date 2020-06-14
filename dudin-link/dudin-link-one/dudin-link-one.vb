RegisterPluginVersion(1,1,0)
Dim info As String = "Syncronize parameters from this container to another"

Dim pos_prev, rot_prev, scale_prev As Vertex
Dim a_prev, width_prev, height_prev As Double
Dim c As Container
Dim arr_c As Array[Container]
Dim p_geom As PluginInstance

Dim count_target_containers As Integer = 2

sub OnInitParameters()
	for i=1 to count_target_containers
		RegisterParameterContainer("c"&i, "Link #"&i&" to")
	next
	RegisterParameterBool("pos", "Position", false)
	RegisterParameterBool("rot", "Rotation", false)
	RegisterParameterBool("scale", "Scale", false)
	RegisterParameterBool("a", "Alpha", false)
	
	RegisterParameterBool("width", "Width", false)
	RegisterParameterBool("height", "Height", false)
	
	p_geom = this.GetGeometryPluginInstance()
end sub
sub OnInit()
	arr_c.Clear()
	for i=1 to count_target_containers
		c = GetParameterContainer("c"&i)
		arr_c.Push(c)
	
		c.position.xyz = this.position.xyz
		c.rotation.xyz = this.rotation.xyz
		c.scaling.xyz  = this.scaling.xyz
		c.alpha.value  = this.alpha.value
		
		if p_geom <> null then
			c.GetGeometryPluginInstance().SetParameterDouble("width", p_geom.GetParameterDouble("width"))
			c.GetGeometryPluginInstance().SetParameterDouble("height", p_geom.GetParameterDouble("height"))
		end if
	next
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	if NeedUpdate() then
		for i=1 to count_target_containers
			c = arr_c[i-1]
			
			if GetParameterBool("pos")   then c.position.xyz = this.position.xyz
			if GetParameterBool("rot")   then c.rotation.xyz = this.rotation.xyz
			if GetParameterBool("scale") then c.scaling.xyz  = this.scaling.xyz
			if GetParameterBool("a")     then c.alpha.value  = this.alpha.value
			
			if p_geom <> null then
				c.GetGeometryPluginInstance().SetParameterDouble("width", p_geom.GetParameterDouble("width"))
				c.GetGeometryPluginInstance().SetParameterDouble("height", p_geom.GetParameterDouble("height"))
			end if
		next
	end if
end sub

Dim result as Boolean
Function NeedUpdate() AS Boolean
	result = false
	if GetParameterBool("pos")   AND this.position.xyz <> pos_prev   then result = true
	if GetParameterBool("rot")   AND this.rotation.xyz <> rot_prev   then result = true
	if GetParameterBool("scale") AND this.scaling.xyz  <> scale_prev then result = true
	if GetParameterBool("a")     AND this.alpha.value  <> a_prev     then result = true
	
	pos_prev   = this.position.xyz
	rot_prev   = this.rotation.xyz
	scale_prev = this.scaling.xyz
	a_prev     = this.alpha.value
	
	if p_geom <> null then
		if GetParameterBool("width")  AND p_geom.GetParameterDouble("width")  <> width_prev  then result = true
		if GetParameterBool("height") AND p_geom.GetParameterDouble("height") <> height_prev then result = true
		
		width_prev  = p_geom.GetParameterDouble("width")
		height_prev = p_geom.GetParameterDouble("height")
	end if
		
	NeedUpdate = result
End Function
