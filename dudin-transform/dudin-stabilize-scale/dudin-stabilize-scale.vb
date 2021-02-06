Dim target_size As Double = 1.0
Dim scale As Vertex

sub OnExecPerField()
	scale = this.ParentContainer.Matrix.GetScaling()
	this.scaling.xyz = CVertex(target_size/scale.x, target_size/scale.y, target_size/scale.z)
end sub
