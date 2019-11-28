RegisterPluginVersion(1,2,0)
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
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	PrintClear()
	Output("CHANGE", map, mapKey)
	PrintOut()
end sub

sub OnSharedMemoryVariableDeleted(map As SharedMemory, mapKey As String)
	PrintClear()
	Output("DELETE", map, mapKey)
	PrintOut()
end sub

Dim all As String
Sub PrintClear()
	all = ""
End Sub

Sub Output(action As String, map As SharedMemory, mapKey As String)
	Dim hash As String
	if map == Scene.Map then hash = "Scene"
	if map == System.Map then hash = "System"
	if map == VizCommunication.Map then hash = "VizCom"
	all = all & action & " " & hash & "[" & mapKey & "] = " & CStr(map[mapKey]) & "\n"
End Sub

Sub PrintOut()
	if GetParameterBool("print_console") then
		println(all)
	end if
	if GetParameterBool("print_console") then
		this.Geometry.Text = all
	end if
End Sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		PrintClear()
		for i=0 to arr_varnames.ubound
			if GetParameterBool("check_scene")  AND Scene.Map.ContainsKey(arr_varnames[i])  then Output("NOW", Scene.Map, arr_varnames[i])
			if GetParameterBool("check_system") AND System.Map.ContainsKey(arr_varnames[i]) then Output("NOW", System.Map, arr_varnames[i])
			if GetParameterBool("check_vizcom") AND VizCommunication.Map.ContainsKey(arr_varnames[i]) then Output("NOW", VizCommunication.Map, arr_varnames[i])
		next
		PrintOut()
	end if
end sub
