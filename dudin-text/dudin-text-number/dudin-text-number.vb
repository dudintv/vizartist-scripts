RegisterPluginVersion(1,1,0)

Dim cSource, cSign, cInteger, cComma, cFraction, cSuffix As Container
Dim xInteger, xComma, xFraction, xSuffix As Double
Dim sInput As String
Dim value As Double
Dim arrsNumber As Array[String]

Dim sourceModeNames As Array[String]
sourceModeNames.Push("text input")
sourceModeNames.Push("number input")
sourceModeNames.Push("container")
sourceModeNames.Push("SHM system variable")
Dim SOURCE_MODE_TEXT = 0
Dim SOURCE_MODE_NUMBER = 1
Dim SOURCE_MODE_CONTAINER = 2
Dim SOURCE_MODE_SHM = 3

Dim delimiterChars As Array[String]
delimiterChars.push(", (comma)")
delimiterChars.push(". (period)")
Dim DELIMITER_COMMA = 0
Dim DELIMITER_PERIOD = 1

sub OnInitParameters()
	RegisterRadioButton("source_mode", "Source mode", SOURCE_MODE_TEXT, sourceModeNames)
	RegisterParameterString("source_text", " └ Text input", "", 999, 999, "")
	RegisterParameterDouble("source_number", " └ Number input", 0, -999999, 999999)
	RegisterParameterContainer("source_container", " └ Container with text")
	RegisterParameterString("source_shm_variable", " └ SHM variable name", "", 999, 999, "")

	RegisterRadioButton("delimiter", "Delimiter", DELIMITER_COMMA, delimiterChars)
	RegisterParameterInt("precision", "Precision", 2, 0, 10)
	RegisterParameterString("suffix", "Suffix", "", 999, 999, "")

	RegisterParameterBool("hide_on_zero", "Hide when zero", false)
	RegisterParameterBool("show_positive_sign", "Show positive sign (+123)", false)
	RegisterParameterBool("auto_remove_non_numbers", "Auto-remove non numbers", false)

	RegisterParameterDouble("kerning_sign", "Kerning after sign", 0, -999999, 999999)
	RegisterParameterDouble("kerning_integer", "Kerning after integer", 0, -999999, 999999)
	RegisterParameterDouble("kerning_comma", "Kerning after comma", 0, -999999, 999999)
	RegisterParameterDouble("kerning_fraction", "Kerning after fraction", 0, -999999, 999999)

	RegisterParameterDouble("inertia", "Inertia (1 = disabled)", 5, 1, 999999)
end sub

sub OnGuiStatus()
	SendGuiParameterShow("source_text", CInt(GetParameterInt("source_mode") == SOURCE_MODE_TEXT))
	SendGuiParameterShow("source_number", CInt(GetParameterInt("source_mode") == SOURCE_MODE_NUMBER))
	SendGuiParameterShow("source_container", CInt(GetParameterInt("source_mode") == SOURCE_MODE_CONTAINER))
	SendGuiParameterShow("source_shm_variable", CInt(GetParameterInt("source_mode") == SOURCE_MODE_SHM))
end sub

sub OnInit()
	Dim cInput = GetParameterContainer("source_container")
	Scene.Map.UnregisterChangedCallback("")
	cInput.geometry.UnregisterChangedCallback()

	if GetParameterInt("source_mode") == SOURCE_MODE_CONTAINER then
		cInput.geometry.RegisterTextChangedCallback()
	elseif GetParameterInt("source_mode") == SOURCE_MODE_SHM then
		Scene.Map.RegisterChangedCallback(GetParameterString("source_shm_variable"))
	end if

	cSign = this.findSubContainer("sign")
	cInteger = this.findSubContainer("integer")
	cComma = this.findSubContainer("comma")
	cFraction = this.findSubContainer("fraction")
	cSuffix = this.findSubContainer("suffix")
end sub

sub OnParameterChanged(parameterName As String)
	Process()
end sub

sub OnGeometryChanged(geom As Geometry)
	Process()
end sub

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	Process()
end sub

