RegisterPluginVersion(1,4,4)
Dim info As String = "Read and normalize a text file periodically.
Developer: Dmitry Dudin, http://dudin.tv"

Structure FrontMatterField
	name As String
	value As String
End Structure
Dim field As FrontMatterField
Dim frontMatterFields As Array[FrontMatterField]

Dim fullFilePath, originalFullText, trimmedFullText, dataText, frontMatterText, shmDataVarName, shmFrontMatterVarName, frontMatterPrefix, s, console As String
Dim arrOriginalLines, arrFrontMatterLines, arrDataLines As Array[String]
Dim interval As Double
Dim tick, frontMatterLineIndex As Integer
Dim isLoaded, hasFrontMatter As Boolean
Dim cDataTarget, cFrontMatterDataTarget As Container

' INTERFACE
Dim arrDataOutputModes As Array[String]
arrDataOutputModes.Push("This")
arrDataOutputModes.Push("Other")
arrDataOutputModes.Push("SHM")
Dim OUTPUT_DATA_TO_THIS_GEOM As Integer = 0
Dim OUTPUT_DATA_TO_OTHER_GEOM As Integer = 1
Dim OUTPUT_DATA_TO_SYSTEM_VAR As Integer = 2

Dim arrFrontMatterModes As Array[String]
arrFrontMatterModes.Push("Nothing (ignore)")
arrFrontMatterModes.Push("This text")
arrFrontMatterModes.Push("Other text")
arrFrontMatterModes.Push("Containers (by fields names)")
arrFrontMatterModes.Push("One SHM variable")
arrFrontMatterModes.Push("Distribute to SHM variables")
Dim OUTPUT_FM_TO_VOID As Integer = 0
Dim OUTPUT_FM_TO_THIS As Integer = 1
Dim OUTPUT_FM_TO_OTHER As Integer = 2
Dim OUTPUT_FM_TO_CONTAINERS As Integer = 3
Dim OUTPUT_FM_TO_SHM As Integer = 4
Dim OUTPUT_FM_TO_SHMS As Integer = 5

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterString("full_file_path", "Text file fullpath", "", 100, 999, "")
	RegisterParameterBool("split_filepath", "Split to path and finename?", false)
	RegisterParameterString("file_path", " └ Path to the file, prefix [*required]", "", 100, 999, "")
	RegisterParameterString("file_name", " └ Filename [*required]", "", 100, 999, "")
	RegisterParameterString("file_ext", " └ File extention, suffix", "", 100, 999, "")

	RegisterParameterBool("is_removing_empty_lines", "Remove empty lines", false)
	RegisterParameterBool("ignore_first_data_line", "Remove first line in data (can be a header)", false)

	RegisterParameterBool("has_front_matter", "Enable Front Matter (---)", false)
	RegisterRadioButton("front_matter_data_output_mode", " └ Output to:", OUTPUT_FM_TO_SHM, arrFrontMatterModes)
	RegisterParameterString("front_matter_prefix", "     └ Prefix for output names", "", 100, 999, "")
	RegisterParameterString("front_matter_shm_var_name", "     └ SHM system variable name", "", 100, 999, "")
	RegisterParameterContainer("front_matter_target", "     └ Container with text")

	RegisterParameterDouble("interval", "Reading interval (sec)", 5.0, 1.0, 60.0)
	RegisterRadioButton("data_output_mode", "Output data to:", OUTPUT_DATA_TO_THIS_GEOM, arrDataOutputModes)
	RegisterParameterContainer("target", " └ Container with text (or this)")
	RegisterParameterString("shm_data_var_name", " └ SHM system variable name", "", 100, 999, "")
	RegisterPushButton("read", "Read the file now", 1)
	RegisterParameterText("console", "", 999, 999)
end sub

sub OnGuiStatus()
	SendGuiParameterShow("full_file_path", CInt(NOT GetParameterBool("split_filepath")))
	SendGuiParameterShow("file_path", CInt(GetParameterBool("split_filepath")))
	SendGuiParameterShow("file_name", CInt(GetParameterBool("split_filepath")))
	SendGuiParameterShow("file_ext", CInt(GetParameterBool("split_filepath")))
end sub

