Dim info As String = "Autofollow for multiple containers.
Check maximum point of selected containers
and align this_container to this point (in global).
Developer: Dmitry Dudin, version 1.2 (09.01.2019)
"

' INTERFACE
Dim arr_mode As Array[String]
arr_mode.Push("[ X ]")
arr_mode.Push("[ Y ]")
Dim arr_direction As Array[String]
arr_direction.Push("[ > ]")
arr_direction.Push("[ * ]")
arr_direction.Push("[ < ]")
Dim arr_c As Array[Container]

' STUFF
Dim c As Container
Dim i As Integer
'use vertexes for easy world to local transformation
Dim min, max, mid As Vertex
Dim v1,v2, v_world, thisSize As Vertex
Dim defY,defX As Double

' SETTINGS
' just set up count of container for observing
Dim quantity_of_container As Integer = 1
Dim threshold As Double = 0.01
Dim thresholdMove As Double = 1.0
 
sub OnInitParameters()
	RegisterInfoText(info)
	RegisterRadioButton("mode", "Follow axis:", 0, arr_mode)
	RegisterRadioButton("direction", "Direction:", 0, arr_direction)
	RegisterParameterDouble("zeroX", "X-pos if self empty:", 0, -1000.0, 1000.0)
	RegisterParameterDouble("defX", "X-pos default:", 0, -1000.0, 1000.0)
	RegisterParameterDouble("shiftX", "X-shift:", 0, -1000.0, 1000.0)
	RegisterParameterDouble("zeroY", "Y-pos if self empty:", 0, -1000.0, 1000.0)
	RegisterParameterDouble("defY", "Y-pos default:", 0, -1000.0, 1000.0)
	RegisterParameterDouble("shiftY", "Y-shift:", 0, -1000.0, 1000.0)
	For i = 1 to quantity_of_container
		RegisterParameterContainer("c" & i,"Container " & i & ":")
	Next
end sub
 
sub OnInit()
	arr_c.Clear()
 
	For i = 1 to quantity_of_container
		c = GetParameterContainer("c" & i)
		If c <> null Then arr_c.Push(c)
	Next
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
	If GetParameterInt("mode") == 0 Then
		SendGuiParameterShow("zeroX",SHOW)
		SendGuiParameterShow("zeroY",HIDE)
		SendGuiParameterShow("defX",SHOW)
		SendGuiParameterShow("defY",HIDE)
		SendGuiParameterShow("shiftX",SHOW)
		SendGuiParameterShow("shiftY",HIDE)
	ElseIf GetParameterInt("mode") == 1 Then
		SendGuiParameterShow("zeroX",HIDE)
		SendGuiParameterShow("zeroY",SHOW)
		SendGuiParameterShow("defX",HIDE)
		SendGuiParameterShow("defY",SHOW)
		SendGuiParameterShow("shiftX",HIDE)
		SendGuiParameterShow("shiftY",SHOW)
	End If
end sub
 
