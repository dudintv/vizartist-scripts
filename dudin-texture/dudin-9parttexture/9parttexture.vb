Dim info As String = "The script create asset of nine sub-containers
for stretching central parts and fixing border.

Developer: Dmitry Dudin.  Version 0.5 (24 dec 2018)
"

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterPushButton("create_planes", "Generate nine sub-containers", 1)
	
	RegisterParameterBool("border_simple", "Common border", true)
	RegisterParameterDouble("borders", "Border %", 0, 0, 50)
	RegisterParameterDouble("border_left", "Left border %", 0, 0, 100)
	RegisterParameterDouble("border_top", "Top border %", 0, 0, 100)
	RegisterParameterDouble("border_right", "Right border %", 0, 0, 100)
	RegisterParameterDouble("border_bottom", "Bottom border %", 0, 0, 100)
	
	RegisterParameterBool("size_by_gabarit", "Get size from container", true)
	RegisterParameterDouble("width", "Width", 100, 0, 10000)
	RegisterParameterDouble("height", "Height", 100, 0, 10000)
	RegisterParameterContainer("gabarit", "Size source")
	RegisterParameterDouble("gabarit_multiplyer", "Magnifier", 100, 0, 100000)
end sub

Dim c, cGabarit As Container
Dim arrC As Array[Container]
Dim borderLeft, borderTop, borderRight, borderBottom As Double
Dim width, height, koeficient As Double
Dim posX, posY, posTX, posTY, scaleX, scaleY, scaleTX, scaleTY As Array[Double]

sub OnInit()
end sub
sub OnGeometryChanged(geom As Geometry)
	OnInit()
end sub
sub OnParameterChanged(parameterName As String)
	if GetParameterBool("border_simple") then
		SendGuiParameterShow("borders", 1)
		SendGuiParameterShow("border_left", 0)
		SendGuiParameterShow("border_top", 0)
		SendGuiParameterShow("border_right", 0)
		SendGuiParameterShow("border_bottom", 0)
	else
		SendGuiParameterShow("borders", 0)
		SendGuiParameterShow("border_left", 1)
		SendGuiParameterShow("border_top", 1)
		SendGuiParameterShow("border_right", 1)
		SendGuiParameterShow("border_bottom", 1)
	end if
	if GetParameterBool("size_by_gabarit") then
		SendGuiParameterShow("width", 0)
		SendGuiParameterShow("height", 0)
		SendGuiParameterShow("gabarit", 1)
		SendGuiParameterShow("gabarit_multiplyer", 1)
	else
		SendGuiParameterShow("width", 1)
		SendGuiParameterShow("height", 1)
		SendGuiParameterShow("gabarit", 0)
		SendGuiParameterShow("gabarit_multiplyer", 0)
	end if
	
	Make9Part()
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		this.DeleteChildren()
		for i=1 to 9
			c = this.AddContainer(TL_DOWN)
			c.name = "9part_" & i
			c.CreateGeometry("BUILT_IN*GEOM*Rectangle")
			c.Texture = this.Texture
		next
		Scene.UpdateSceneTree()
	end if
end sub

