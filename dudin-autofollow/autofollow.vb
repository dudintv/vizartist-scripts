RegisterPluginVersion(1,3,0)
Dim info As String = "Autofollow for multiple containers.
Check maximum point of selected containers
and align this_container to this point (in global).
Developer: Dmitry Dudin, dudin.tv
"

' SETTINGS
' just set up count of container for observing
Dim quantity_of_container As Integer = 1
Dim thresholdZero As Double = 0.001
Dim thresholdMove As Double = 0.01
Dim animDamping As Double = 1.0

' INTERFACE
Dim arr_mode As Array[String]
arr_mode.Push("[ X ]")
arr_mode.Push("[ Y ]")
arr_mode.Push("[ Z ]")

Dim MODE_X As Integer = 0
Dim MODE_Y As Integer = 1
Dim MODE_Z As Integer = 2

Dim arr_direction As Array[String]
arr_direction.Push("[ > ]")
arr_direction.Push("[ * ]")
arr_direction.Push("[ < ]")
Dim arr_c, arr_sized As Array[Container]

' STUFF
Dim c As Container
Dim i As Integer
'use vertexes for easy world to local transformation
Dim min, max, mid, new As Vertex
Dim v1,v2, v_world, thisSize As Vertex
Dim defY,defX As Double
 
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
	Select Case GetParameterInt("mode")
	Case MODE_X
		SendGuiParameterShow("zeroX",SHOW)
		SendGuiParameterShow("zeroY",HIDE)
		SendGuiParameterShow("zeroZ",HIDE)
		SendGuiParameterShow("defX",SHOW)
		SendGuiParameterShow("defY",HIDE)
		SendGuiParameterShow("defZ",HIDE)
		SendGuiParameterShow("shiftX",SHOW)
		SendGuiParameterShow("shiftY",HIDE)
		SendGuiParameterShow("shiftZ",HIDE)
	Case MODE_Y
		SendGuiParameterShow("zeroX",HIDE)
		SendGuiParameterShow("zeroY",SHOW)
		SendGuiParameterShow("zeroZ",HIDE)
		SendGuiParameterShow("defX",HIDE)
		SendGuiParameterShow("defY",SHOW)
		SendGuiParameterShow("defZ",HIDE)
		SendGuiParameterShow("shiftX",HIDE)
		SendGuiParameterShow("shiftY",SHOW)
		SendGuiParameterShow("shiftZ",HIDE)
	Case MODE_Z
		SendGuiParameterShow("zeroX",HIDE)
		SendGuiParameterShow("zeroY",HIDE)
		SendGuiParameterShow("zeroZ",SHOW)
		SendGuiParameterShow("defX",HIDE)
		SendGuiParameterShow("defY",HIDE)
		SendGuiParameterShow("defZ",SHOW)
		SendGuiParameterShow("shiftX",HIDE)
		SendGuiParameterShow("shiftY",HIDE)
		SendGuiParameterShow("shiftZ",SHOW)
	End Select
end sub

Function IsHaveSize(_c As Container) As Boolean
	_c.RecomputeMatrix()
	_c.GetTransformedBoundingBox(v1,v2)
	IsHaveSize = (v2.x > v1.x + thresholdZero OR v2.x < v1.x - thresholdZero) AND (v2.y > v1.y + thresholdZero OR v2.y < v1.y - thresholdZero)
End Function


