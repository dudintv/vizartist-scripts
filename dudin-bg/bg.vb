RegisterPluginVersion(1,14,0)

Dim info As String = "
Developer: Dmitry Dudin, dudin.tv
"

Dim cBg As Container = this
Dim cSource, cExactSource, child, cMinX, cMinY, cMinZ, cMaxChild, cByIndex As Container
Dim modeSize, mode, modeMinX, modeMinY, modeMinZ, fonChangeMode, maxSizeMode, pauseAxis, alignX, alignY, sourceMode As Integer
Dim x,y,z, xMulty, yMulty, zMulty, xPadding, yPadding, zPadding, xMin, yMin, zMin As Double
Dim pos As Vertex
Dim size, childSize, minSize As Vertex
Dim newSize, newPosition, v1, v2, vBg1, vBg2, vSource1, vSource2 As Vertex
Dim newPosX, newPosY As Double
Dim sizeTreshold, prevNewValue, newValue As Double
Dim animTreshold As Double = 0.001

Dim prevSourceId, curSourceId As Integer
Dim changingSourceDelay As Integer = 1
Dim changingSourceTick As Integer = changingSourceDelay

Dim arrSource As Array[String]
arrSource.Push("FIRST")
arrSource.Push("PREV")
arrSource.Push("NEXT")
arrSource.Push("PATH")
arrSource.Push("OTHER")
Dim SOURCE_FIRST As Integer = 0
Dim SOURCE_PREV As Integer = 1
Dim SOURCE_NEXT As Integer = 2
Dim SOURCE_PATH As Integer = 3
Dim SOURCE_OTHER As Integer = 4

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
arrGetSizeFrom.Push("Child by index + subpath")
Dim arrMinMode As Array[String]
arrMinMode.Push("Off")
arrMinMode.Push("Number")
arrMinMode.Push("Container")
Dim arrFonShangeMode As Array[String]
arrFonShangeMode.Push("Scaling")
arrFonShangeMode.Push("Geometry")

