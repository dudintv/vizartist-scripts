RegisterPluginVersion(1,1,0)
Dim info As String = "Read and normalize a text file periodically. 
Developer: Dmitry Dudin, http://dudin.tv"

Dim filepath, result, varname, s As String
Dim arr_result As Array[String]
Dim interval As Double
Dim tick As Integer
Dim isLoaded As Boolean
Dim cTarget As Container

' INTERFACE
Dim arrOutputModes As Array[String]
arrOutputModes.Push("Text geom")
arrOutputModes.Push("Global var")
Dim OUTPUT_TO_GEOM As Integer = 0
Dim OUTPUT_TO_VAR As Integer = 1

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterString("filepath", "Text file fullpath", "", 100, 999, "")
	RegisterParameterBool("is_removing_empty_lines", "Remove empty lines", false)
	RegisterParameterDouble("interval", "Reading interval (sec)", 10.0, 1.0, 60.0)
	RegisterRadioButton("output_mode", "Output to:", 0, arrOutputModes)
	RegisterParameterContainer("target", "└ Text container (or this)")
	RegisterParameterString("varname", "└ Variable name", "", 100, 999, "")
	RegisterPushButton("read", "Read the file now", 1)
end sub

sub OnInit()
	filepath = GetParameterString("filepath")
	filepath.trim()
	interval = GetParameterDouble("interval")
	varname = GetParameterString("varname")
	cTarget = GetParameterContainer("target")
	if cTarget == null then cTarget = this
	varname.trim()
	tick = 0
	ReadFile()
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
	ReadFile()
	
	SendGuiParameterShow("target", CInt(GetParameterInt("output_mode") == OUTPUT_TO_GEOM))
	SendGuiParameterShow("varname", CInt(GetParameterInt("output_mode") == OUTPUT_TO_VAR))
end sub

sub OnExecPerField()
	tick += 1
	if tick > interval/System.CurrentRefreshRate then
		tick = 0
		ReadFile()
	end if
end sub

sub ReadFile()
	if filepath == "" then
		println("filepath is empty")
		exit sub
	end if
	
	isLoaded = System.LoadTextFile(filepath, result)
	if Not isLoaded then
		println("Can not load the file: " & filepath)
		exit sub
	end if
	
	result.split("\n", arr_result)
	for i=0 to arr_result.ubound
		s = arr_result[i]
		s.trim()
		if GetParameterBool("is_removing_empty_lines") AND s == "" then
			arr_result.erase(i)
			i -= 1
		end if
	next
	result.join(arr_result, "\n")
	
	Output(result)
end sub

sub Output(result as String)
	Select Case GetParameterInt("output_mode")
	Case OUTPUT_TO_GEOM
		cTarget.geometry.text = result
	Case OUTPUT_TO_VAR
		System.Map[varname] = result
	End Select
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 Then
		ReadFile()
		tick = 0
	end if
end sub