'''''''''''''''''''''''''''''''''''''''''''''''''''''''''
 
sub OnExecPerField()
	thisSize = this.GetTransformedBoundingBoxDimensions()
	
	arr_sized.Clear
	For i = 0 to arr_c.ubound
		if IsHaveSize(arr_c[i]) then arr_sized.Push(arr_c[i])
	Next
	
	Select Case GetParameterInt("mode")
	Case MODE_X
		If IsHaveSize(this) Then
			If arr_sized.size > 0 Then
				If GetParameterInt("direction") == 0 Then
					arr_sized[0].GetTransformedBoundingBox(v1,v2)
					new = v1
					For i = 1 to arr_sized.ubound
						arr_sized[i].GetTransformedBoundingBox(v1,v2)
						If v1.x < new.x Then new = v1
					Next
					new.x += GetParameterDouble("shiftX")
				ElseIf GetParameterInt("direction") == 1 Then
					arr_sized[0].GetTransformedBoundingBox(v1,v2)
					max = v2
					min = v1
					For i = 1 to arr_sized.ubound
						arr_c[i].GetTransformedBoundingBox(v1,v2)
						If v2.x > max.x Then max = v2
						If v1.x < min.x Then min = v1
					Next
					new.x = (min.x + max.x)/2 + GetParameterDouble("shiftX")
				ElseIf GetParameterInt("direction") == 2 Then
					arr_sized[0].GetTransformedBoundingBox(v1,v2)
					new = v2
					For i = 1 to arr_sized.ubound
						arr_sized[i].GetTransformedBoundingBox(v1,v2)
						If v2.x > new.x Then new = v2
					Next
					new.x += GetParameterDouble("shiftX")
				End If
			Else
				'go to default pos
				new.x = GetParameterDouble("defX")
			End If
		Else
			'this is empty
			new.x = GetParameterDouble("zeroX")
		End If
 
	Case MODE_Y
		If IsHaveSize(this) Then
			If arr_sized.size > 0 Then
				If GetParameterInt("direction") == 0 Then
					arr_sized[0].GetTransformedBoundingBox(v1,v2)
					new = v1
					For i = 1 to arr_sized.ubound
						arr_sized[i].GetTransformedBoundingBox(v1,v2)
						If v1.y < new.y Then new = v1
					Next
					new.y += GetParameterDouble("shiftY")
				ElseIf GetParameterInt("direction") == 1 Then
					arr_sized[0].GetTransformedBoundingBox(v1,v2)
					max = v2
					min = v1
					For i = 1 to arr_sized.ubound
						arr_c[i].GetTransformedBoundingBox(v1,v2)
						If v2.y > max.y Then max = v2
						If v1.y < min.y Then min = v1
					Next
					new.y = (min.y + max.y)/2 + GetParameterDouble("shiftY")
				ElseIf GetParameterInt("direction") == 2 Then
					arr_sized[0].GetTransformedBoundingBox(v1,v2)
					new = v2
					For i = 1 to arr_sized.ubound
						arr_sized[i].GetTransformedBoundingBox(v1,v2)
						If v2.y > new.y Then new = v2
					Next
					new.y += GetParameterDouble("shiftY")
				End If
			Else
				'go to default pos
				new.y = GetParameterDouble("defY")
			End If
		Else
			'this is empty
			new.y = GetParameterDouble("zeroY")
		End If
	
	Case MODE_Z
		If IsHaveSize(this) Then
			If arr_sized.size > 0 Then
				If GetParameterInt("direction") == 0 Then
					arr_sized[0].GetTransformedBoundingBox(v1,v2)
					new = v1
					For i = 1 to arr_sized.ubound
						arr_sized[i].GetTransformedBoundingBox(v1,v2)
						If v1.z < new.z Then new = v1
					Next
					new.z += GetParameterDouble("shiftZ")
				ElseIf GetParameterInt("direction") == 1 Then
					arr_sized[0].GetTransformedBoundingBox(v1,v2)
					max = v2
					min = v1
					For i = 1 to arr_sized.ubound
						arr_c[i].GetTransformedBoundingBox(v1,v2)
						If v2.z > max.z Then max = v2
						If v1.z < min.z Then min = v1
					Next
					new.z = (min.z + max.z)/2 + GetParameterDouble("shiftZ")
				ElseIf GetParameterInt("direction") == 2 Then
					arr_sized[0].GetTransformedBoundingBox(v1,v2)
					new = v2
					For i = 1 to arr_sized.ubound
						arr_sized[i].GetTransformedBoundingBox(v1,v2)
						If v2.z > new.z Then new = v2
					Next
					new.z += GetParameterDouble("shiftZ")
				End If
			Else
				'go to default pos
				new.z = GetParameterDouble("defZ")
			End If
		Else
			'this is empty
			new.z = GetParameterDouble("zeroZ")
		End If
	End Select
	
	'animate to new point
	new = this.WorldPosToLocalPos(new)
	Select Case GetParameterInt("mode")
	Case MODE_X
		If Abs(new.x - this.position.x) > thresholdMove Then
			this.position.x += (new.x - this.position.x)/animDamping
		End If
	Case MODE_Y
		If Abs(new.y - this.position.y) > thresholdMove Then
			this.position.y += (new.y - this.position.y)/animDamping
		End If
	Case MODE_Z
		If Abs(new.z - this.position.z) > thresholdMove Then
			this.position.z += (new.z - this.position.z)/animDamping
		End If
	End Select
end sub