Dim arrAlignX, arrAlignY As Array[String]
arrAlignX.Push("Left")
arrAlignX.Push("Center")
arrAlignX.Push("Right")
arrAlignY.Push("Top")
arrAlignY.Push("Center")
arrAlignY.Push("Bottom")
Dim ALIGN_X_LEFT As Integer = 0
Dim ALIGN_X_CENTER As Integer = 1
Dim ALIGN_X_RIGHT As Integer = 2
Dim ALIGN_Y_TOP As Integer = 0
Dim ALIGN_Y_CENTER As Integer = 1
Dim ALIGN_Y_BOTTOM As Integer = 2

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
	RegisterRadioButton("fonChangeMode","How to change bg size", 0, arrFonShangeMode)
	RegisterRadioButton("source",             "Source", 2, arrSource)
	RegisterParameterString("sourcePath",     "└ Source path (\\\"sibling/sub/name\\\")", "", 100, 999, "")
	RegisterParameterContainer("sourceOther", "└ Source container:")
	RegisterRadioButton("modeSize",     "Get size from: ", 0, arrGetSizeFrom)
	RegisterRadioButton("maxSizeMode",  "└ max size by asix:", 0, arrAxis)
	RegisterParameterInt("numChild",    "└ Child index (0=none)", 1, 0, 100)
	RegisterParameterString("numChildSubPath", "   └ Sub path (\\\"sub/name\\\")", "", 100, 999, "")
	RegisterRadioButton("mode",         "└ Axis to consider:", 0, arrConsideringAxis)
	RegisterParameterDouble("xMulty",   "   └ Mult X", 1.0, 0.0, 10000000.0)
	RegisterParameterDouble("xPadding", "   └ Add X", 0.0, -100000.0, 10000000.0)
	RegisterParameterDouble("yMulty",   "   └ Mult Y", 1.0, 0.0, 10000000.0)
	RegisterParameterDouble("yPadding", "   └ Add Y", 0.0, -100000.0, 10000000.0)
	RegisterParameterDouble("zMulty",   "   └ Mult Z", 1.0, 0.0, 10000000.0)
	RegisterParameterDouble("zPadding", "   └ Add Z", 0.0, -100000.0, 10000000.0)
	
	RegisterRadioButton("xMinMode",             "Min X size", 0, arrMinMode)
	RegisterParameterDouble("xMin",             "└ Min X value", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("xMinContainer", "└ Min X-axis container")
	RegisterParameterDouble("xMinAdd",          "└ Min X add", 0.0, -100000.0, 10000000.0)
	
	RegisterRadioButton("yMinMode",             "Min Y size", 0, arrMinMode)
	RegisterParameterDouble("yMin",             "└ Min Y value", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("yMinContainer", "└ Min Y-axis container")
	RegisterParameterDouble("yMinAdd",          "└ Min Y add", 0.0, -100000.0, 10000000.0)
	
	RegisterRadioButton("zMinMode",             "Min Z size", 0, arrMinMode)
	RegisterParameterDouble("zMin",             "└ Min Z value", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("zMinContainer", "└ Min Z-axis container")
	RegisterParameterDouble("zMinAdd",          "└ Min Z add", 0.0, -100000.0, 10000000.0)
	
	RegisterParameterBool("hideByZero", "Hide bg if size close to zero", TRUE)
	RegisterParameterDouble("treshold", "└ Zero-size of source container", 0.1, 0.0, 1000.0)
	
	
	RegisterParameterBool("positionX",        "Autofollow by X axis", FALSE)
	RegisterRadioButton("positionAlignX",     "└ X Align", ALIGN_X_CENTER, arrAlignX)
	RegisterParameterDouble("positionShiftX", "└ X shift", 0, -99999, 99999)
	RegisterParameterBool("positionY",        "Autofollow by Y axis", FALSE)
	RegisterRadioButton("positionAlignY",     "└ Y Align", ALIGN_Y_CENTER, arrAlignY)
	RegisterParameterDouble("positionShiftY", "└ Y shift", 0, -99999, 99999)
	
	RegisterParameterBool("hasInertion", "Enable inertia", true)
	RegisterParameterDouble("inertion", "Animation inertia", 1.0, 1.0, 100.0)
	RegisterRadioButton("pauseMode",    "└ Pause direction", 0, arrPauseMode)
	RegisterRadioButton("pauseAxis",    "   └ Considering size axis", 0, arrPauseAxis)
	RegisterParameterInt("pauseLess",   "   └ Pause >< (frames)", 0, 0, 1000)
	RegisterParameterInt("pauseMore",   "   └ Pause <> (frames)", 0, 0, 1000)
	
	RegisterParameterBool("hasOffSize", "Enable off transition", FALSE)
	RegisterParameterDouble("offWidth", "└ Off width", 0, -99999, 99999)
	RegisterParameterDouble("offHeight", "└ Off height", 0, -99999, 99999)
	RegisterParameterDouble("offTransition", "└ Off transition (0 - 100)", 0, 0.0, 100.0)
end sub

sub OnParameterChanged(parameterName As String)
	maxSizeMode = GetParameterInt("maxSizeMode")
end sub
 
sub OnGuiStatus()
	sourceMode =  GetParameterInt("source")
	if sourceMode == SOURCE_OTHER then
		SendGuiParameterShow("sourcePath", HIDE)
		SendGuiParameterShow("sourceOther", SHOW)
	elseif sourceMode == SOURCE_PATH then
		SendGuiParameterShow("sourcePath", SHOW)
		SendGuiParameterShow("sourceOther", HIDE)
	else
		SendGuiParameterShow("sourcePath", HIDE)
		SendGuiParameterShow("sourceOther", HIDE)
	end if

	modeSize = GetParameterInt("modeSize")
	If modeSize == 0 Then
		SendGuiParameterShow("numChild",HIDE)
		SendGuiParameterShow("maxSizeMode",HIDE)
	ElseIf modeSize == 1 Then
		SendGuiParameterShow("numChild",HIDE)
		SendGuiParameterShow("maxSizeMode",SHOW)
	ElseIf modeSize == 2 OR modeSize == 3 Then
		SendGuiParameterShow("numChild",SHOW)
		SendGuiParameterShow("maxSizeMode",HIDE)
	End If
	SendGuiParameterShow("numChildSubPath", CInt(modeSize == 3))
	
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
	
	SendGuiParameterShow("inertion", CInt( GetParameterBool("hasInertion") ))
	SendGuiParameterShow("pauseMode", CInt( GetParameterBool("hasInertion") ))
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
		showPauseAxis = CInt( GetParameterBool("hasInertion") AND GetParameterInt("pauseMode") > 0)
	End Select
	
	SendGuiParameterShow("xMulty", showXMulti)
	SendGuiParameterShow("yMulty", showYMulti)
	SendGuiParameterShow("zMulty", showZMulti)
	SendGuiParameterShow("xPadding", showXPadding)
	SendGuiParameterShow("yPadding", showYPadding)
	SendGuiParameterShow("zPadding", showZPadding)
	SendGuiParameterShow("xMinMode", showXMinMode)
	SendGuiParameterShow("yMinMode", showYMinMode)
	SendGuiParameterShow("zMinMode", showZMinMode)
	SendGuiParameterShow("pauseAxis", showPauseAxis)
	SendGuiParameterShow("pauseLess", CInt( (GetParameterInt("pauseMode") == PAUSE_MODE_LESS OR GetParameterInt("pauseMode") == PAUSE_MODE_BOTH) AND GetParameterBool("hasInertion") )) 
	SendGuiParameterShow("pauseMore", CInt( (GetParameterInt("pauseMode") == PAUSE_MODE_MORE OR GetParameterInt("pauseMode") == PAUSE_MODE_BOTH) AND GetParameterBool("hasInertion") )) 
	
	If mode == MODE_X OR mode == MODE_XY Then
		modeMinX = GetParameterInt("xMinMode")
		If modeMinX == 0 Then
			showXMin = HIDE
			showXMinC = HIDE
		ElseIf modeMinX == 1 Then
			showXMin = SHOW
			showXMinC = HIDE
		ElseIf modeMinX == 2 Then
			showXMin = HIDE
			showXMinC = SHOW
		End If
	End If
	
	If mode == MODE_Y OR mode == MODE_XY Then
		modeMinY = GetParameterInt("yMinMode")
		If modeMinY == 0 Then
			showYMin = HIDE
			showYMinC = HIDE
		ElseIf modeMinY == 1 Then
			showYMin = SHOW
			showYMinC = HIDE
		ElseIf modeMinY == 2 Then
			showYMin = HIDE
			showYMinC = SHOW
		End If
	End If
	
	If mode == MODE_Z Then
		modeMinZ = GetParameterInt("zMinMode")
		If modeMinZ == 0 Then
			showZMin = HIDE
			showZMinC = HIDE
		ElseIf modeMinZ == 1 Then
			showZMin = SHOW
			showZMinC = HIDE
		ElseIf modeMinZ == 2 Then
			showZMin = HIDE
			showZMinC = SHOW
		End If
	End If
	
	SendGuiParameterShow("xMin",          showXMin)
	SendGuiParameterShow("xMinContainer", showXMinC)
	SendGuiParameterShow("xMinAdd",       showXMinC)
	
	SendGuiParameterShow("yMin",          showYMin)
	SendGuiParameterShow("yMinContainer", showYMinC)
	SendGuiParameterShow("yMinAdd",       showYMinC)
	
	SendGuiParameterShow("zMin",          showZMin)
	SendGuiParameterShow("zMinContainer", showZMinC)
	SendGuiParameterShow("zMinAdd",       showZMinC)
	
	SendGuiParameterShow("positionShiftX", CInt(GetParameterBool("positionX")))
	SendGuiParameterShow("positionShiftY", CInt(GetParameterBool("positionY")))
	
	SendGuiParameterShow("positionAlignX", CInt(GetParameterBool("positionX")))
	SendGuiParameterShow("positionAlignY", CInt(GetParameterBool("positionY")))
	
	SendGuiParameterShow("offWidth",  CInt(  GetParameterBool("hasOffSize") AND (mode == MODE_X OR mode == MODE_XY)  ))
	SendGuiParameterShow("offHeight", CInt(  GetParameterBool("hasOffSize") AND (mode == MODE_Y OR mode == MODE_XY)  ))
	SendGuiParameterShow("offTransition", CInt(  GetParameterBool("hasOffSize")  ))
end sub

Sub GetSourceContainer()
	sourceMode =  GetParameterInt("source")
	if sourceMode == SOURCE_FIRST then
		cSource = this.ParentContainer.FirstChildContainer
	elseif sourceMode == SOURCE_PREV then
		cSource = this.PreviousContainer 
	elseif sourceMode == SOURCE_NEXT then
		cSource = this.NextContainer
	elseif sourceMode == SOURCE_PATH then
		if changingSourceTick > 0 then
			changingSourceTick -= 1
		else
			Dim newSource = GetContainerByPath(this, GetParameterString("sourcePath"))
			curSourceId = newSource.VizId
			if prevSourceId <> curSourceId then
				changingSourceTick = changingSourceDelay
				prevSourceId = curSourceId
			else
				cSource = newSource
			end if
		end if
	elseif sourceMode == SOURCE_OTHER then
		cSource = GetParameterContainer("sourceOther")
	else
		cSource = null
	end if
End Sub

Function GetContainerByPath(cRoot As Container, path As String) As Container
	Dim arrPathSteps As Array[String]
	path.substitute("\\", "/", true)
	path.Split("/", arrPathSteps)
	
	Dim cResult As Container = cRoot
	Dim numberStep As Integer
	for i=0 to arrPathSteps.ubound
		arrPathSteps[i].Trim()
		numberStep = Cint(arrPathSteps[i])
		' we have to ignore the case "numberStep == 0" because it means "no numbers"
		if numberStep > 0 then
			cResult = cResult.GetChildContainerByIndex(numberStep-1)
		elseif numberStep < 0 then
			cResult = cResult.GetChildContainerByIndex(cResult.ChildContainerCount - numberStep)
		elseif arrPathSteps[i] == "" then
			cResult = scene.RootContainer
		elseif arrPathSteps[i] == "." OR arrPathSteps[i] == "this" then
			cResult = cResult ' to support pointing direct children
		elseif arrPathSteps[i] == ".." then
			if i == 0 then
				cResult = cResult.ParentContainer.ParentContainer
			else
				cResult = cResult.ParentContainer
			end if
		elseif i == 0 then
			cResult = cResult.ParentContainer.FindSubContainer(arrPathSteps[i])
		else
			cResult = cResult.FindSubContainer(arrPathSteps[i])
		end if
	next
	
	GetContainerByPath = cResult
End Function
 
sub OnExecPerField()
	mode = GetParameterInt("mode")
	sizeTreshold = GetParameterDouble("treshold")/100.0
	fonChangeMode = GetParameterInt("fonChangeMode")
	
	GetSourceContainer()
	If cSource == null Then
		cBg.Active = false
		exit sub
	end if
	
	GetSourceSize()
	If cExactSource == null Then
		cBg.Active = false
		exit sub
	end if
	
	CalcMinSize()
	
	'-------------------------------------------------------------------------------------------------
	xMulty = GetParameterDouble("xMulty")
	yMulty = GetParameterDouble("yMulty")
	zMulty = GetParameterDouble("zMulty")
	xPadding = GetParameterDouble("xPadding")
	yPadding = GetParameterDouble("yPadding")
	zPadding = GetParameterDouble("zPadding")
	
	'set newSize as the current in order to support various alignments
	if fonChangeMode == 0 then
		newSize.x = cBg.scaling.x
		newSize.y = cBg.scaling.y
	elseif fonChangeMode == 1 then
		newSize.x = cBg.geometry.GetParameterDouble("width")/100.0
		newSize.y = cBg.geometry.GetParameterDouble("height")/100.0
	end if
	
	'processing X
	If mode == MODE_X OR mode == MODE_XY Then
		If size.X < sizeTreshold Then
			If GetParameterBool("hideByZero") Then
				cBg.Active = false
				'Exit Sub
			Else
				cBg.Active = true
				newSize.x = 0
			End If
		Else
			cBg.Active = true
			x = size.X/100.0 * xMulty + xPadding/10.0
			If x < xMin Then x = xMin
			newSize.x = GetSizeConsideringOffSize("offWidth", x)
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
				newSize.y = 0
			End If
		Else
			cBg.Active = true
			y = size.Y/100.0 * yMulty + yPadding/100.0
			If y < yMin Then y = yMin
			newSize.y = GetSizeConsideringOffSize("offHeight", y)
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
				newSize.z = 0
			End If
		Else
			cBg.Active = true
			z = size.Z/100.0 * zMulty + zPadding/100.0
			If z < zMin Then z = zMin
			newSize.z = z
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
	ElseIf modeSize == 3 Then
		'mode: "Child by index + sub path"
		cByIndex = cSource.GetChildContainerByIndex(GetParameterInt("numChild") - 1)
		
		if GetParameterString("numChildSubPath") == "" then
			cExactSource = cByIndex
		else
			cExactSource = GetContainerByPath(cByIndex, "./" & GetParameterString("numChildSubPath"))
		end if
	End If
	cExactSource.RecomputeMatrix()
	size = GetLocalSize(cExactSource, cBg)
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
		minSize = GetLocalSize(cMinX, cBg)
		If minSize.X > sizeTreshold AND minSize.Y > sizeTreshold Then
			xMin = minSize.X/100.0 + GetParameterDouble("xMinAdd")/100.0
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
		minSize = GetLocalSize(cMinY, cBg)
		yMin = minSize.Y/100.0
		If minSize.X > sizeTreshold AND minSize.Y > sizeTreshold Then
			yMin = minSize.Y/100.0  + GetParameterDouble("yMinAdd")/100.0
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
	cBg.GetTransformedBoundingBox(vBg1, vBg2)
	
	if fonChangeMode == 0 then
		cBg.scaling.x = cachedScalingX
		cBg.scaling.y = cachedScalingY
	elseif fonChangeMode == 1 then
		cBg.geometry.SetParameterDouble(  "width", cachedWidth )
		cBg.geometry.SetParameterDouble(  "height", cachedHeight )
	end if
	cBg.RecomputeMatrix()
End Sub

Sub CalcPosition()
	alignX = GetParameterInt("positionAlignX")
	alignY = GetParameterInt("positionAlignY")
	
	if GetParameterBool("positionX") OR GetParameterBool("positionY") then
		'cExactSource.GetTransformedBoundingBox(vSource1, vSource2)
		Dim arrExactSourceVertexes = GetLocalGabaritVertexes(cExactSource, cBg)
		vSource1 = arrExactSourceVertexes[0]
		vSource2 = arrExactSourceVertexes[1]
		
		PreCalcFinishGabarits()
		
		if GetParameterBool("positionX") then
			if alignX == ALIGN_X_LEFT then
				newPosition.x = vSource1.x + (cBg.position.x - vBg1.x)
			elseif alignX == ALIGN_X_CENTER then
				newPosition.x = (vSource1.x + vSource2.x)/2.0 + (cBg.position.x - (vBg1.x + vBg2.x)/2.0)
			elseif alignX == ALIGN_X_RIGHT then
				newPosition.x = vSource2.x + (cBg.position.x - vBg2.x)
			end if
			newPosition.x += GetParameterDouble("positionShiftX")
		end if
		
		if GetParameterBool("positionY") then
			if alignY == ALIGN_Y_TOP then
				newPosition.y = vSource2.y + (cBg.position.y - vBg2.y)
			elseif alignY == ALIGN_Y_CENTER then
				newPosition.y = (vSource1.y + vSource2.y)/2.0 + (cBg.position.y - (vBg1.y + vBg2.y)/2.0)
			elseif alignY == ALIGN_Y_BOTTOM then
				newPosition.y = vSource1.y + (cBg.position.y - vBg1.y)
			end if
			newPosition.y += GetParameterDouble("positionShiftY")
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
		If GetParameterInt("pauseMode") == PAUSE_MODE_NONE OR NOT GetParameterBool("hasInertion") Then
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
	if NOT GetParameterBool("hasInertion") then
		GetAnimatedValue = newValue
		exit function
	end if

	if newValue < (currentValue - animTreshold) OR newValue > (currentValue + animTreshold) Then
		GetAnimatedValue = currentValue + (newValue - currentValue)/GetParameterDouble("inertion")
	else
		GetAnimatedValue = newValue
	end if
End Function

Function GetLocalGabaritVertexes(_c_gabarit as Container, _c as Container) As Array[Vertex]
	_c_gabarit.GetTransformedBoundingBox(v1,v2)
	v1 = _c.WorldPosToLocalPos(v1)
	v2 = _c.WorldPosToLocalPos(v2)
	Dim _arrResult As Array[Vertex]
	_arrResult.Push(v1)
	_arrResult.Push(v2)
	GetLocalGabaritVertexes = _arrResult
End Function

Function GetLocalSize(_c_gabarit as Container, _c as Container) As Vertex
	_c_gabarit.GetTransformedBoundingBox(v1,v2)
	v1 = _c.WorldPosToLocalPos(v1)
	v2 = _c.WorldPosToLocalPos(v2)
	GetLocalSize = CVertex(v2.x-v1.x,v2.y-v1.y,v2.z-v1.z)
End Function

Function GetSizeConsideringOffSize(offSizeName As String, onSize As Double) As Double
	if NOT GetParameterBool("hasOffSize") OR (offSizeName == "offHeight" AND mode == MODE_X) OR (offSizeName == "offWidth" AND mode == MODE_Y) then
		GetSizeConsideringOffSize = onSize
		Exit Function
	end if

   GetSizeConsideringOffSize = onSize + (GetParameterDouble(offSizeName)/100.0 - onSize)*GetParameterDouble("offTransition")/100.0
End function
