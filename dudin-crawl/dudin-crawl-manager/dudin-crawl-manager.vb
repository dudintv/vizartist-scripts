RegisterPluginVersion(1,0,1)
Dim info As String = "Crawl manager
Developer: Dmitry Dudin
http://dudin.tv"

Dim sourceMode, outputMode As Integer
Dim arrCrawlItems, arrSourteText As Array[String]
Dim sourceText, delimiter As String
Dim cInput, cOutput As Container

Dim sourceModes, outputModes As Array[String]
sourceModes.Push("text")
sourceModes.Push("container")
sourceModes.Push("SHM")
outputModes.Push("container")
outputModes.Push("SHM")
Dim SOURCE_MODE_TEXT = 0
Dim SOURCE_MODE_CONTAINER = 1
Dim SOURCE_MODE_SHM = 2
Dim OUTPUT_MODE_CONTAINER = 0
Dim OUTPUT_MODE_SHM = 1

sub OnInitParameters()
	RegisterParameterString("delimiter", "Source delimiter", "\\n", 100, 999, "")
	RegisterRadioButton("source_mode", "Source mode", 0, sourceModes)
	RegisterParameterText("source_text", " └ Source text", 100, 100)
	RegisterParameterContainer("source_container", " └ Source text container")
	RegisterParameterString("source_shm", " └ Source SHM variable", "", 100, 999, "")

	RegisterRadioButton("output_mode", "Output mode", 0, outputModes)
	RegisterParameterContainer("output_container", " └ Output text container (or this)")
	RegisterParameterString("output_shm", " └ Output SHM variable", "", 100, 999, "")

	RegisterParameterString("separator_text", "Separator text", "", 100, 999, "")
	RegisterParameterBool("separator_from_start", "Insert the separator in front?", false)

	RegisterPushButton("process", "Process", 1)
end sub

sub OnInit()
	sourceMode = GetParameterInt("source_mode")
	outputMode = GetParameterInt("output_mode")
	delimiter = GetParameterString("delimiter")
	if delimiter == "\\n" then delimiter = "\n"

	if sourceMode == SOURCE_MODE_CONTAINER then
		System.Map.UnregisterChangedCallback(GetParameterString("source_shm"))

		cInput = GetParameterContainer("source_container")
		if cInput <> null then cInput.geometry.RegisterTextChangedCallback()
	elseif sourceMode == SOURCE_MODE_SHM then
		if cInput <> null then cInput.geometry.UnregisterChangedCallback()

		System.Map.RegisterChangedCallback(GetParameterString("source_shm"))
	end if

	cOutput = GetParameterContainer("output_container")
	if cOutput == null then cOutput = this
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
	SendGuiParameterShow("source_text", CInt(sourceMode == SOURCE_MODE_TEXT))
	SendGuiParameterShow("source_container", CInt(sourceMode == SOURCE_MODE_CONTAINER))
	SendGuiParameterShow("source_shm", CInt(sourceMode == SOURCE_MODE_SHM))

	SendGuiParameterShow("output_container", CInt(outputMode == OUTPUT_MODE_CONTAINER))
	SendGuiParameterShow("output_shm", CInt(outputMode == OUTPUT_MODE_SHM))
	Process()
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId = 1 Then
		Process()
	end if
end sub

sub OnGeometryChanged(geom As Geometry)
	Process()
end sub

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	Process()
end sub

Sub Process()

	if sourceMode == SOURCE_MODE_TEXT then
		sourceText = GetParameterString("source_text")
	elseif sourceMode == SOURCE_MODE_CONTAINER then
		sourceText = cInput.geometry.text
	elseif sourceMode == SOURCE_MODE_SHM then
		sourceText = CStr(System.Map[GetParameterString("source_shm")])
	end if
	sourceText.Trim()
	sourceText.Split(delimiter, arrSourteText)

	arrCrawlItems.Clear()
	if sourceText == "" then exit sub

	for i=0 to arrSourteText.ubound
		if GetParameterBool("separator_from_start") then
			arrCrawlItems.Push(GetParameterString("separator_text"))
		end if

		arrCrawlItems.Push(arrSourteText[i])

		if NOT GetParameterBool("separator_from_start") then
			arrCrawlItems.Push(GetParameterString("separator_text"))
		end if
	next

	Dim result = ""
	result.join(arrCrawlItems, "\n")

	if outputMode == OUTPUT_MODE_CONTAINER then
		cOutput.geometry.text = result
	elseif outputMode == OUTPUT_MODE_SHM then
		System.Map[GetParameterString("output_shm")] = result
	end if
End Sub

