RegisterPluginVersion(1,7,0)
Dim info As String = "Moving/arrange animation channels to certain directors. to Developer: Dmitry Dudin, dudin.tv"

Dim cRoot As Container
Dim arr_c_parts, arr_c As Array[Container]
Dim arr_ch As Array[Channel]
Dim dir, parentDir As Director
Dim dir_name, parentDirPath As String
Dim offset_start, offset_step, offset As Double
Dim arrFilterNames, arrParentDirNames As Array[String]

Dim arr_type As Array[String]
arr_type.Push("To single director")
arr_type.Push("By childs name")
arr_type.Push("By childs name + index")
Dim ARRANGE_TO_SINGLE_DIR As Integer = 0
Dim ARRANGE_TO_DIRS_BY_CONTS_NAME As Integer = 1
Dim ARRANGE_TO_DIRS_BY_CONTS_NAME_PLUS_INDEX As Integer = 2

Dim arrFilterTypes As Array[String]
arrFilterTypes.Push("None")
arrFilterTypes.Push("Include")
arrFilterTypes.Push("Exclude")
Dim FILTER_TYPE_NONE = 0
Dim FILTER_TYPE_INCLUDE = 1
Dim FILTER_TYPE_EXCLUDE = 2

Dim arrOffsetTypes As Array[String]
arrOffsetTypes.Push("Don't change")
arrOffsetTypes.Push("Reset to 0")
arrOffsetTypes.Push("Stager effect")
Dim OFFSET_TYPE_DONT_CHANGE = 0
Dim OFFSET_TYPE_RESET = 1
Dim OFFSET_TYPE_STAGER = 2

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterContainer("root", "Root container (or this)")
	RegisterRadioButton("type", "Where to place", 0, arr_type)
	RegisterParameterString("single_dir_name", " └ Director Name", "", 40, 999, "")
	RegisterParameterString("parent_dir", " └ Parent director (path: a/b/c)", "", 40, 999, "")
	RegisterParameterString("prefix_dir_name", " └ Prefix for directors names", "", 40, 999, "")
	RegisterParameterString("suffix_dir_name", " └ Suffix for directors names", "", 40, 999, "")
	RegisterRadioButton("offset_type", "Offset of directors", 0, arrOffsetTypes)
	RegisterParameterDouble("offset_start", " └ Offset start (sec)", 0, -999999, 999999)
	RegisterParameterDouble("offset_step", " └ Offset step (sec)", 0, -999999, 999999)
	RegisterParameterBool("reverse_order", " └ Reverse order", false)
	RegisterRadioButton("filter_type", "Filter by container names", FILTER_TYPE_NONE, arrFilterTypes)
	RegisterParameterString("filter", " └ Filter name (a,b,c)", "", 99, 999, "")
	RegisterParameterBool("regex", " └ Use regex syntax (a|b|c)", false)
	RegisterPushButton("arrange", "Arrange animations", 1)
end sub

sub OnGuiStatus()
	Dim isStagerOffset = GetParameterInt("offset_type") == OFFSET_TYPE_STAGER
	SendGuiParameterShow("single_dir_name", CInt(GetParameterInt("type") == 0))
	SendGuiParameterShow("reverse_order", CInt(isStagerOffset))
	SendGuiParameterShow("offset_start", CInt(isStagerOffset))
	SendGuiParameterShow("offset_step", CInt(isStagerOffset))
	SendGuiParameterShow("filter", CInt(GetParameterInt("filter_type") <> FILTER_TYPE_NONE))
	SendGuiParameterShow("prefix_dir_name", Cint(GetParameterInt("type") <> 0))
	SendGuiParameterShow("suffix_dir_name", Cint(GetParameterInt("type") <> 0))
end sub

