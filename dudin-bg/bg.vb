RegisterPluginVersion(1,9,1)

Dim info As String = "
Developer: Dmitry Dudin, dudin.tv
"

Dim cBg As Container = this
Dim cSource, cExactSource, child, cMinX, cMinY, cMinZ, cMaxChild As Container
Dim modeSize, mode, modeMinX, modeMinY, modeMinZ, fonChangeMode, maxSizeMode, pauseAxis As Integer
Dim x,y,z, xMulty, yMulty, zMulty, xPadding, yPadding, zPadding, xMin, yMin, zMin As Double
Dim pos As Vertex
Dim size, childSize, minSize As Vertex
Dim newSize, newPosition, v1, v2, vSource1, vSource2 As Vertex
Dim newPosX, newPosY As Double
Dim sizeTreshold, prevNewValue, newValue As Double
Dim animTreshold As Double = 0.001

Dim arrPauseAxis As Array[String]
arrPauseAxis.Push("X")
arrPauseAxis.Push("Y")

Dim arrAxis As Array[String]
arrAxis.Push("X")
arrAxis.Push("Y")
arrAxis.Push("Z")

Dim arrConsideringAxis As Array[String]
arrConsideringAxis.Push("X")
arrConsideringAxis.Push("Y")
arrConsideringAxis.Push("Z")
arrConsideringAxis.Push("XY")

Dim MODE_X  As Integer = 0
Dim MODE_Y  As Integer = 1
Dim MODE_Z  As Integer = 2
Dim MODE_XY As Integer = 3

Dim arrGetSizeFrom As Array[String]
arrGetSizeFrom.Push("Source container size")
arrGetSizeFrom.Push("Max of childs")
arrGetSizeFrom.Push("Child by index")
Dim arrMinMode As Array[String]
arrMinMode.Push("Off")
arrMinMode.Push("Number")
arrMinMode.Push("Container")
Dim arrFonShangeMode As Array[String]
arrFonShangeMode.Push("Scaling")
arrFonShangeMode.Push("Geometry")

Dim arrPauseMode As Array[String]
arrPauseMode.Push("none")
arrPauseMode.Push("><")
arrPauseMode.Push("<>")
arrPauseMode.Push(">< and <>")
Dim PAUSE_MODE_NONE As Integer = 0
Dim PAUSE_MODE_LESS As Integer = 1
Dim PAUSE_MODE_MORE As Integer = 2
Dim PAUSE_MODE_BOTH As Integer = 3

Dim iPauseDownTicks As Integer

