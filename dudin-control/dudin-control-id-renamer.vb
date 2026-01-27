RegisterPluginVersion(2,1,0)

dim cRoot as Container
dim arrItems, allItemContainers as Array[Container]
dim arrP as Array[PluginInstance]
dim currentFieldId, newId, newDescription, itemName, containerName, commonPrefix as String
dim console, skipRegex as String

dim appSmPluginNames As StringMap

structure ContainerWithControls
	c as Container
	arrP as Array[PluginInstance]
	name as String
	index as Integer
	nameOrder as Integer
	isIgnored as Boolean
end structure
dim containersWithControls as Array[ContainerWithControls]

appSmPluginNames["ControlContainer"] = "(CNTR)"
appSmPluginNames["ControlImage"] = "(IMG)"
appSmPluginNames["ControlHideOnEmpty"] = "(HIDE)"
appSmPluginNames["ControlNum"] = "(NUM)"
appSmPluginNames["ControlOmo"] = "(OMO)"
appSmPluginNames["ControlParameter"] = "(PARAM)"
appSmPluginNames["ControlText"] = "(TXT)"

dim buttonNamesPrefix As Array[String]
buttonNamesPrefix.Push("Parent")
buttonNamesPrefix.Push("Static")
dim PREFIX_FROM_PARENT = 0
dim PREFIX_STATIC = 1

dim buttonNamesItemMode As Array[String]
buttonNamesItemMode.Push("Index")
buttonNamesItemMode.Push("Name")
buttonNamesItemMode.Push("Name+Index")
dim ITEM_INDEX = 0
dim ITEM_NAME = 1
dim ITEM_NAME_INDEX = 2

dim buttonNamesContainerMode As Array[String]
buttonNamesContainerMode.Push("Index")
buttonNamesContainerMode.Push("Name")
buttonNamesContainerMode.Push("Name+Index")
dim CONTAINER_INDEX = 0
dim CONTAINER_NAME = 1
dim CONTAINER_NAME_INDEX = 2

dim buttonNamesContainerNameFrom As Array[String]
buttonNamesContainerNameFrom.Push("Full name")
buttonNamesContainerNameFrom.Push("Suffix")
dim GET_CONTAINER_NAME_FROM_FULL_NAME = 0
dim GET_CONTAINER_NAME_FROM_SUFFIX = 1

dim buttonNamesSuffix As Array[String]
buttonNamesSuffix.Push("Control-based")
buttonNamesSuffix.Push("Static")
dim SUFFIX_CONTROL_BASED = 0
dim SUFFIX_STATIC = 1

dim buttonNamesFilter As Array[String]
buttonNamesFilter.Push("Include")
buttonNamesFilter.Push("Exclude")
dim FILTER_INCLUDE = 0
dim FILTER_EXCLUDE = 1

dim buttonNamesDescription As Array[String]
buttonNamesDescription.Push("As ID")
buttonNamesDescription.Push("Extracted name")
buttonNamesDescription.Push("Full container name")
buttonNamesDescription.Push("Total details")
dim DESCRIPTION_SAME_AS_ID = 0
dim DESCRIPTION_NAME = 1
dim DESCRIPTION_FULL_NAME = 2
dim DESCRIPTION_TOTAL = 3

dim BUTTON_PREVIEW = 0
dim BUTTON_RENAME = 1

