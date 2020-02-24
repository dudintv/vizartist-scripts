RegisterPluginVersion(1,0,0)
Dim info As String = "Syncronize parameters from this container to another"

Dim pos_prev, rot_prev, scale_prev As Vertex
Dim a_prev As Double
Dim c As Container

sub OnInitParameters()
	RegisterParameterContainer("c", "Link to")
	RegisterParameterBool("pos", "Position", true)
	RegisterParameterBool("rot", "Rotation", true)
	RegisterParameterBool("scale", "Scale", true)
	RegisterParameterBool("a", "Alpha", true)
end sub
sub OnInit()
	c = GetParameterContainer("c")
	c.position.xyz = this.position.xyz
	c.rotation.xyz = this.rotation.xyz
	c.scaling.xyz  = this.scaling.xyz
	c.alpha.value  = this.alpha.value
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	if NeedUpdate() then
		if GetParameterBool("pos")   then c.position.xyz = this.position.xyz
		if GetParameterBool("rot")   then c.rotation.xyz = this.rotation.xyz
		if GetParameterBool("scale") then c.scaling.xyz  = this.scaling.xyz
		if GetParameterBool("a")     then c.alpha.value  = this.alpha.value
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
		
	NeedUpdate = result
End Function
