RegisterPluginVersion(1,0,1)
Dim info As String = "Parlamant Colorizer
Developer: Dmitry Dudin
http://dudin.tv"

Dim sInput, prevInput, console As String
Dim arrLines, arrFields As Array[String]
Dim cParlament, cColors As Container
Dim pParlament, pColorize As PluginInstance
Dim coloredSeatsNumber, totalSeatsNumber As Integer

Structure ColorPoint
	value As Integer
	color As Color
End Structure
Dim arrColorPoints As Array[ColorPoint]

Dim arrInputTypes As Array[String]
arrInputTypes.Push("Container")
arrInputTypes.Push("SHM var")
Dim INPUT_TYPE_CONTAINER = 0
Dim INPUT_TYPE_SHM = 1

sub OnInitParameters()
	RegisterParameterContainer("root", "Parlament container (or this)")
	RegisterRadioButton("input_type", "Input type", INPUT_TYPE_SHM, arrInputTypes)
	RegisterParameterContainer("input_container", " └ Container with input text")
	RegisterParameterString("input_shm_name", " └ SHM variable", "", 999, 999, "")
	RegisterParameterInt("color_index", "Color-index column", 1, 0, 999)
	RegisterParameterInt("value_index", "Value column", 1, 0, 999)
	RegisterParameterContainer("colors_set_container", "Colors set")
	RegisterPushButton("colorize", "Colorize parlament", 1)
	RegisterParameterText("console", "", 999, 999)
end sub
sub OnGuiStatus()
	SendGuiParameterShow("input_container", CInt(GetParameterInt("input_type") == INPUT_TYPE_CONTAINER))
	SendGuiParameterShow("input_shm_name", CInt(GetParameterInt("input_type") == INPUT_TYPE_SHM))
end sub

sub OnInit()
	System.Map.UnregisterChangedCallback("")
	GetParameterContainer("input_container").Geometry.UnregisterChangedCallback()
	if GetParameterInt("input_type") == INPUT_TYPE_CONTAINER then
		GetParameterContainer("input_container").Geometry.RegisterTextChangedCallback()
	elseif GetParameterInt("input_type") == INPUT_TYPE_SHM then
		System.Map.RegisterChangedCallback(GetParameterString("input_shm_name"))
	end if

	cParlament = GetParameterContainer("root")
	if cParlament == null then cParlament = this

	pParlament = cParlament.GetFunctionPluginInstance("Parlament")
	pColorize = cParlament.GetFunctionPluginInstance("Colorize")
	cColors = GetParameterContainer("colors_set_container")

	Colorize()
end sub
sub OnParameterChanged(parameterName As String)
	if parameterName <> "console" then
		OnInit()
	end if
end sub

sub OnGeometryChanged(geom As Geometry)
	sInput = GetParameterContainer("input_container").Geometry.Text
	if sInput <> prevInput then
		Colorize()
		prevInput = sInput
	end if
end sub
sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	if mapKey == GetParameterString("input_shm_name") then
		Colorize()
	end if
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		Colorize()
	end if
end sub

Sub Colorize()
	console = ""
	GetData()
	ParseInput()
	Dim defaultColor As Color = cColors.FirstChildContainer.Material.Color
	totalSeatsNumber = pParlament.GetParameterInt("n_of_el")
	coloredSeatsNumber = 0

	for i=0 to 10
		pColorize.SetParameterInt("size"&i&"_a", 0)
		pColorize.SetParameterColor("color"&i&"_a", defaultColor)
	next

	Dim index As Integer
	for party=1 to arrColorPoints.size
		Dim thisColorSeats = CInt(arrColorPoints[party-1].value)
		coloredSeatsNumber += thisColorSeats
		pColorize.SetParameterInt("size" & party & "_b", thisColorSeats)
		pColorize.SetParameterColor("color" & party & "_b", arrColorPoints[party-1].color)
	next


	'fill up the rest
	Dim restSeats = totalSeatsNumber - coloredSeatsNumber
	pColorize.SetParameterInt("size" & arrColorPoints.size + 1 & "_b", restSeats)
	pColorize.SetParameterColor("color" & arrColorPoints.size + 1 & "_b", defaultColor)

	if restSeats < 0 then
		Dim warningMessage = "PARLAMENT COLORIZE WARNING: the sum of colored seats ("&coloredSeatsNumber&") is more than " & totalSeatsNumber
		println(3, warningMessage)
		console &= warningMessage & "\n\n"
	elseif restSeats > 0 then
		Dim warningMessage = "PARLAMENT COLORIZE WARNING: the sum of colored seats ("&coloredSeatsNumber&") is less than " & totalSeatsNumber
		println(3, warningMessage)
		console &= warningMessage & "\n\n"
	end if

	for index=arrColorPoints.size + 2 to 10
		pColorize.SetParameterInt("size"&index&"_b", 0)
		pColorize.SetParameterColor("color"&index&"_b", defaultColor)
	next

	pColorize.PushButton("rebuild")
	pColorize.SetChanged()
	Report()
End Sub

Sub GetData()
	if GetParameterInt("input_type") == INPUT_TYPE_CONTAINER then
		if GetParameterContainer("input_container") <> null then
			sInput = GetParameterContainer("input_container").Geometry.Text
		else
			Dim errorMessage = "PARLAMENT COLORIZE ERROR: the input container is not specified"
			println(4, errorMessage)
			console &= errorMessage & "\n\n"
		end if
	elseif GetParameterInt("input_type") == INPUT_TYPE_SHM then
		if System.Map.ContainsKey(GetParameterString("input_shm_name")) then
			sInput = System.Map[GetParameterString("input_shm_name")]
		else
			sInput = ""
			Dim errorMessage = "PARLAMENT COLORIZE ERROR: The variable '" & GetParameterString("input_shm_name") & "' doesn't exist in System.Map"
			println(4, errorMessage)
			console &= errorMessage & "\n\n"
		end if
	end if
	SendGuiRefresh()
End Sub

Sub ParseInput()
	arrColorPoints.Clear()
	sInput.Trim()
	sinput.split("\n", arrLines)
	for i=0 to arrLines.ubound
		arrLines[i].split("|", arrFields)
		Dim point As ColorPoint
		point.value = CInt(arrFields[GetParameterInt("value_index")-1])
		Dim colorIndex As Integer = CInt(arrFields[GetParameterInt("color_index")-1])
		Dim cColor = cColors.GetChildContainerByIndex(colorIndex)
		point.color = cColor.Material.Color
		arrColorPoints.Push(point)
	next
End Sub

Sub Report()
	if console == "" then
		console = "OK\n\n"
	end if

	if sInput <> "" then
		console &= "INPUT DATA:\n" & sInput
	else
		console &= "NO INPUT DATA :("
	end if

	this.ScriptPluginInstance.SetParameterString("console",console)
	SendGuiRefresh()
End Sub