sub OnInitParameters()
	RegisterParameterContainer("root", "Root (or this)")
	RegisterParameterBool("has_rename_id", "Change IDs", false)

	'prefix
	RegisterParameterBool("has_common_prefix", "● Common Prefix", false)
	RegisterRadioButton("prefix_mode", "   └ Get prefix from", 0, buttonNamesPrefix)
	RegisterParameterString("static_prefix", "       └ Prefix", "", 99, 999, "")
	RegisterParameterString("common_prefix_separator", "       └ Separator", "", 99, 999, "")

	'item name
	RegisterParameterBool("has_item", "● Item part", false)
	RegisterRadioButton("item_mode", "   └ Item mode", 0, buttonNamesItemMode)
	RegisterParameterInt("item_index_start", "       └ Index start", 1, 0, 99)
	RegisterParameterInt("item_index_width", "       └ Min index width", 2, 1, 99)

	RegisterParameterString("item_separator", "   └ Separator", "", 99, 999, "")

	'container name
	RegisterParameterBool("has_container", "● Container part", false)
	RegisterRadioButton("container_mode", "   └ Container mode", 0, buttonNamesContainerMode)
	RegisterParameterInt("container_index_width", "       └ Min index width", 2, 1, 99)
	RegisterParameterBool("has_same_number_for_same_name", "       └ Same index for same name", false)
	RegisterRadioButton("get_container_name_from", "       └ Get name from", 0, buttonNamesContainerNameFrom)
	RegisterParameterString("container_name_separator", "           └ Suffix separator", ":", 6, 6, "")

	'suffix
	RegisterParameterBool("has_suffix", "● Suffix part", false)
	RegisterRadioButton("suffix_mode", "   └ Suffix mode", 0, buttonNamesSuffix)
	RegisterParameterString("suffix_separator", "   └ Separator", "", 99, 999, "")
	RegisterParameterString("static_suffix", "   └ Static suffix", "", 99, 999, "")

	'description
	RegisterParameterBool("has_rename_description", "Change descriptions", false)
	RegisterRadioButton("description_mode", "   └ Description mode", 0, buttonNamesDescription)

	'filter
	RegisterParameterBool("has_filter", "Filter", false)
	RegisterParameterBool("has_filter_by_container_name", "   └ By container name", false)
	RegisterRadioButton("filter_by_container_name_mode", "       └ Filter mode", 0, buttonNamesFilter)
	RegisterParameterString("filter_by_container_name_regex", "       └ Filter regex:", "", 99, 999, "")
	RegisterParameterBool("has_filter_by_control_id", "   └ By control ID", false)
	RegisterRadioButton("filter_by_control_id_mode", "       └ Filter mode", 0, buttonNamesFilter)
	RegisterParameterString("filter_by_control_id_regex", "       └ Filter regex:", "", 99, 999, "")

	'action
	RegisterPushButton("preview", "   Preview   ", BUTTON_PREVIEW)
	RegisterPushButton("go", "   Rename   ", BUTTON_RENAME)
	RegisterParameterText("console", "", 999, 999)
end sub

sub OnInit()
	this.ScriptPluginInstance.SetParameterString("console", "")
end sub

dim hasId, hasDescription, hasPrefix, hasItem, hasContainer, hasSuffix, hasStaticSuffix, hasItemIndex, hasContainerIndex, hasContainerName, hasContainerNameSeparator As Boolean
dim hasFilter, hasFilterByContainerName, hasFilterByControlId as Boolean
sub OnParameterChanged(parameterName As String)
	hasId = GetParameterBool("has_rename_id")

	'prefix
	SendGuiParameterShow("has_common_prefix", CInt(hasId))
	hasPrefix = GetParameterBool("has_common_prefix")
	SendGuiParameterShow("prefix_mode", CInt(hasId AND hasPrefix))
	SendGuiParameterShow("common_prefix_separator", CInt(hasId AND hasPrefix AND GetParameterInt("prefix_mode") == PREFIX_FROM_PARENT))
	SendGuiParameterShow("static_prefix", CInt(hasId AND hasPrefix AND GetParameterInt("prefix_mode") == PREFIX_STATIC))

	'item name
	SendGuiParameterShow("has_item", CInt(hasId))
	hasItem = GetParameterBool("has_item")
	SendGuiParameterShow("item_mode", CInt(hasId AND hasItem))
	hasItemIndex = GetParameterInt("item_mode") == ITEM_INDEX OR GetParameterInt("item_mode") == ITEM_NAME_INDEX
	SendGuiParameterShow("item_index_start", CInt(hasId AND hasItem AND hasItemIndex))
	SendGuiParameterShow("item_index_width", CInt(hasId AND hasItem AND hasItemIndex))
	SendGuiParameterShow("item_separator", CInt(hasId AND hasItem))

	'container name
	SendGuiParameterShow("has_container", CInt(hasId))
	hasContainer = GetParameterBool("has_container")
	SendGuiParameterShow("container_mode", CInt(hasId AND hasContainer))
	hasContainerIndex = GetParameterInt("container_mode") == CONTAINER_INDEX OR GetParameterInt("container_mode") == CONTAINER_NAME_INDEX
	hasContainerName = GetParameterInt("container_mode") == CONTAINER_NAME OR GetParameterInt("container_mode") == CONTAINER_NAME_INDEX
	SendGuiParameterShow("container_index_width", CInt(hasId AND hasContainer AND hasContainerIndex))
	SendGuiParameterShow("has_same_number_for_same_name", CInt(hasId AND hasContainer AND hasContainerIndex))
	SendGuiParameterShow("get_container_name_from", CInt(hasId AND hasContainer AND (hasContainerName OR GetParameterBool("has_same_number_for_same_name"))))	
	hasContainerNameSeparator = GetParameterInt("get_container_name_from") == GET_CONTAINER_NAME_FROM_SUFFIX
	SendGuiParameterShow("container_name_separator", CInt(hasId AND hasContainer AND (hasContainerName OR GetParameterBool("has_same_number_for_same_name")) AND hasContainerNameSeparator))

	'suffix
	SendGuiParameterShow("has_suffix", CInt(hasId))
	hasSuffix = GetParameterBool("has_suffix")
	SendGuiParameterShow("suffix_mode", CInt(hasId AND hasSuffix))
	hasStaticSuffix = GetParameterInt("suffix_mode") == SUFFIX_STATIC
	SendGuiParameterShow("suffix_separator", CInt(hasId AND hasSuffix AND NOT hasStaticSuffix))
	SendGuiParameterShow("static_suffix", CInt(hasId AND hasSuffix AND hasStaticSuffix))

	'description
	hasDescription = GetParameterBool("has_rename_description")
	SendGuiParameterShow("description_mode", CInt(hasDescription))

	'filter
	hasFilter = GetParameterBool("has_filter")
	SendGuiParameterShow("has_filter_by_container_name", CInt(hasFilter))
	SendGuiParameterShow("has_filter_by_control_id", CInt(hasFilter))
	hasFilterByContainerName = GetParameterBool("has_filter_by_container_name")
	SendGuiParameterShow("filter_by_container_name_mode", CInt(hasFilter AND hasFilterByContainerName))
	SendGuiParameterShow("filter_by_container_name_regex", CInt(hasFilter AND hasFilterByContainerName))
	hasFilterByControlId = GetParameterBool("has_filter_by_control_id")
	SendGuiParameterShow("filter_by_control_id_mode", CInt(hasFilter AND hasFilterByControlId))
	SendGuiParameterShow("filter_by_control_id_regex", CInt(hasFilter AND hasFilterByControlId))
