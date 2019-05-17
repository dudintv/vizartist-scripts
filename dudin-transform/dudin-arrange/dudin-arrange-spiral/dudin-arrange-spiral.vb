RegisterPluginVersion(1,0,1)
Dim infoText As String = "Place children in spiral form.
Author: Dmitry Dudin"

Dim radius, total_height, circles As Double
Dim prev_count As Integer
Dim xx,yy,zz As Double
Dim PI As Double = 3.1415926535

Dim arr_childs As Array[Container]

Dim randomize As Boolean
Dim random_x, random_y, random_z As Double

sub OnInitParameters()
	RegisterInfoText(infoText)
	RegisterParameterDouble("radius", "Radius", 100, 0, 999999)
	RegisterParameterDouble("total_height", "Total height", 100, 0, 999999)
	RegisterParameterDouble("circles", "Circles", 1, 0, 999999)
	
	RegisterParameterBool("randomize", "Randomize", false)
	RegisterParameterDouble("random_x", "Random X", 0, 0, 999999)
	RegisterParameterDouble("random_y", "Random Y", 0, 0, 999999)
	RegisterParameterDouble("random_z", "Random Z", 0, 0, 999999)
end sub

sub OnInit()
	radius = GetParameterDouble("radius")
	total_height = GetParameterDouble("total_height")
	circles = GetParameterDouble("circles")
	CollectChilds()
	
	randomize = GetParameterBool("randomize")
	random_x = GetParameterDouble("random_x")
	random_y = GetParameterDouble("random_y")
	random_z = GetParameterDouble("random_z")
	
	SendGuiParameterShow("random_x", CInt(randomize))
	SendGuiParameterShow("random_y", CInt(randomize))
	SendGuiParameterShow("random_z", CInt(randomize))
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
	SpiralArrange()
end sub

Sub CollectChilds()
	arr_childs.Clear()
	for i=0 to this.ChildContainerCount-1
		if this.GetChildContainerByIndex(i).Active then
			arr_childs.Push(this.GetChildContainerByIndex(i))
		end if
	next
End Sub

Sub SpiralArrange()
	for i=0 to arr_childs.size-1
		xx = radius*Cos( i/CDbl(arr_childs.size) * 2*PI*circles )
		yy = radius*Sin( i/CDbl(arr_childs.size) * 2*PI*circles )
		zz = i/CDbl(arr_childs.size) * total_height
		arr_childs[i].Position.xyz = CVertex(xx,yy,zz)
		
		if randomize then
			arr_childs[i].Position.x += random_x * Random() - random_x/2.0
			arr_childs[i].Position.y += random_y * Random() - random_y/2.0
			arr_childs[i].Position.z += random_z * Random() - random_z/2.0
		end if
	next
End Sub

sub OnExecPerField()
	CollectChilds()
	if prev_count <> arr_childs.size then
		OnInit()
		SpiralArrange()
		prev_count = arr_childs.size
	end if
end sub
