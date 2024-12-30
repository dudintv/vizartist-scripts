RegisterPluginVersion(1,6,0)
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

' INTERFACE
Dim arr_mode As Array[String]
arr_mode.Push(" X ")
arr_mode.Push(" Y ")
arr_mode.Push(" Z ")

Dim MODE_X As Integer = 0
Dim MODE_Y As Integer = 1
Dim MODE_Z As Integer = 2

Dim arr_direction As Array[String]
arr_direction.Push(" *— ")
arr_direction.Push(" -*- ")
arr_direction.Push(" —* ")
Dim arr_c, arr_sized As Array[Container]

Dim arrPauseMode As Array[String]
arrPauseMode.Push("none")
arrPauseMode.Push("<-")
arrPauseMode.Push("->")
arrPauseMode.Push("<->")

Dim PAUSE_MODE_NONE As Integer = 0
Dim PAUSE_MODE_LESS As Integer = 1
Dim PAUSE_MODE_MORE As Integer = 2
Dim PAUSE_MODE_BOTH As Integer = 3

' STUFF
Dim c As Container
Dim i As Integer
'use vertexes for easy world to local transformation
Dim min, max, mid, new As Vertex
Dim v1, v2, v_world, thisSize As Vertex
Dim defY, defX, toDefault As Double

' for sub-container targets
Dim isChildMode, isRealtimeRetarget As Boolean


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
	RegisterParameterDouble("toDefault", "To default (0-100%):", 0, 0, 100.0)
	RegisterParameterDouble("inertia", "Itertia (1=none)", 1, 1, 1000.0)
	RegisterRadioButton("pause_mode", "└ Pause direction", 0, arrPauseMode)
	RegisterParameterInt("pause_less", "   └ Pause <- (frames)", 0, 0, 1000)
	RegisterParameterInt("pause_more", "   └ Pause -> (frames)", 0, 0, 1000)
	RegisterParameterBool("is_child_mode", "Follow sub-containers by name", false)
	RegisterParameterBool("is_realtime_retarget", "└ Realtime re-search containers", true)
	For i = 1 to quantity_of_container
		RegisterParameterContainer("c" & i,"Container " & i & ":")
		RegisterParameterString("c_name" & i, "└ Sub-container path", "", 100, 999, "")
	Next
end sub

sub OnInit()
	isChildMode = GetParameterBool("is_child_mode")
	isRealtimeRetarget = GetParameterBool("is_realtime_retarget")
	FindTargets()
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

	SendGuiParameterShow("pause_mode", CInt( GetParameterDouble("inertia") > 1 ))
	SendGuiParameterShow("pause_less", CInt( (GetParameterInt("pause_mode") == PAUSE_MODE_LESS OR GetParameterInt("pause_mode") == PAUSE_MODE_BOTH) AND GetParameterDouble("inertia") > 1 ))
	SendGuiParameterShow("pause_more", CInt( (GetParameterInt("pause_mode") == PAUSE_MODE_MORE OR GetParameterInt("pause_mode") == PAUSE_MODE_BOTH) AND GetParameterDouble("inertia") > 1 ))

	For i = 1 to quantity_of_container
		SendGuiParameterShow("c_name" & i, CInt(isChildMode))
	Next
	SendGuiParameterShow("is_realtime_retarget", CInt(isChildMode))
end sub

Function IsHaveSize(_c As Container) As Boolean
	_c.RecomputeMatrix()
	_c.GetTransformedBoundingBox(v1,v2)
	IsHaveSize = (v2.x > v1.x + thresholdZero OR v2.x < v1.x - thresholdZero) AND (v2.y > v1.y + thresholdZero OR v2.y < v1.y - thresholdZero)
End Function

Function FindSubContainerByPath(ByVal _c As Container, _path as String) As Container
	Dim _cCurrent As Container
	Dim _arrsSubContainerPath As Array[String]
	_path.split(".", _arrsSubContainerPath)
	for _pathIndex=0 to _arrsSubContainerPath.ubound
		_arrsSubContainerPath[_pathIndex].Trim()
		if _arrsSubContainerPath[_pathIndex] == "" then _arrsSubContainerPath.Erase(_pathIndex)
	next
	for _pathIndex=0 to _arrsSubContainerPath.ubound
		_cCurrent = _c.FindSubContainer(_arrsSubContainerPath[_pathIndex])
		if _cCurrent == null then
			FindSubContainerByPath = _c
			Exit Function
		end if
		_c = _cCurrent
	next
	FindSubContainerByPath = _c
End Function
sub FindTargets()
	arr_c.Clear()
	if isChildMode then
		For i = 1 to quantity_of_container
			c = FindSubContainerByPath(GetParameterContainer("c" & i), GetParameterString("c_name" & i))
			'if c == null then c = GetParameterContainer("c" & i)
			If c <> null Then arr_c.Push(c)
		Next
	else
		For i = 1 to quantity_of_container
			c = GetParameterContainer("c" & i)
			If c <> null Then arr_c.Push(c)
		Next
	end if
end sub


'''''''''''''''''''''''''''''''''''''''''''''''''''''''''

sub OnExecPerField()
	if isChildMode AND isRealtimeRetarget then
		FindTargets()
	end if

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

	new = this.WorldPosToLocalPos(new)

	'consider "to default"
	toDefault = 1-GetParameterDouble("toDefault")/100
	Select Case GetParameterInt("mode")
	Case MODE_X
		new.x = GetParameterDouble("defX") + (new.x - GetParameterDouble("defX"))*toDefault
	Case MODE_Y
		'TODO
	Case MODE_Z
		'TODO
	End Select



	'animate to new point
	Select Case GetParameterInt("mode")
	Case MODE_X
		Animate(this.position.x, new.x)
	Case MODE_Y
		Animate(this.position.y, new.y)
	Case MODE_Z
		Animate(this.position.z, new.z)
	End Select
end sub

Dim iPauseDownTicks As Integer
Dim prevNewValue As Double
Sub Animate(ByRef thisValue as Double, newValue As Double)
	if prevNewValue <> newValue then
		prevNewValue = newValue
		If GetParameterInt("pause_mode") == PAUSE_MODE_NONE Then
			iPauseDownTicks = 0
		Else
			If (GetParameterInt("pause_mode") == PAUSE_MODE_MORE OR GetParameterInt("pause_mode") == PAUSE_MODE_BOTH) AND newValue - thisValue > thresholdMove Then
				iPauseDownTicks = GetParameterInt("pause_more")
			End If
			If (GetParameterInt("pause_mode") == PAUSE_MODE_LESS OR GetParameterInt("pause_mode") == PAUSE_MODE_BOTH) AND thisValue - newValue > thresholdMove Then
				iPauseDownTicks = GetParameterInt("pause_less")
			End If
		End If
	end if

	If iPauseDownTicks > 0 then
		iPauseDownTicks -= 1
		Exit Sub
	End if

	If Abs(newValue - thisValue) > thresholdMove Then
		thisValue += (newValue - thisValue)/GetParameterDouble("inertia")
	End If
End Sub