'''''''''''''''''''''''''''''''''''''''''''''''

Sub Process()
	'prepare sInput and value
	select case GetParameterInt("source_mode")
	case SOURCE_MODE_TEXT
		value = PrepareDoubleValue(GetParameterString("source_text"))
	case SOURCE_MODE_NUMBER
		value = GetParameterDouble("source_number")
	case SOURCE_MODE_CONTAINER
		cSource = GetParameterContainer("source_container")
		if cSource <> null then
			value = PrepareDoubleValue(cSource.geometry.text)
		else
			value = PrepareDoubleValue("")
		end if
	case SOURCE_MODE_SHM
		value = PrepareDoubleValue(CStr(System.map[GetParameterString("source_shm_variable")]))
	end select

	'println(4, "value = " & value)
	sInput = doubleToString(value, GetParameterInt("precision"))
	sInput.split(".", arrsNumber)

	'sign
	cSign.active = value < 0 OR GetParameterBool("show_positive_sign")
	if value > 0 then
		cSign.geometry.text = "+"
	elseif value < 0 then
		cSign.geometry.text = "-"
		arrsNumber[0].Erase(0,1)
	elseif NOT GetParameterBool("hide_on_zero") then
		cSign.geometry.text = ""
	end if

	'integer
	cInteger.geometry.text = arrsNumber[0]

	'fraction
	if GetParameterInt("delimiter") == DELIMITER_COMMA then
		cComma.geometry.text = ","
	else
		cComma.geometry.text = "."
	end if
	if arrsNumber.size > 1 then
		cComma.active = true
		cFraction.active = true
		cFraction.geometry.text = arrsNumber[1]
	else
		cComma.active = false
		cFraction.active = false
		cFraction.geometry.text = ""
	end if

	'suffix
	cSuffix.geometry.text = GetParameterString("suffix")

	'common
	this.active = NOT ( GetParameterBool("hide_on_zero") AND value == 0 )

	Align()
End Sub

'''''''''''''''''''''''''''''''''''''''''''''''

Function PrepareDoubleValue(s as String) as Double
	s.trim()
	s.substitute(",", ".", true)
	if GetParameterBool("auto_remove_non_numbers") then
		s.Substitute("[^1-9.]", "", true) 'remove non-number-and-dot symbols
	end if
	PrepareDoubleValue = CDbl(s)
End Function

Sub Align()
	cSign.RecomputeMatrix()
	cInteger.RecomputeMatrix()
	cComma.RecomputeMatrix()
	cFraction.RecomputeMatrix()
	cSuffix.RecomputeMatrix()
	xInteger = AutoFollow(0, cSign, GetParameterDouble("kerning_sign"))
	xComma = Autofollow(xInteger, cInteger, GetParameterDouble("kerning_integer"))
	xFraction = Autofollow(xComma, cComma, GetParameterDouble("kerning_comma"))
	xSuffix = Autofollow(xFraction, cFraction, GetParameterDouble("kerning_fraction"))

	if GetParameterDouble("inertia") <= 1 then
		cInteger.position.x = xInteger
		cComma.position.x = xComma
		cFraction.position.x = xFraction
		cSuffix.position.x = xSuffix
	end if
End Sub

Dim v1, v2 As Vertex
Function AutoFollow(xTarget As Double, cTarget As Container, kerning As Double) As Double
	cTarget.RecomputeMatrix()
	cTarget.GetBoundingBox(v1, v2)
	AutoFollow = xTarget + v2.x*cTarget.scaling.x + kerning/10.0
End Function

'''''''''''''''''''''''''''''''''''''''''''''''''''

sub OnExecPerField()
	if GetParameterDouble("inertia") > 1 then
		MoveElement(cInteger, xInteger)
		MoveElement(cComma, xComma)
		MoveElement(cFraction, xFraction)
		MoveElement(cSuffix, xSuffix)
	end if
end sub

Sub MoveElement(c As Container, xTarget As Double)
	c.position.x = c.position.x + (xTarget - c.position.x)/GetParameterDouble("inertia")
End Sub
