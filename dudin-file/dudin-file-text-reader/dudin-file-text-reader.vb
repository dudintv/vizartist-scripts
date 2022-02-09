RegisterPluginVersion(1,0,0)
Dim info As String = "Read and normalize text file with interval"

Dim filepath, result, varname, s As String
Dim arr_result As Array[String]
Dim interval As Double
Dim tick As Integer
Dim isLoaded As Boolean

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterString("filepath", "Text file fullpath", "", 100, 999, "")
	RegisterParameterDouble("interval", "Reading interval (sec)", 10.0, 1.0, 60.0)
	RegisterParameterString("varname", "Variable name", "", 100, 999, "")
	RegisterPushButton("read", "Read the file", 1)
end sub

sub OnInit()
	filepath = GetParameterString("filepath")
	filepath.trim()
	interval = GetParameterDouble("interval")
	varname = GetParameterString("varname")
	varname.trim()
	tick = 0
	ReadFile()
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
	ReadFile()
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
		if s == "" then
			arr_result.erase(i)
			i -= 1
		end if
	next
	result.join(arr_result, "\n")
	
	System.Map[varname] = result
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 Then
		ReadFile()
		tick = 0
	end if
end sub
