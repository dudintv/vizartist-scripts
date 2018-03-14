Dim info As String = "Скрипт определяет максимально высокую точку у выбранных контейнеров
и располагает self-контейнер в эту точку (в глобальном измерении)
Разработчик: Дудин Дмитрий. Vizart co.    Версия 1.1 (13 октябрь 2015)
"
 
Dim arr_mode As Array[String]
arr_mode.Push("[ X ]")
arr_mode.Push("[ Y ]")
Dim arr_direction As Array[String]
arr_direction.Push("[ > ]")
arr_direction.Push("[ * ]")
arr_direction.Push("[ < ]")
Dim mode, direction As Integer
Dim arr_c As Array[Container]
Dim c As Container
Dim i As Integer
Dim max, max2, mid As Vertex
Dim v1,v2, v_world, thisSize As Vertex
Dim defY,defX As Double
Dim threshold As Double = 0.01
Dim thresholdMove As Double = 1.0
Dim quantity_of_container As Integer = 1
 
sub OnInitParameters()
	RegisterInfoText(info)
	RegisterRadioButton("mode", "Следить по оси:", 0, arr_mode)
	RegisterRadioButton("direction", "Направление:", 0, arr_direction)
	RegisterParameterDouble("zeroX", "Позиция по X если сам пустой:", 0, -1000.0, 1000.0)
	RegisterParameterDouble("defX", "Позиция по X без смещений:", 0, -1000.0, 1000.0)
	RegisterParameterDouble("shiftX", "Смещение по X со смещением:", 0, -1000.0, 1000.0)
	RegisterParameterDouble("zeroY", "Позиция по Y если сам пустой:", 0, -1000.0, 1000.0)
	RegisterParameterDouble("defY", "Позиция по Y без смещений:", 0, -1000.0, 1000.0)
	RegisterParameterDouble("shiftY", "Смещение по Y со смещением:", 0, -1000.0, 1000.0)
	For i = 1 to quantity_of_container
		RegisterParameterContainer("c" & i,"Контейнер " & i & ":")
	Next
end sub
 
sub OnInit()
	arr_c.Clear()
 
	For i = 1 to quantity_of_container
		c = GetParameterContainer("c" & i)
		If c <> null Then arr_c.Push(c)
	Next
end sub
 
sub OnExecPerField()
 
	mode = GetParameterInt("mode")
	direction = GetParameterInt("direction")
	thisSize = this.GetTransformedBoundingBoxDimensions()
	If mode == 0 Then
		'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
		
		If direction == 0 Then
			max = CVertex(10000000.0,0,0)
			For i = 0 to quantity_of_container-1
				arr_c[i].RecomputeMatrix()
				arr_c[i].GetTransformedBoundingBox(v1,v2)
				If v2.y > (v1.y + threshold) Then
					If v1.x < max.x AND (v2.x > v1.x + threshold OR v2.x < v1.x - threshold) Then max = v1
				End If
			Next
 
			If max.x >= 1000000 Then
				'если не на что ориентировать и надо встать в свое стандартное место
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
			
		ElseIf direction == 1 Then
			max2 = CVertex(-10000000.0,0,0)
			max = CVertex(10000000.0,0,0)
			For i = 0 to quantity_of_container-1
				arr_c[i].RecomputeMatrix()
				arr_c[i].GetTransformedBoundingBox(v1,v2)
 
				If v2.x > (v1.x + threshold) Then
					If v2.x > max2.x AND (v2.x > v1.x + threshold OR v2.x < v1.x - threshold) Then max2 = v2
					If v1.x < max.x AND (v2.x > v1.x + threshold OR v2.x < v1.x - threshold) Then max = v1
				End If
			Next
			
			mid.x = (max.x + max2.x)/2
			mid.x += GetParameterDouble("shiftX")
			max = mid
			
		ElseIf direction == 2 Then
			max = CVertex(-10000000.0,0,0)
			For i = 0 to quantity_of_container-1
				arr_c[i].RecomputeMatrix()
				arr_c[i].GetTransformedBoundingBox(v1,v2)
				If v2.y > (v1.y + threshold) Then
					If v2.x > max.x AND (v2.x > v1.x + threshold OR v2.x < v1.x - threshold) Then max = v2
				End If
			Next
 
			If max.x <= -1000000 Then
				'если не на что ориентировать и надо встать в свое стандартное место
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
		
		'ANIMATE REAL MOVE TO NEW POINT
		max = this.WorldPosToLocalPos(max)
		If Abs(max.x - this.position.x) > thresholdMove Then
			this.position.x += (max.x - this.position.x)/5.0
		End If
 
 
	ElseIf mode == 1 Then
		'YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
 
		If direction == 0 Then
			max = CVertex(0,10000000.0,0)
			For i = 0 to quantity_of_container-1
				arr_c[i].RecomputeMatrix()
				arr_c[i].GetTransformedBoundingBox(v1,v2)
				If v2.x > (v1.x + threshold) Then
					If v1.y < max.y AND (v2.y > v1.y + threshold OR v2.y < v1.y - threshold) Then max = v1
				End If
			Next
 
			If max.y >= 1000000 Then
				'если не на что ориентировать и надо встать в свое стандартное место
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
		ElseIf direction == 1 Then
			max2 = CVertex(0,-10000000.0,0)
			max = CVertex(0,10000000.0,0)
			For i = 0 to quantity_of_container-1
				arr_c[i].RecomputeMatrix()
				arr_c[i].GetTransformedBoundingBox(v1,v2)
 
				If v2.y > (v1.y + threshold) Then
					If v2.y > max2.y AND (v2.y > v1.y + threshold OR v2.y < v1.y - threshold) Then max2 = v2
					If v1.y < max.y AND (v2.y > v1.y + threshold OR v2.y < v1.y - threshold) Then max = v1
				End If
			Next
			
			mid.y = (max.y + max2.y)/2
			mid.y += GetParameterDouble("shiftY")
			max = mid
 
 
		ElseIf direction == 2 Then
			max = CVertex(0,-10000000.0,0)
			For i = 0 to quantity_of_container-1
				arr_c[i].RecomputeMatrix()
				arr_c[i].GetTransformedBoundingBox(v1,v2)
				If v2.x > (v1.x + threshold) Then
					If v2.y > max.y AND (v2.y > v1.y + threshold OR v2.y < v1.y - threshold) Then max = v2
				End If
			Next
 
			If max.y <= -1000000 Then
				'если не на что ориентировать и надо встать в свое стандартное место
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
		
		'ANIMATE REAL MOVE TO NEW POINT
		max = this.WorldPosToLocalPos(max)
		If Abs(max.y - this.position.y) > thresholdMove Then
			this.position.y += (max.y - this.position.y)/1.0
		End If
	End If
end sub
 
sub OnParameterChanged(parameterName As String)
	OnInit()
 
	mode = GetParameterInt("mode")
	If mode == 0 Then
		SendGuiParameterShow("zeroX",SHOW)
		SendGuiParameterShow("zeroY",HIDE)
		SendGuiParameterShow("defX",SHOW)
		SendGuiParameterShow("defY",HIDE)
		SendGuiParameterShow("shiftX",SHOW)
		SendGuiParameterShow("shiftY",HIDE)
	ElseIf mode == 1 Then
		SendGuiParameterShow("zeroX",HIDE)
		SendGuiParameterShow("zeroY",SHOW)
		SendGuiParameterShow("defX",HIDE)
		SendGuiParameterShow("defY",SHOW)
		SendGuiParameterShow("shiftX",HIDE)
		SendGuiParameterShow("shiftY",SHOW)
	End If
end sub