sub OnInit()
	cRoot = GetParameterContainer("root")
	if cRoot == null then cRoot = this
	
	arr_c_parts.Clear()
	for i=1 to cRoot.ChildContainerCount
		arr_c_parts.Push(cRoot.GetChildContainerByIndex(i-1))
	next
	offset_start = GetParameterDouble("offset_start")
	offset_step = GetParameterDouble("offset_step")
	
	GetParameterString("filter").split(",", arrFilterNames)
	for i=0 to arrFilterNames.ubound
		arrFilterNames[i].trim()
	next
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecAction(buttonId As Integer)
	OnInit()
	if buttonId == 1 then
		ArrangeAnimation()
	end if
end sub

Sub ArrangeAnimation()
	GetParentDirector()
	
	for i=0 to arr_c_parts.ubound
		'setup director
		if GetParameterInt("type") == ARRANGE_TO_SINGLE_DIR then
			dir_name = GetParameterString("single_dir_name")
		elseif GetParameterInt("type") == ARRANGE_TO_DIRS_BY_CONTS_NAME then
			dir_name = GetParameterString("prefix_dir_name") & arr_c_parts[i].name & GetParameterString("suffix_dir_name")
		elseif GetParameterInt("type") == ARRANGE_TO_DIRS_BY_CONTS_NAME_PLUS_INDEX then
			dir_name = GetParameterString("prefix_dir_name") & arr_c_parts[i].name & GetParameterString("suffix_dir_name") & CStr(i+1)
		end if
		
		if parentDir <> null then
			dir = parentDir.FindSubDirector(dir_name)
		else
			dir = Stage.FindDirector(dir_name)
		end if
		
		if dir == null then
			if parentDir <> null then 
				dir = parentDir.AddDirector(TL_DOWN)
				println("! dir = " & dir.name)
			else
				dir = Stage.RootDirector.AddDirector(TL_NEXT)
			end if
			dir.name = dir_name
		end if
		
		'setup offset
		if GetParameterInt("offset_type") == OFFSET_TYPE_STAGER then
			if GetParameterBool("reverse_order") then
				offset = offset_start + offset_step*(arr_c_parts.ubound - i)
			else
				offset = offset_start + offset_step*i
			end if
			dir.offset = offset
		elseif GetParameterInt("offset_type") == OFFSET_TYPE_RESET then
			dir.offset = 0
		end if
		
		arr_c.Clear()
		arr_c_parts[i].GetContainerAndSubContainers(arr_c, false)
		for y=0 to arr_c.ubound
			if IsFilterPassed(arr_c[y].name) AND arr_c[y].GetChannelsOfObject(arr_ch) > 0 then
				for j=0 to arr_ch.ubound
					arr_ch[j].MoveToDirector(dir)
				next
			end if
		next
	next
End Sub

sub GetParentDirector()
	parentDirPath = GetParameterString("parent_dir")
	parentDirPath.trim()
	parentDirPath.split("/", arrParentDirNames)
	parentDir = null
	for i=0 to arrParentDirNames.ubound
	println("arrParentDirNames[i] = " & arrParentDirNames[i])
		arrParentDirNames[i].trim()
		if parentDir == null then
			parentDir = Stage.FindDirector(arrParentDirNames[i])
		else
			parentDir = parentDir.FindSubDirector(arrParentDirNames[i])
		end if
	next
	
	println("parentDir = " & parentDir.name)
end sub

Function IsFilterPassed(name As String) As Boolean
	Dim hasFilter = GetParameterInt("filter_type") <> FILTER_TYPE_NONE
	
	if NOT hasFilter Then
		IsFilterPassed = true
		exit function
	end if
	
	Dim isNameFound = false
	for i=0 to arrFilterNames.ubound
		if GetParameterBool("regex") then
			if name.Match(GetParameterString("filter")) then isNameFound = true
		else
			if name == arrFilterNames[i] then isNameFound = true
		end if
	next
	
	Dim includePass = GetParameterInt("filter_type") == FILTER_TYPE_INCLUDE AND isNameFound
	Dim excludePass = GetParameterInt("filter_type") == FILTER_TYPE_EXCLUDE AND NOT isNameFound
	IsFilterPassed = includePass OR excludePass
End Function
