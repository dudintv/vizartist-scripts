RegisterPluginVersion(1,2,0)
Dim info As String = "
Theme Materials Switcher
Developer: Dmitry Dudin
"

Dim arrcThemes, arrcTags, arrcRawTargets As Array[Container]
Dim arrcSources As Array[Array[Container]]
Dim arrcTargets As Array[Array[Container]]
Dim arrsThemes As Array[String]
Dim arrsTags As Array[String]
Dim s, console, sName, sThemeType As String
Dim cSourceRoot, cTargetRoot As Container
Dim m As Material
Dim iPrevOmo = GetFunctionPluginInstance("Omo").GetParameterInt("vis_con")
Dim iCurrentOmo As Integer

Dim SWITCH_MODE_NUMBER = 0
Dim SWITCH_MODE_OMO = 1
Dim SWITCH_MODE_SHM = 2
Dim arrSwitchModes As Array[String]
arrSwitchModes.Push("Number")
arrSwitchModes.Push("From Omo")
arrSwitchModes.Push("SHM Variable")

Structure Tagged
	sTag As String
	arrcSources As Array[Container]
	arrcTargets As Array[Container]
End Structure
Dim arrTagged As Array[Tagged]

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterPushButton("init", "Init", 1)
	RegisterParameterContainer("sources_root", "Sources root [or this]")
	RegisterParameterContainer("targets_root", "Targets root [or whole scene]")
	RegisterRadioButton("switch_mode", "Switch mode", 1, arrSwitchModes)
	RegisterParameterInt("current_theme_index", " └ Current Theme", 0, 0, 999)
	RegisterParameterString("shm_variable", " └ SHM Variable Name", "", 40, 999, "")
	RegisterParameterBool("has_omo_sync", " └ Sync to Omo", true)
	RegisterParameterText("console", "", 600, 240)
end sub

sub OnInit()
	cSourceRoot = GetParameterContainer("sources_root")
	if cSourceRoot == null then cSourceRoot = this
	
	console = ""
	GetSource()
	GetTargets()
	this.ScriptPluginInstance.SetParameterString("console", console)
end sub
sub OnParameterChanged(parameterName As String)
	SendGuiParameterShow("current_theme_index", CInt(GetParameterInt("switch_mode") == SWITCH_MODE_NUMBER))
	SendGuiParameterShow("shm_variable", CInt(GetParameterInt("switch_mode") == SWITCH_MODE_SHM))
	SendGuiParameterShow("has_omo_sync", CInt(GetParameterInt("switch_mode") <> SWITCH_MODE_OMO))

	if parameterName <> "console" then
		OnInit()
		UpdateTheme()
	end if
end sub
sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		OnInit()
		UpdateTheme()
	end if
end sub


sub OnExecPerField()
	if GetParameterInt("switch_mode") == SWITCH_MODE_OMO then
		iCurrentOmo = cSourceRoot.GetFunctionPluginInstance("Omo").GetParameterInt("vis_con")
		if iPrevOmo <> iCurrentOmo then 
			ApplyTheme(iCurrentOmo)
			iPrevOmo = iCurrentOmo
		end if
	end if
end sub

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	if GetParameterInt("switch_mode") == SWITCH_MODE_SHM then
		if mapKey == GetParameterString("shm_variable") then
			ApplyTheme(CInt(System.Map[GetParameterString("shm_variable")]))
		end if
	end if
end sub


''''''''''''''''''''''''''''''

function GetSource() as Boolean
	arrsThemes.Clear()
	arrsTags.Clear()
	
	' GET THEMES
	GetFirstLevelChilds(cSourceRoot, arrcThemes)
	if arrcThemes.size <= 0 then
		GetSource = false
		exit function
	end if
	for i=0 to arrcThemes.ubound
		arrsThemes.Push(arrcThemes[i].name)
	next
	s.Join(arrsThemes, ", ")
	console &= "THEMES: " & s
	
	
	
	' GET TAGS from the first theme
	GetFirstLevelChilds(arrcThemes[0], arrcTags)
	if arrcTags.size <= 0 then
		GetSource = false
		exit function
	end if
	for i=0 to arrcTags.ubound
		arrsTags.Push(arrcTags[i].name)
	next
	
	s.Join(arrsTags, ", ")
	console &= "\nTAGS: " & s
	
	arrcSources.Clear()
	for t=0 to arrsTags.ubound
		Dim _arrc As Array[Container]
		arrcSources.Push(_arrc)
	next
	
	' VALIDATE ALL REST TAGS from other themes
	for th=0 to arrcThemes.ubound
		GetFirstLevelChilds(arrcThemes[th], arrcTags)
		for t=0 to arrcTags.ubound
			if arrcTags[t].name <> arrsTags[t] then
				GetSource = false
				exit function
			end if
			arrcSources[t].Push(arrcTags[t])
		next
	next
	