sub OnExecPerField()
	thisSize = this.GetTransformedBoundingBoxDimensions()
	
	If GetParameterInt("mode") == 0 Then
		'MODE X
		
		If GetParameterInt("direction") == 0 Then
			max = CVertex(10000000.0,0,0)
			For i = 0 to quantity_of_container-1
				arr_c[i].RecomputeMatrix()
				arr_c[i].GetTransformedBoundingBox(v1,v2)
				If v2.y > (v1.y + threshold) Then
					If v1.x < max.x AND (v2.x > v1.x + threshold OR v2.x < v1.x - threshold) Then max = v1
				End If
			Next
 
			If max.x >= 1000000 Then
				'go to default pos
				defX = GetParameterDouble("defX")
				If this.position.x > defX - threshold AND this.position.x < defX + threshold Then
					max.x = defX
				Else
					v_world = LocalPosToWorldPos(this.position.xyz)
					max.x = v_world.x - ((v_world.x - defX)/5.0)
				End If
			Else
				max.x += GetParameterDouble("shiftX")
			End If
			
		ElseIf GetParameterInt("direction") == 1 Then
			max = CVertex(-10000000.0,0,0)
			min = CVertex(10000000.0,0,0)
			For i = 0 to quantity_of_container-1
				arr_c[i].RecomputeMatrix()
				arr_c[i].GetTransformedBoundingBox(v1,v2)
 
				If v2.x > (v1.x + threshold) Then
					If v2.x > max.x AND (v2.x > v1.x + threshold OR v2.x < v1.x - threshold) Then max = v2
					If v1.x < min.x AND (v2.x > v1.x + threshold OR v2.x < v1.x - threshold) Then min = v1
				End If
			Next
			
			mid.x = (min.x + max.x)/2
			mid.x += GetParameterDouble("shiftX")
			max = mid
			
		ElseIf GetParameterInt("direction") == 2 Then
			max = CVertex(-10000000.0,0,0)
			For i = 0 to quantity_of_container-1
				arr_c[i].RecomputeMatrix()
				arr_c[i].GetTransformedBoundingBox(v1,v2)
				If v2.y > (v1.y + threshold) Then
					If v2.x > max.x AND (v2.x > v1.x + threshold OR v2.x < v1.x - threshold) Then max = v2
				End If
			Next
 
			If max.x <= -1000000 Then
				'go to default pos
				defX = GetParameterDouble("defX")
				If this.position.x > defX - threshold AND this.position.x < defX + threshold Then
					max.x = defX
				Else
					v_world = LocalPosToWorldPos(this.position.xyz)
					max.x = v_world.x - ((v_world.x - defX)/5.0)
				End If
			Else
				max.x += GetParameterDouble("shiftX")
			End If
		End If
		
		if thisSize.y < (0 + threshold) OR thisSize.x < (0 + threshold) then
			max.x = GetParameterDouble("zeroX")
		end if
		
		'animate real move to new point
		max = this.WorldPosToLocalPos(max)
		If Abs(max.x - this.position.x) > thresholdMove Then
			this.position.x += (max.x - this.position.x)/5.0
		End If
 
 
	ElseIf GetParameterInt("mode") == 1 Then
		'MODE Y
 
		If GetParameterInt("direction") == 0 Then
			max = CVertex(0,10000000.0,0)
			For i = 0 to quantity_of_container-1
				arr_c[i].RecomputeMatrix()
				arr_c[i].GetTransformedBoundingBox(v1,v2)
				If v2.x > (v1.x + threshold) Then
					If v1.y < max.y AND (v2.y > v1.y + threshold OR v2.y < v1.y - threshold) Then max = v1
				End If
			Next
 
			If max.y >= 1000000 Then
				'go to default pos
				defY = GetParameterDouble("defY")
				If this.position.y > defY - threshold AND this.position.y < defY + threshold Then
					max.y = defY
				Else
					v_world = LocalPosToWorldPos(this.position.xyz)
					max.y = v_world.y - ((v_world.y - defY)/2.0)
				End If
			Else
				max.y += GetParameterDouble("shiftY")
			End If
		ElseIf GetParameterInt("direction") == 1 Then
			max = CVertex(0,-10000000.0,0)
			min = CVertex(0,10000000.0,0)
			For i = 0 to quantity_of_container-1
				arr_c[i].RecomputeMatrix()
				arr_c[i].GetTransformedBoundingBox(v1,v2)
 
				If v2.y > (v1.y + threshold) Then
					If v2.y > max.y AND (v2.y > v1.y + threshold OR v2.y < v1.y - threshold) Then max = v2
					If v1.y < min.y AND (v2.y > v1.y + threshold OR v2.y < v1.y - threshold) Then min = v1
				End If
			Next
			
			mid.y = (min.y + max.y)/2
			mid.y += GetParameterDouble("shiftY")
			max = mid
 
 
		ElseIf GetParameterInt("direction") == 2 Then
			max = CVertex(0,-10000000.0,0)
			For i = 0 to quantity_of_container-1
				arr_c[i].RecomputeMatrix()
				arr_c[i].GetTransformedBoundingBox(v1,v2)
				If v2.x > (v1.x + threshold) Then
					If v2.y > max.y AND (v2.y > v1.y + threshold OR v2.y < v1.y - threshold) Then max = v2
				End If
			Next
 
			If max.y <= -1000000 Then
				'go to default pos
				defY = GetParameterDouble("defY")
				If this.position.y > defY - threshold AND this.position.y < defY + threshold Then
					max.y = defY
				Else
					v_world = LocalPosToWorldPos(this.position.xyz)
					max.y = v_world.y - ((v_world.y - defY)/2.0)
				End If
			Else
				max.y += GetParameterDouble("shiftY")
			End If
			
		End If
		
		if thisSize.y < (0 + threshold) OR thisSize.x < (0 + threshold) then
			max.y = GetParameterDouble("zeroY")
		end if
		
		'animate real move to new point
		max = this.WorldPosToLocalPos(max)
		If Abs(max.y - this.position.y) > thresholdMove Then
			this.position.y += (max.y - this.position.y)/1.0
		End If
	End If
end sub