sub OnInit()
	fullFilePath = GetParameterString("full_file_path")
	fullFilePath.trim()
	shmDataVarName = GetParameterString("shm_data_var_name")
	shmDataVarName.trim()
	shmFrontMatterVarName = GetParameterString("front_matter_shm_var_name")
	shmFrontMatterVarName.trim()
	frontMatterPrefix = GetParameterString("front_matter_prefix")
	frontMatterPrefix.trim()

	interval = 0.2 'set a very small delay for the very first try
	hasFrontMatter = GetParameterBool("has_front_matter")

	if GetParameterInt("front_matter_data_output_mode") == OUTPUT_FM_TO_OTHER then
		cFrontMatterDataTarget = GetParameterContainer("front_matter_target")
	end if
	if GetParameterInt("data_output_mode") == OUTPUT_DATA_TO_OTHER_GEOM then
		cDataTarget = GetParameterContainer("target")
	end if

	tick = 0
	ReadFile()
end sub
sub OnParameterChanged(parameterName As String)
	if parameterName == "console" then exit sub

	OnInit()
	tick = 0
	ReadFile()

	SendGuiParameterShow("target", CInt(GetParameterInt("data_output_mode") == OUTPUT_DATA_TO_OTHER_GEOM))
	SendGuiParameterShow("shm_data_var_name", CInt(GetParameterInt("data_output_mode") == OUTPUT_DATA_TO_SYSTEM_VAR))

	Dim hasFrontMatterPrefix = (GetParameterInt("front_matter_data_output_mode") == OUTPUT_FM_TO_CONTAINERS) OR (GetParameterInt("front_matter_data_output_mode") == OUTPUT_FM_TO_SHMS)
	SendGuiParameterShow("front_matter_data_output_mode", CInt(  hasFrontMatter  ))
	SendGuiParameterShow("front_matter_prefix", CInt(  hasFrontMatter AND hasFrontMatterPrefix  ))
	SendGuiParameterShow("front_matter_shm_var_name", CInt(  hasFrontMatter AND GetParameterInt("front_matter_data_output_mode") == OUTPUT_FM_TO_SHM  ))
	SendGuiParameterShow("front_matter_target", CInt(  hasFrontMatter AND GetParameterInt("front_matter_data_output_mode") == OUTPUT_FM_TO_OTHER  ))
end sub

sub OnExecPerField()
	tick += 1
	if tick > interval/System.CurrentRefreshRate then
		ReadFile()
		interval = GetParameterDouble("interval")
		tick = 0
	end if
end sub

sub ReadFile()
	println("ReadFile tick = " & tick)
	console = ""

	if GetParameterBool("split_filepath") then
		if GetParameterString("file_path") == "" then
			println(4, "FILE READER ERROR: file path is empty")
			exit sub
		end if
		if GetParameterString("file_name") == "" then
			println(4, "FILE READER ERROR: file name is empty")
			exit sub
		end if
		Dim path = GetParameterString("file_path")
		path.trim()
		if path.right(1) <> "\\" then path &= "\\"
		fullFilePath =path & GetParameterString("file_name") & GetParameterString("file_ext")
	else
		if fullFilePath == "" then
			println("FILE READER ERROR: the filepath is empty")
			exit sub
		end if
	end if

	isLoaded = System.LoadTextFile(fullFilePath, originalFullText)
	if Not isLoaded then
		Dim errorMessage = "Can not load the file: " & fullFilePath
		println(4, "FILE READER ERROR: " & errorMessage)
		console &= errorMessage & "\n\n"
		Report()
		exit sub
	end if

	TrimOriginalTextLines() ' to prepare arrOriginalLines

	if GetParameterBool("has_front_matter") then
		FindFrontMatterLine() ' to prepare frontMatterLineIndex
		if frontMatterLineIndex >= 0 then
			arrFrontMatterLines = GetSubLines(arrOriginalLines, 0, frontMatterLineIndex - 1)
			arrDataLines = GetSubLines(arrOriginalLines, frontMatterLineIndex + 1 + CInt(GetParameterBool("ignore_first_data_line")), -1)
			frontMatterText.join(arrFrontMatterLines, "\n")
			dataText.join(arrDataLines, "\n")
		else
			frontMatterText = ""
			dataText = ""
		end if
	else
		frontMatterText = ""
		if GetParameterBool("ignore_first_data_line") then arrOriginalLines.Erase(0)
		trimmedFullText.join(arrOriginalLines, "\n")
		dataText = trimmedFullText
	end if

	OutputFrontMatter()
	OutputData()
	Report()
end sub

sub TrimOriginalTextLines()
	originalFullText.split("\n", arrOriginalLines)
	for i=0 to arrOriginalLines.ubound
		s = arrOriginalLines[i]
		s.trim()
		if GetParameterBool("is_removing_empty_lines") AND s == "" then
			arrOriginalLines.erase(i)
			i -= 1
		end if
	next