end sub

sub OnExecAction(buttonId As Integer)
	console = ""
	if buttonId == BUTTON_PREVIEW then
		console = "PREVIEW MODE (NO REAL CHANGES)\n\n"
	end if

	cRoot = GetParameterContainer("root")
	if cRoot == null then cRoot = this

	arrItems.clear()
	for i=1 to cRoot.ChildContainerCount
		arrItems.Push(cRoot.GetChildContainerByIndex(i-1))
	next

	for i=0 to arrItems.ubound
		arrItems[i].GetContainerAndSubContainers(allItemContainers, false)
		itemName = arrItems[i].name
		itemName.substitute("_", "-", true)

		console &= "ITEM: " & arrItems[i].name & "\n"
		containersWithControls = GetContainersWithControls(arrItems[i])
		for y=0 to containersWithControls.ubound
			containerName = containersWithControls[y].name
			arrP = containersWithControls[y].arrP

			if containersWithControls[y].isIgnored then
				console &= " └ [SKIPPED CONTAINER] " & containerName & "\n"
			else
				for k=0 to arrP.ubound

					currentFieldId = arrP[k].GetParameterString("field_id")

					dim shouldIgnoreByControlId = false
					if GetParameterBool("has_filter") AND GetParameterBool("has_filter_by_control_id") then
						select case GetParameterInt("filter_by_control_id_mode")
						case FILTER_INCLUDE
							shouldIgnoreByControlId = NOT currentFieldId.Match(GetParameterString("filter_by_control_id_regex"))
						case FILTER_EXCLUDE
							shouldIgnoreByControlId =currentFieldId.Match(GetParameterString("filter_by_control_id_regex"))
						end select
					end if

					if shouldIgnoreByControlId then
						console &= " └ [SKIPPED CONTROL] " & currentFieldId
					else

						'GET NEW ID
						newId = ""

						if GetParameterBool("has_common_prefix") then
							if GetParameterInt("prefix_mode") == PREFIX_STATIC then
								newId &= GetParameterString("static_prefix")
							else
								commonPrefix = cRoot.name
								commonPrefix.substitute("_", "-", true)
								newId &= commonPrefix & GetParameterString("common_prefix_separator")
							end if
						end if

						if hasItem then
							dim itemNameIndex = CStr(i + GetParameterInt("item_index_start"))
							itemNameIndex.PadLeft(GetParameterInt("item_index_width"), "0")
							select case GetParameterInt("item_mode")
							Case ITEM_INDEX
								newId &= itemNameIndex
							Case ITEM_NAME
								newId &= itemName
							Case ITEM_NAME_INDEX
								newId &= itemName & itemNameIndex
							end select
							newId &= GetParameterString("item_separator")
						end if

						if hasContainer then
							dim containerNameIndex = CStr(containersWithControls[y].nameOrder)
							containerNameIndex.PadLeft(GetParameterInt("container_index_width"), "0")
							select case GetParameterInt("container_mode")
							Case CONTAINER_INDEX
								newId &= containerNameIndex
							Case CONTAINER_NAME
								newId &= containerName
							Case CONTAINER_NAME_INDEX
								newId &= containerName & containerNameIndex
							end select
						end if


						if hasSuffix then
							newId &= GetParameterString("suffix_separator")
							select case GetParameterInt("suffix_mode")
							case SUFFIX_CONTROL_BASED
								newId &= appSmPluginNames[arrP[k].PluginName]
							case SUFFIX_STATIC
								newId &= GetParameterString("static_suffix")
							end select
						end if

						'GET NEW DESCRIPTION
						
						select case GetParameterInt("description_mode")
						case DESCRIPTION_SAME_AS_ID
							newDescription = newId
						case DESCRIPTION_NAME
							newDescription = containerName
						case DESCRIPTION_FULL_NAME
							newDescription = containersWithControls[y].c.name
						case DESCRIPTION_TOTAL
							newDescription = itemName & " " & containersWithControls[y].c.name & " " & appSmPluginNames[arrP[k].PluginName]
						end select
						

						'APPLY
						if GetParameterBool("has_rename_id") then
							if currentFieldId == newId then
								console &= " └ " & "[HAVEN'T CHANGED] " & newId
							else
								console &= " └ " & currentFieldId & "  --->  " & newId
							end if
							if buttonId == BUTTON_RENAME then
								arrP[k].SetParameterString("field_id", newId)
							end if
						end if
						if GetParameterBool("has_rename_description") then
							if buttonId == BUTTON_RENAME then
								arrP[k].SetParameterString("description", newDescription)
							end if
							console &= "   |   " & newDescription
						end if

					end if

					console &= "\n"
				next
			end if
		next
	next

	this.ScriptPluginInstance.SetParameterString("console",console)