sub OnInitParameters()
    RegisterInfoText(info)
	RegisterParameterContainer("source","Source container:")
	RegisterRadioButton("fonChangeMode", "└ How to change bg size", 0, arrFonShangeMode)
	RegisterRadioButton("modeSize", "Get size from: ", 0, arrGetSizeFrom)
	RegisterRadioButton("maxSizeMode", "└ max size by asix:", 0, arrAxis)
	RegisterParameterInt("numChild", "└ Child index (0=none)", 1, 0, 100)
	RegisterRadioButton("mode", "└ Axis to consider:", 0, arrConsideringAxis)
	RegisterParameterDouble("xMulty", "   └ Mult X", 1.0, 0.0, 10000000.0)
	RegisterParameterDouble("xPadding", "   └ Add X", 0.0, -100000.0, 10000000.0)
	RegisterParameterDouble("yMulty", "   └ Mult Y", 1.0, 0.0, 10000000.0)
	RegisterParameterDouble("yPadding", "   └ Add Y", 0.0, -100000.0, 10000000.0)
	RegisterParameterDouble("zMulty", "   └ Mult Z", 1.0, 0.0, 10000000.0)
	RegisterParameterDouble("zPadding", "   └ Add Z", 0.0, -100000.0, 10000000.0)
	
	RegisterRadioButton("xMinMode", "Min BG X-axis mode", 0, arrMinMode)
	RegisterParameterDouble("xMin", "└ Min X value", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("xMinContainer", "└ Min X-axis container")
	
	RegisterRadioButton("yMinMode", "Min BG Y-axis mode", 0, arrMinMode)
	RegisterParameterDouble("yMin", "└ Min Y value", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("yMinContainer", "└ Min Y-axis container")
	
	RegisterRadioButton("zMinMode", "Min BG Z-axis mode", 0, arrMinMode)
	RegisterParameterDouble("zMin", "└ Min Z value", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("zMinContainer", "└ Min Z-axis container")
	
	RegisterParameterBool("hideByZero", "Hide bg if size close to zero", TRUE)
	RegisterParameterDouble("treshold", "└ Zero-size of source container", 0.1, 0.0, 1000.0)
	
	
	RegisterParameterBool("positionX", "Autofollow by X axis", FALSE)
	RegisterParameterDouble("positionShiftX", "└ X shift", 0, -99999, 99999)
	RegisterParameterBool("positionY", "Autofollow by Y axis", FALSE)
	RegisterParameterDouble("positionShiftY", "└ Y shift", 0, -99999, 99999)
	
	
	RegisterParameterDouble("inertion", "Animation inertion", 1.0, 1.0, 100.0)
	RegisterRadioButton("pauseMode", "└ Pause direction", 0, arrPauseMode)
	RegisterRadioButton("pauseAxis", "   └ Considering size axis", 0, arrPauseAxis)
	RegisterParameterInt("pauseLess", "   └ Pause >< (frames)", 0, 0, 1000)
	RegisterParameterInt("pauseMore", "   └ Pause <> (frames)", 0, 0, 1000)
end sub

sub OnParameterChanged(parameterName As String)
	cSource = GetParameterContainer("source")
	maxSizeMode = GetParameterInt("maxSizeMode")
end sub
 
sub OnGuiStatus()
	modeSize = GetParameterInt("modeSize")
	If modeSize == 0 Then
		SendGuiParameterShow("numChild",HIDE)
		SendGuiParameterShow("maxSizeMode",HIDE)
	ElseIf modeSize == 1 Then
		SendGuiParameterShow("numChild",HIDE)
		SendGuiParameterShow("maxSizeMode",SHOW)
	ElseIf modeSize == 2 Then
		SendGuiParameterShow("numChild",SHOW)
		SendGuiParameterShow("maxSizeMode",HIDE)
	End If
	
	SendGuiParameterShow( "treshold", CInt(GetParameterBool("hideByZero")) )
	
	Dim showXMulti as Integer = HIDE
	Dim showYMulti as Integer = HIDE
	Dim showZMulti as Integer = HIDE
	Dim showXPadding as Integer = HIDE
	Dim showYPadding as Integer = HIDE
	Dim showZPadding as Integer = HIDE
	Dim showXMinMode as Integer = HIDE
	Dim showXMin as Integer = HIDE
	Dim showXMinC as Integer = HIDE
	Dim showYMinMode as Integer = HIDE
	Dim showYMin as Integer = HIDE
	Dim showYMinC as Integer = HIDE
	Dim showZMinMode as Integer = HIDE
	Dim showZMin as Integer = HIDE
	Dim showZMinC as Integer = HIDE
	Dim showPauseAxis as Integer = HIDE
	
	SendGuiParameterShow("pauseMode", CInt( GetParameterDouble("inertion") > 1 ))	
	mode = GetParameterInt("mode")
	Select Case mode
	Case MODE_X
		showXMulti = SHOW
		showXPadding = SHOW
		showXMinMode = SHOW
		showXMin = SHOW
		showXMinC = SHOW
	Case MODE_Y
		showYMulti = SHOW
		showYPadding = SHOW
		showYMinMode = SHOW
		showYMin = SHOW
		showYMinC = SHOW
	Case MODE_Z
		showZMulti = SHOW
		showZPadding = SHOW
		showZMinMode = SHOW
		showZMin = SHOW
		showZMinC = SHOW
	Case MODE_XY
		showXMulti = SHOW
		showYMulti = SHOW
		showXPadding = SHOW
		showYPadding = SHOW
		showXMinMode = SHOW
		showXMin = SHOW
		showXMinC = SHOW
		showYMinMode = SHOW
		showYMin = SHOW
		showYMinC = SHOW
		showPauseAxis = CInt( GetParameterDouble("inertion") > 1 AND GetParameterInt("pauseMode") > 0)
	End Select
	
	SendGuiParameterShow("xMulty", showXMulti)
	SendGuiParameterShow("yMulty", showYMulti)
	SendGuiParameterShow("zMulty", showZMulti)
	SendGuiParameterShow("xPadding", showXPadding)
	SendGuiParameterShow("yPadding", showYPadding)
	SendGuiParameterShow("zPadding", showZPadding)
	SendGuiParameterShow("xMinMode", showXMinMode)
	SendGuiParameterShow("xMin", showXMin)
	SendGuiParameterShow("xMinContainer", showXMinC)
	SendGuiParameterShow("yMinMode", showYMinMode)
	SendGuiParameterShow("yMin", showYMin)
	SendGuiParameterShow("yMinContainer", showYMinC)
	SendGuiParameterShow("zMinMode", showZMinMode)
	SendGuiParameterShow("zMin", showZMin)
	SendGuiParameterShow("zMinContainer", showZMinC)
	SendGuiParameterShow("pauseAxis", showPauseAxis)
	
	SendGuiParameterShow("pauseLess", CInt( (GetParameterInt("pauseMode") == PAUSE_MODE_LESS OR GetParameterInt("pauseMode") == PAUSE_MODE_BOTH) AND GetParameterDouble("inertion") > 1 )) 
	SendGuiParameterShow("pauseMore", CInt( (GetParameterInt("pauseMode") == PAUSE_MODE_MORE OR GetParameterInt("pauseMode") == PAUSE_MODE_BOTH) AND GetParameterDouble("inertion") > 1 )) 
	
	If mode == MODE_X OR mode == MODE_XY Then
		modeMinX = GetParameterInt("xMinMode")
		If modeMinX == 0 Then
			SendGuiParameterShow("xMin",HIDE)
			SendGuiParameterShow("xMinContainer",HIDE)
		ElseIf modeMinX == 1 Then
			SendGuiParameterShow("xMin",SHOW)
			SendGuiParameterShow("xMinContainer",HIDE)
		ElseIf modeMinX == 2 Then
			SendGuiParameterShow("xMin",HIDE)
			SendGuiParameterShow("xMinContainer",SHOW)
		End If
	End If
	
	If mode == MODE_Y OR mode == MODE_XY Then
		modeMinY = GetParameterInt("yMinMode")
		If modeMinY == 0 Then
			SendGuiParameterShow("yMin",HIDE)
			SendGuiParameterShow("yMinContainer",HIDE)
		ElseIf modeMinY == 1 Then
			SendGuiParameterShow("yMin",SHOW)
			SendGuiParameterShow("yMinContainer",HIDE)
		ElseIf modeMinY == 1 Then
			SendGuiParameterShow("yMin",HIDE)
			SendGuiParameterShow("yMinContainer",SHOW)
		End If
	End If
	
	If mode == MODE_Z Then
		modeMinZ = GetParameterInt("zMinMode")
		If modeMinZ == 0 Then
			SendGuiParameterShow("zMin",HIDE)
			SendGuiParameterShow("zMinContainer",HIDE)
		ElseIf modeMinZ == 1 Then
			SendGuiParameterShow("zMin",SHOW)
			SendGuiParameterShow("zMinContainer",HIDE)
		ElseIf modeMinZ == 2 Then
			SendGuiParameterShow("zMin",HIDE)
			SendGuiParameterShow("zMinContainer",SHOW)
		End If
	End If
	
	SendGuiParameterShow( "positionShiftX", CInt( GetParameterBool("positionX") ) )
	SendGuiParameterShow( "positionShiftY", CInt( GetParameterBool("positionY") ) )
end sub
 
sub OnExecPerField()
	If cSource == null Then exit sub
	
	mode = GetParameterInt("mode")
	sizeTreshold = GetParameterDouble("treshold")/100.0
	fonChangeMode = GetParameterInt("fonChangeMode")
	
	GetSourceSize()
	CalcMinSize()
	
	'-------------------------------------------------------------------------------------------------
	xMulty = GetParameterDouble("xMulty")
	yMulty = GetParameterDouble("yMulty")
	zMulty = GetParameterDouble("zMulty")
	xPadding = GetParameterDouble("xPadding")
	yPadding = GetParameterDouble("yPadding")
	zPadding = GetParameterDouble("zPadding")
	
	'processing X
	If mode == MODE_X OR mode == MODE_XY Then
		If size.X < sizeTreshold Then
			If GetParameterBool("hideByZero") Then
				cBg.Active = false
				'Exit Sub
			Else
				cBg.Active = true
				newSize.X = 0
			End If
		Else
			cBg.Active = true
			x = size.X/100.0 * xMulty + xPadding/10.0
			If x < xMin Then x = xMin
			newSize.X = x
		End If
	End If
	
	'processing Y
	If mode == MODE_Y OR mode == MODE_XY Then
		If size.Y < sizeTreshold Then
			If GetParameterBool("hideByZero") Then
				cBg.Active = false
				'Exit Sub
			Else
				cBg.Active = true
				newSize.Y = 0
			End If
		Else
			cBg.Active = true
			y = size.Y/100.0 * yMulty + yPadding/100.0
			If y < yMin Then y = yMin
			newSize.Y = y
		End If
	End If
	
	'processing Z
	If mode == MODE_Z Then
		If size.Z < sizeTreshold Then
			If GetParameterBool("hideByZero") Then
				cBg.Active = false
				Exit Sub
			Else
				cBg.Active = true
				newSize.Z = 0
			End If
		Else
			cBg.Active = true
			z = size.Z/100.0 * zMulty + zPadding/100.0
			If z < zMin Then z = zMin
			newSize.Z = z
		End If
	End If
	
	'-------------------------------------------------------------------------------------------------
	CalcPosition()
	ApplyTransformationWithInertion()
	cBg.RecomputeMatrix()
End Sub

Sub GetSourceSize()
	'mode logic ("Source container size", "Max child", "Child by index")
	modeSize = GetParameterInt("modeSize")
	If modeSize == 0 Then
		'mode: "Source container size"
		cExactSource = cSource
	ElseIf modeSize == 1 Then
		'mode: "Max child"
		child = cSource.FirstChildContainer
		cMaxChild = child
		size = GetLocalSize (child, cBg)
		child = child.NextContainer
		Do While child <> null
			child.RecomputeMatrix()
			childSize = GetLocalSize (child, cBg)
			If maxSizeMode == 0 AND childSize.X > size.X Then cMaxChild = child
			If maxSizeMode == 1 AND childSize.Y > size.Y Then cMaxChild = child
			If maxSizeMode == 2 AND childSize.Z > size.Z Then cMaxChild = child
			if cMaxChild == child then size = childSize
			child = child.NextContainer
		Loop
		cExactSource = cMaxChild
	ElseIf modeSize == 2 Then
		'mode: "Child by index"
		cExactSource = cSource.GetChildContainerByIndex(GetParameterInt("numChild") - 1)
		cExactSource.RecomputeMatrix()
	End If
	
	size = GetLocalSize (cExactSource, cBg)
End Sub

Sub CalcMinSize()
	cMinX = GetParameterContainer("xMinContainer")
	cMinY = GetParameterContainer("yMinContainer")
	cMinZ = GetParameterContainer("zMinContainer")
	
	If modeMinX == 1 Then
		'Min size from value
		xMin = GetParameterDouble("xMin")
	ElseIf modeMinX == 2 AND cMinX <> null Then
		'Min size from container
		minSize = GetLocalSize (cMinX, cBg)
		If minSize.X > sizeTreshold AND minSize.Y > sizeTreshold Then
			xMin = minSize.X/100.0
		Else
			xMin = 0
		End If
	Else
		xMin = 0
	End If
	
	If modeMinY == 1 Then
		'Min size from value
		yMin = GetParameterDouble("yMin")
	ElseIF modeMinY == 2 AND cMinY <> null Then
		'Min size from container
		minSize = GetLocalSize (cMinY, cBg)
		yMin = minSize.Y/100.0
		If minSize.X > sizeTreshold AND minSize.Y > sizeTreshold Then
			yMin = minSize.Y/100.0
		Else
			yMin = 0
		End If
	Else
		yMin = 0
	End If
End Sub

Dim cachedScalingX, cachedScalingY, cachedWidth, cachedHeight As Double
Sub PreCalcFinishGabarits()
	cachedScalingX = cBg.scaling.x
	cachedScalingY = cBg.scaling.y
	cachedWidth = cBg.geometry.GetParameterDouble("width")
	cachedHeight = cBg.geometry.GetParameterDouble("height")

	if fonChangeMode == 0 then
		cBg.scaling.x = newSize.x
		cBg.scaling.y = newSize.y
	elseif fonChangeMode == 1 then
		cBg.geometry.SetParameterDouble(  "width", 100.0*newSize.x )
		cBg.geometry.SetParameterDouble(  "height", 100.0*newSize.y )
	end if
	cBg.RecomputeMatrix()
	
	' calculate the target bounding box like it's already finished
	cBg.GetTransformedBoundingBox(v1, v2)
	
	cBg.scaling.x = cachedScalingX
	cBg.scaling.y = cachedScalingY
	cBg.geometry.SetParameterDouble(  "width", cachedWidth )
	cBg.geometry.SetParameterDouble(  "height", cachedHeight )
	cBg.RecomputeMatrix()
End Sub

Sub CalcPosition()
	if GetParameterBool("positionX") OR GetParameterBool("positionY") then
		cExactSource.GetTransformedBoundingBox(vSource1, vSource2)
		PreCalcFinishGabarits()
		
		if GetParameterBool("positionX") then
			vSource1 = cSource.LocalPosToWorldPos(vSource1)
			vSource1 = cExactSource.parentContainer.WorldPosToLocalPos(vSource1)
			newPosition.x = vSource1.x - (v1.x-cBg.position.x) + GetParameterDouble("positionShiftX")
		end if
		
		if GetParameterBool("positionY") then
			vSource2 = cSource.LocalPosToWorldPos(vSource2)
			vSource2 = cExactSource.parentContainer.WorldPosToLocalPos(vSource2)
			newPosition.y = vSource2.y - (v2.y-cBg.position.y) + GetParameterDouble("positionShiftY")
		end if
	end if
End Sub

Function HasPause() as Boolean
	pauseAxis = GetParameterInt("pauseAxis")
	Dim currentValue As Double
	if mode == MODE_X OR (mode == MODE_XY AND pauseAxis == MODE_X) Then
		newValue = newSize.x
		if fonChangeMode == 0 then
			currentValue = cBg.scaling.x
		elseif fonChangeMode == 1 then
			newValue *= 100
			currentValue = cBg.geometry.GetParameterDouble("width")
		end if
	elseif mode == MODE_Y OR (mode == MODE_XY AND pauseAxis == MODE_Y) Then
		newValue = newSize.y
		if fonChangeMode == 0 then
			currentValue = cBg.scaling.y
		elseif fonChangeMode == 1 then
			newValue *= 100
			currentValue = cBg.geometry.GetParameterDouble("height")
		end if
	elseif mode == MODE_Z Then
		newValue = newSize.z
		if fonChangeMode == 0 then
			currentValue = cBg.scaling.z
		elseif fonChangeMode == 1 then
			newValue *= 100
			currentValue = cBg.geometry.GetParameterDouble("height")
		end if
	end if
	
	if prevNewValue <> newValue then
		If GetParameterInt("pauseMode") == PAUSE_MODE_NONE Then
			iPauseDownTicks = 0
		Else
			If (GetParameterInt("pauseMode") == PAUSE_MODE_MORE OR GetParameterInt("pauseMode") == PAUSE_MODE_BOTH) AND newValue > currentValue + animTreshold Then
				iPauseDownTicks = GetParameterInt("pauseMore")
			End If
			If (GetParameterInt("pauseMode") == PAUSE_MODE_LESS OR GetParameterInt("pauseMode") == PAUSE_MODE_BOTH) AND newValue < currentValue - animTreshold Then
				iPauseDownTicks = GetParameterInt("pauseLess")
			End If
		End If
		
		prevNewValue = newValue
	end if
	
	If iPauseDownTicks > 0 then
		iPauseDownTicks -= 1
		HasPause = true
	Else
		HasPause = false
	End if
End Function

Sub ApplyTransformationWithInertion()
	If HasPause() then exit sub
	
	if mode == MODE_X OR mode == MODE_XY Then
		if fonChangeMode == 0 then
			cBg.scaling.x = GetAnimatedValue(cBg.scaling.x, newSize.x)
		elseif fonChangeMode == 1 then
			cBg.geometry.SetParameterDouble(  "width", GetAnimatedValue(cBg.geometry.GetParameterDouble("width"), 100.0*newSize.x)  )
		end if
	end if
	if mode == MODE_Y OR mode == MODE_XY Then
		if fonChangeMode == 0 then
			cBg.scaling.y = GetAnimatedValue(cBg.scaling.y, newSize.y)
		elseif fonChangeMode == 1 then
			cBg.geometry.SetParameterDouble(  "height", GetAnimatedValue(cBg.geometry.GetParameterDouble("height"), 100.0*newSize.y)  )
		end if
	end if
	if mode == MODE_Z Then
		if fonChangeMode == 0 then
			cBg.scaling.z = GetAnimatedValue(cBg.scaling.z, newSize.z)
		elseif fonChangeMode == 1 then
			cBg.geometry.SetParameterDouble(  "height", GetAnimatedValue(cBg.geometry.GetParameterDouble("height"), 100.0*newSize.z)  )
		end if
	end if
	
	if GetParameterBool("positionX") then
		cBg.position.x = GetAnimatedValue(cBg.position.x, newPosition.x)
	end if
	
	if GetParameterBool("positionY") then
		cBg.position.y = GetAnimatedValue(cBg.position.y, newPosition.y)
	end if
End Sub

Function GetAnimatedValue(currentValue as Double, newValue as Double) As Double
	if newValue < (currentValue - animTreshold) OR newValue > (currentValue + animTreshold) Then
		GetAnimatedValue = currentValue + (newValue - currentValue)/GetParameterDouble("inertion")
	else
		GetAnimatedValue = newValue
	end if
End Function

Function GetLocalSize(_c_gabarit as Container, _c as Container) As Vertex
	_c_gabarit.GetTransformedBoundingBox(v1,v2)
	v1 = _c.WorldPosToLocalPos(v1)
	v2 = _c.WorldPosToLocalPos(v2)
	GetLocalSize = CVertex(v2.x-v1.x,v2.y-v1.y,v2.z-v1.z)
End Function
