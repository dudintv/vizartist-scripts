RegisterPluginVersion(1,3,0)
Dim infoText As String = "Debug global variables in all three SharedMemory hashes(Scene, System, VizCommunication). Author: Dmitry Dudin, http://dudin.tv"

Dim arr_varnames As Array[String]
Dim varnames As String

sub OnInitParameters()
	RegisterParameterBool("check_scene", "Check Scene.Map", true)
	RegisterParameterBool("check_system", "Check System.Map", true)
	RegisterParameterBool("check_vizcom", "Check VizCom.Map", true)
	RegisterParameterString("varnames", "Variable names (xxx, yyy)", "", 50, 999, "")
	RegisterParameterBool("print_console", "Print to console", true)
	RegisterParameterBool("print_thistext", "Print to this.text", false)
	RegisterPushButton("print_now", "Print now (if exist)", 1)
end sub

sub OnInit()
	varnames = GetParameterString("varnames")
	varnames.trim()
	varnames.split(",", arr_varnames)
	for i=0 to arr_varnames.ubound
		arr_varnames[i].trim()
	next
	'clear watching
	Scene.Map.UnregisterChangedCallback("")
	System.Map.UnregisterChangedCallback("")
	VizCommunication.Map.UnregisterChangedCallback("")
	'start watch
	for i=0 to arr_varnames.ubound
		if GetParameterBool("check_scene")  then Scene.Map.RegisterChangedCallback(arr_varnames[i])
		if GetParameterBool("check_system") then System.Map.RegisterChangedCallback(arr_varnames[i])
		if GetParameterBool("check_vizcom") then VizCommunication.Map.RegisterChangedCallback(arr_varnames[i])
	next

	PrintOutAllVariables()
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	PrintClear()
	PrepareOutput("CHANGED", map, mapKey)
	PrintOut()
end sub

sub OnSharedMemoryVariableDeleted(map As SharedMemory, mapKey As String)
	PrintClear()
	PrepareOutput("DELETED", map, mapKey)
	PrintOut()
end sub

Dim all As String
Sub PrintClear()
	all = ""
End Sub

Sub PrepareOutput(action As String, map As SharedMemory, mapKey As String)
	Dim hashName As String
	if map == Scene.Map then hashName = "Scene"
	if map == System.Map then hashName = "System"
	if map == VizCommunication.Map then hashName = "VizCom"

	if map.ContainsKey(mapKey) then
		all &= hashName & "[" & mapKey & "] is " & action & ": \n" & CStr(map[mapKey]) & "\n\n"
	else
		all &= hashName & "[" & mapKey & "] is NOT EXIST.\n\n"
	end if
End Sub

Sub PrintOut()
	if GetParameterBool("print_console") then
		println(all)
	end if
	if GetParameterBool("print_thistext") then
		this.Geometry.Text = all
	end if
End Sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		 PrintOutAllVariables()
	end if
end sub

Sub PrintOutAllVariables()
	PrintClear()
	for i=0 to arr_varnames.ubound
		if GetParameterBool("check_scene")  then PrepareOutput("", Scene.Map, arr_varnames[i])
		if GetParameterBool("check_system") then PrepareOutput("", System.Map, arr_varnames[i])
		if GetParameterBool("check_vizcom") then PrepareOutput("", VizCommunication.Map, arr_varnames[i])
	next
	PrintOut()
End Sub