end sub

function GetContainersWithControls(_c As Container) as Array[ContainerWithControls]
	dim _arrResult as Array[ContainerWithControls]
	dim _arrChild as Array[Container]
	dim _arrP as Array[PluginInstance]
	_c.GetContainerAndSubContainers(_arrChild, false)
	_arrChild.Erase(0)
	dim _nameOrder = 0
	for i=0 to _arrChild.UBound
		_arrP = GetControlPlugins(_arrChild[i])
		if _arrP.size > 0 then
			dim _cWithControls As ContainerWithControls
			_cWithControls.c = _arrChild[i]
			_cWithControls.arrP = _arrP
			_cWithControls.name = GetContainerName(_arrChild[i])
			_cWithControls.index = i
			
			if GetParameterBool("has_same_number_for_same_name") then
				_cWithControls.nameOrder = FindContainerWithControlByName(_arrResult, _cWithControls.name).nameOrder
				if _cWithControls.nameOrder <= 0 then
					_nameOrder += 1
					_cWithControls.nameOrder = _nameOrder
				end if
			else
				_nameOrder += 1
				_cWithControls.nameOrder = _nameOrder
			end if

			_cWithControls.isIgnored = false
			if GetParameterBool("has_filter") AND GetParameterBool("has_filter_by_container_name") then
				select case GetParameterInt("filter_by_container_name_mode")
				case FILTER_INCLUDE
					_cWithControls.isIgnored = NOT _arrChild[i].name.Match(GetParameterString("filter_by_container_name_regex"))
				case FILTER_EXCLUDE
					_cWithControls.isIgnored = _arrChild[i].name.Match(GetParameterString("filter_by_container_name_regex"))
				end select
			end if
			_arrResult.Push(_cWithControls)
		end if
	next
	GetContainersWithControls = _arrResult
end function

function GetContainerName(_c as Container) as String
	dim _name as String
	_name = _c.name
	if GetParameterInt("get_container_name_from") == GET_CONTAINER_NAME_FROM_SUFFIX then
		dim _nameSeparator as String = GetParameterString("container_name_separator")
		dim _nameSeparatorStartPos = _name.Find(_nameSeparator) + _nameSeparator.Length
		_name = _name.Right(_name.Length - _nameSeparatorStartPos)
	end if
	_name.substitute("_", "-", true)
	_name.substitute(":", "-", true)
	GetContainerName = _name
end function

function GetControlPlugins(_c as Container) as Array[PluginInstance]
	dim _arrPiResult as Array[PluginInstance]
	_c.GetFunctionPluginInstances(_arrPiResult)

	'search for allowed control plugins
	for i=0 to _arrPiResult.ubound
		if not appSmPluginNames.ContainsKey(_arrPiResult[i].PluginName) then
			_arrPiResult.erase(i)
			i -= 1
		end if
	next

	GetControlPlugins = _arrPiResult
end function

function FindContainerWithControlByName(_arrCWC as Array[ContainerWithControls], _name as String) as ContainerWithControls
	for i=0 to _arrCWC.ubound
		if _arrCWC[i].name == _name then
			FindContainerWithControlByName = _arrCWC[i]
			exit function
		end if
	next
end function
