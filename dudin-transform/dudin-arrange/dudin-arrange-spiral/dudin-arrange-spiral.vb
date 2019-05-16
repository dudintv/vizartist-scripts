RegisterPluginVersion(1,0,0)
Dim infoText As String = "Place children in spiral form.
Author: Dmitry Dudin"

Dim radius, total_height, circles As Double
Dim count As Integer
Dim xx,yy,zz As Double
Dim PI As Double = 3.1415926535

sub OnInitParameters()
	RegisterInfoText(infoText)
	RegisterParameterDouble("radius", "Radius", 100, 0, 999999)
	RegisterParameterDouble("total_height", "Total height", 100, 0, 999999)
	RegisterParameterDouble("circles", "Circles", 1, 0, 999999)
end sub

sub OnInit()
	radius = GetParameterDouble("radius")
	total_height = GetParameterDouble("total_height")
	circles = GetParameterDouble("circles")
	count = this.ChildContainerCount
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
	SpiralPlace()
end sub

Sub SpiralPlace()
	for i=0 to count-1
		xx = radius*Cos( i/CDbl(count) * 2*PI*circles )
		yy = radius*Sin( i/CDbl(count) * 2*PI*circles )
		zz = i/CDbl(count) * total_height
		this.GetChildContainerByIndex(i).Position.xyz = CVertex(xx,yy,zz)
	next
End Sub
