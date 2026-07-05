RegisterPluginVersion(1,0,0)
Dim info As String = "
Theme Materials Switcher
Developer: Dmitry Dudin
"

Dim arrcThemes, arrcTags, arrcRawTargets As Array[Container]
Dim arrcSources As Array[Array[Container]]
Dim arrcTargets As Array[Array[Container]]
Dim arrsThemes As Array[String]
Dim arrsTags As Array[String]
Dim s, console, sName As String
Dim cSourceRoot, cTargetRoot As Container
Dim m As Material

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
	RegisterParameterInt("current_theme_index", "Current Theme", 0, 0, 999)
	RegisterParameterText("console", "", 600, 240)
end sub

sub OnInit()
	console = ""
	GetSource()
	GetTargets()
	this.ScriptPluginInstance.SetParameterString("console",console)
end sub
sub OnParameterChanged(parameterName As String)
	if parameterName <> "console" then
		OnInit()
	end if
	
	if parameterName == "current_theme_index" then
		Dim _i As Integer = GetParameterInt("current_theme_index")
		if _i > arrcThemes.ubound then _i = arrcThemes.ubound
		ApplyTheme(_i)
	end if
end sub

''''''''''''''''''''''''''''''

function GetSource() as Boolean
	arrsThemes.Clear()
	arrsTags.Clear()
	cSourceRoot = GetParameterContainer("sources_root")
	if cSourceRoot == null then cSourceRoot = this
	
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
			if sName.Find("<" & arrsTags[t] & ">") >= 0 then
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
	println("themeIndex = " & themeIndex)
	for t=0 to arrsTags.ubound
		m = arrcSources[t][themeIndex].Material
		for i=0 to arrcTargets[t].ubound
			arrcTargets[t][i].Material = m
		next
	next
end sub

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