'	console &= "\n---"
'	for t=0 to arrcTags.ubound
'		console &= "\nTAG: " & arrsTags[t]
'		for i=0 to arrcSources[t].ubound
'			console &= "\n└ " & arrcSources[t][i].name
'		next
'	next
	
	GetSource = true
	console &= "\nSOURCE IS VALID"
end function

sub GetTargets()
	cTargetRoot = GetParameterContainer("targets_root")
	if cTargetRoot == null then
		GetAllContainersInScene(arrcRawTargets)
	else
		cTargetRoot.GetContainerAndSubContainers(arrcRawTargets, false)
	end if
	
	arrcTargets.Clear()
	for t=0 to arrsTags.ubound
		Dim _arrc As Array[Container]
		arrcTargets.Push(_arrc)
	next
	
	for i=0 to arrcRawTargets.ubound
		sName = arrcRawTargets[i].name
		for t=0 to arrsTags.ubound
			'if sName.Find("<" & arrsTags[t] & ">") >= 0 then
			if sName.Match("<" & arrsTags[t] & ".*>") then
				arrcTargets[t].Push(arrcRawTargets[i])
			end if
		next
	next
	
	console &= "\n\nTARGETS:"
	for t=0 to arrsTags.ubound
		console &= "\n└ TAG: " & arrsTags[t]
		Dim _names As Array[String]
		for i=0 to arrcTargets[t].ubound
			_names.Push(arrcTargets[t][i].name)
			console &= "\n   └ " & arrcTargets[t][i].name
		next
	next
end sub

sub ApplyTheme(themeIndex As Integer)
	if themeIndex > arrcThemes.ubound then themeIndex = arrcThemes.ubound
	cSourceRoot.GetFunctionPluginInstance("Omo").SetParameterInt("vis_con", themeIndex)
	for t=0 to arrsTags.ubound
		m = arrcSources[t][themeIndex].Material
		for i=0 to arrcTargets[t].ubound
			sThemeType = GetThemeType(arrcTargets[t][i].name, arrsTags[t])
			if sThemeType == "omo" then
				arrcTargets[t][i].GetFunctionPluginInstance("Omo").SetParameterInt("vis_con", themeIndex)
			else
				arrcTargets[t][i].Material = m
			end if
		next
	next
end sub

sub UpdateTheme()
	if GetParameterInt("switch_mode") == SWITCH_MODE_NUMBER then
		ApplyTheme(GetParameterInt("current_theme_index"))
	elseif GetParameterInt("switch_mode") == SWITCH_MODE_OMO then
		iPrevOmo = -1
		ApplyTheme(cSourceRoot.GetFunctionPluginInstance("Omo").GetParameterInt("vis_con"))
	elseif GetParameterInt("switch_mode") == SWITCH_MODE_SHM then
		System.Map.RegisterChangedCallback(GetParameterString("shm_variable"))
		ApplyTheme(CInt(System.Map[GetParameterString("shm_variable")]))
	end if
end sub

function GetThemeType(_name As String, _tag As String) As String
	Dim _sStart = "<" & _tag & ":"
	Dim _iStart = _name.Find(_sStart)
	if _iStart < 0 then
		GetThemeType = ""
		exit function
	end if
	Dim _sMiddle = _name.GetSubstring(_iStart + _sStart.Length, _name.Length - _iStart)
	Dim _iEnd = _sMiddle.Find(">")
	Dim _type = _sMiddle.GetSubstring(0, _iEnd)
	GetThemeType = _type
end function

'''''''''''''''''''''''''''''''

Sub GetFirstLevelChilds(root as Container, ByRef _arr_childs As Array[Container])
	_arr_childs.Clear()
	Dim _c As Container
	_c = root.FirstChildContainer
	Do
		_arr_childs.Push(_c)
		_c = _c.NextContainer
	Loop While _c <> null
End Sub

Sub GetAllContainersInScene(_arrc As Array[Container])
	_arrc.Clear()
	Dim _c As Container = Scene.RootContainer
	Dim _arrcChilds As Array[Container]
	Do
		_c.GetContainerAndSubContainers(_arrcChilds, false)
		for _i=0 to _arrcChilds.ubound
			_arrc.Push(_arrcChilds[_i])
		next
		_c = _c.NextContainer
	Loop While _c <> null
End Sub