End sub

sub FindFrontMatterLine()
	frontMatterLineIndex = -1
	for i=0 to arrOriginalLines.ubound
		if arrOriginalLines[i].Match("^-{3,}$") then
			' if the line contains ONLY "-" characters, min is three on them
			frontMatterLineIndex = i
			exit sub
		end if
	next
End sub

Function GetSubLines(arrLines As Array[String], startIndex As Integer, endIndex As Integer) As Array[String]
	Dim arrResultLines As Array[String]
	if endIndex < 0 then endIndex = arrLines.size + endIndex ' where "-1" gives the last index
	for i=startIndex to endIndex
		arrResultLines.Push(arrLines[i])
	Next
	GetSubLines = arrResultLines
End Function

Sub PrepareFrontMatterFields()
	frontMatterFields.Clear()
	for i=0 to arrFrontMatterLines.ubound
		Dim theFieldNameSeparatorIndex = arrFrontMatterLines[i].Find(":")
		
		if theFieldNameSeparatorIndex > 0 then
			field.name = arrFrontMatterLines[i].GetSubstring(0, Min(theFieldNameSeparatorIndex, arrFrontMatterLines[i].length))
		end if
			
		if theFieldNameSeparatorIndex > 0 AND theFieldNameSeparatorIndex < arrFrontMatterLines[i].length-1 then
			field.value = arrFrontMatterLines[i].GetSubstring(Min(theFieldNameSeparatorIndex + 1, arrFrontMatterLines[i].length-1), arrFrontMatterLines[i].length)
		else
			println("theFieldNameSeparatorIndex = " & theFieldNameSeparatorIndex)
			println("arrFrontMatterLines[i].length = " & arrFrontMatterLines[i].length-1)
			field.value = ""
		end if
		
		if theFieldNameSeparatorIndex > 0 then
			frontMatterFields.Push(field)
		end if
	next
End Sub

sub OutputFrontMatter()
	Select Case GetParameterInt("front_matter_data_output_mode")
	Case OUTPUT_FM_TO_VOID
		'ignore the frontmatter data
		exit sub
	Case OUTPUT_FM_TO_THIS
		if GetParameterInt("data_output_mode") <> OUTPUT_DATA_TO_THIS_GEOM then
			this.geometry.text = frontMatterText
		end if
	Case OUTPUT_FM_TO_OTHER
		cFrontMatterDataTarget.geometry.text = frontMatterText
	Case OUTPUT_FM_TO_CONTAINERS
		PrepareFrontMatterFields()
		for i=0 to frontMatterFields.ubound
			Dim cFrontMatterFiledTarget = Scene.FindContainer(frontMatterPrefix & frontMatterFields[i].name)
			if cFrontMatterFiledTarget <> null then
				cFrontMatterFiledTarget.Geometry.Text = frontMatterFields[i].value
			end if
		next
	Case OUTPUT_FM_TO_SHM
		if shmFrontMatterVarName <> "" then
			System.Map[shmFrontMatterVarName] = frontMatterText
		end if
	Case OUTPUT_FM_TO_SHMS
		PrepareFrontMatterFields()
		for i=0 to frontMatterFields.ubound
			if frontMatterFields[i].name <> "" then
				System.Map[frontMatterPrefix & frontMatterFields[i].name] = frontMatterFields[i].value
			end if
		next
	End Select
end sub

sub OutputData()
	Select Case GetParameterInt("data_output_mode")
	Case OUTPUT_DATA_TO_THIS_GEOM
		if  GetParameterInt("front_matter_data_output_mode") == OUTPUT_FM_TO_THIS then
			this.geometry.text = frontMatterText & "\n---\n" & dataText
		else
			this.geometry.text = dataText
		end if
	Case OUTPUT_DATA_TO_OTHER_GEOM
		cDataTarget.geometry.text = dataText
	Case OUTPUT_DATA_TO_SYSTEM_VAR
		System.Map[shmDataVarName] = dataText
	End Select
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 Then
		ReadFile()
	end if
end sub

Sub Report()
	if console == "" then
		console = "OK\n\nFILE PATH:\n" & fullFilePath & "\n\n"
	end if

	if frontMatterText <> "" then console &= "FRONTMATTER DATA:\n" & frontMatterText & "\n\n"
	if dataText <> "" then
		console &= "FILE DATA:\n" & dataText
	else
		console &= "NO DATA :("
	end if

	this.ScriptPluginInstance.SetParameterString("console",console)
End Sub