Sub Make9Part()
	this.GetContainerAndSubContainers(arrC, false)
	arrC.Erase(0)
	
	if GetParameterBool("border_simple") then
		borderLeft = GetParameterDouble("borders")
		borderTop    = borderLeft
		borderRight  = borderLeft
		borderBottom = borderLeft
	else
		borderLeft   = GetParameterDouble("border_left")
		borderTop    = GetParameterDouble("border_top")
		borderRight  = GetParameterDouble("border_right")
		borderBottom = GetParameterDouble("border_bottom")
	end if
	if GetParameterBool("size_by_gabarit") then
		GetGabaritSize()
	else
		width = GetParameterDouble("width")
		height = GetParameterDouble("height")
	end if
	
	posX.Clear()
	posY.Clear()
	posTX.Clear()
	posTY.Clear()
	scaleX.Clear()
	scaleY.Clear()
	scaleTX.Clear()
	scaleTY.Clear()
	
	'----------------------------------------------
	
	scaleX.Push( borderLeft/100.0 )
	scaleX.Push( width/100.0 )
	scaleX.Push( borderRight/100.0 )
	
	scaleY.Push( borderTop/100.0 )
	scaleY.Push( height/100.0 )
	scaleY.Push( borderBottom/100.0 )
	
	scaleTX.Push( 100/borderLeft )
	scaleTX.Push( 100/(100-borderLeft-borderRight) )
	scaleTX.Push( 100/borderRight )
	
	scaleTY.Push( 100/borderTop )
	scaleTY.Push( 100/(100-borderTop-borderBottom) )
	scaleTY.Push( 100/borderBottom )
	
	'-----------------------------------------------
	
	posX.Push( (borderRight + 100*scaleX[1])/2.0 )
	posX.Push( (borderRight-borderLeft)/2.0 )
	posX.Push( -(borderLeft + 100*scaleX[1])/2.0 )
	
	posY.Push( (borderBottom + 100*scaleY[1])/2.0 )
	posY.Push( (borderBottom-borderTop)/2.0 )
	posY.Push( -(borderTop + 100*scaleY[1])/2.0 )
	
	posTX.Push( (50-borderLeft/2.0)/10*scaleTX[0] )
	posTX.Push( (borderRight-borderLeft)/2/10*scaleTX[1] )
	posTX.Push( -(50-borderRight/2.0)/10*scaleTX[2] )
	
	posTY.Push( (50-borderTop/2.0)/10*scaleTY[0] )
	posTY.Push( (borderBottom-borderTop)/2/10*scaleTY[1] )
	posTY.Push( -(50-borderBottom/2.0)/10*scaleTY[2] )
	
	
	for i=0 to 8
		arrC[i].Texture.MapPosition.X =  posTX[ i-3*CInt(i/3) ]
		arrC[i].Texture.MapPosition.Y =  posTY[ CInt(i/3)     ]
		arrC[i].Texture.MapScaling.X = scaleTX[ i-3*CInt(i/3) ]
		arrC[i].Texture.MapScaling.Y = scaleTY[ CInt(i/3)     ]
		
		arrC[i].Position.X =  -posX[ i-3*CInt(i/3) ]
		arrC[i].Position.Y =  -posY[ CInt(i/3)     ]
		arrC[i].Scaling.X =  scaleX[ i-3*CInt(i/3) ]
		arrC[i].Scaling.Y =  scaleY[ CInt(i/3)     ]
	next
End Sub

Dim old_width, old_height As Double
Dim vGabarit1, vGabarit2, old_vGabarit1, old_vGabarit2, vLocalGabarit1, vLocalGabarit2 As Vertex

Sub GetGabaritSize()
	cGabarit = GetParameterContainer("gabarit")
	koeficient = GetParameterDouble("gabarit_multiplyer")/100.0
	
	cGabarit.GetTransformedBoundingBox(vGabarit1, vGabarit2)
	vLocalGabarit1 = this.WorldPosToLocalPos(vGabarit1)
	vLocalGabarit2 = this.WorldPosToLocalPos(vGabarit2)
	width  = vLocalGabarit2.X - vLocalGabarit1.X - borderLeft - borderRight + 10*koeficient
	height = vLocalGabarit2.Y - vLocalGabarit1.Y - borderTop - borderBottom + 10*koeficient
	
	if width < 0 then width = 0
	if height < 0 then height = 0
End Sub

sub OnExecPerField()
	if GetParameterBool("size_by_gabarit") then
		GetGabaritSize()
		
		if old_width <> width OR old_height <> height OR old_vGabarit1 <> vGabarit1 OR old_vGabarit2 <> vGabarit2 then
			Make9Part()
			old_width = width
			old_height = height
			old_vGabarit1 = vGabarit1
			old_vGabarit2 = vGabarit2
		end if
	end if
end sub
