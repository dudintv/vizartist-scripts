RegisterPluginVersion(1,5,0)

dim cRoot as Container
dim arrcItems, allItemContainers as Array[Container]
dim arrpi as Array[PluginInstance]
dim currentId, newId, itemName, containerName, suffixSeparator as String
dim console, skipRegex as String

dim appsmPluginNames As StringMap

appsmPluginNames["ControlContainer"] = "(CTNR)"
appsmPluginNames["ControlImage"] = "(IMG)"
appsmPluginNames["ControlHideOnEmpty"] = "(HIDE)"
appsmPluginNames["ControlNum"] = "(NUM)"
appsmPluginNames["ControlOmo"] = "(OMO)"
appsmPluginNames["ControlParameter"] = "(PARAM)"
appsmPluginNames["ControlText"] = "(TXT)"

sub OnInitParameters()
	RegisterParameterContainer("root", "Root (or this)")
	RegisterParameterBool("root_prefix", "Prefix from parent", false)
	RegisterParameterBool("add_type", "Add Control plugin type as suffix", true)
	RegisterParameterBool("rename_id", "Rename IDs", true)
	RegisterParameterBool("rename_description", "Rename descriptions", true)
	RegisterParameterString("skip_regex", "Skip if ID has regex:", "", 99, 999, "")
	RegisterParameterBool("has_name_from_siffux", "Get ids name from suffix", true)
	RegisterParameterString("suffix_separator", "Suffix separator", ":", 6, 6, "")
	RegisterPushButton("go", "   Rename   ", 1)
	RegisterParameterText("console", "", 999, 999)
end sub

sub OnInit()
	this.ScriptPluginInstance.SetParameterString("console", "")
end sub
sub OnParameterChanged(parameterName As String)
	SendGuiParameterShow("suffix_separator", CInt(GetParameterBool("has_name_from_siffux")))
	suffixSeparator = GetParameterString("suffix_separator")
end sub

sub OnExecAction(buttonId As Integer)
	console = ""
	cRoot = GetParameterContainer("root")
	skipRegex = GetParameterString("skip_regex")
	arrcItems.clear()
	if cRoot == null then cRoot = this
	
	for i=1 to cRoot.ChildContainerCount
		arrcItems.Push(cRoot.GetChildContainerByIndex(i-1))
	next
	
	for i=0 to arrcItems.ubound
		arrcItems[i].GetContainerAndSubContainers(allItemContainers, false)
		for y=0 to allItemContainers.ubound
			arrpi = GetControlPlugins(allItemContainers[y])
			for k=0 to arrpi.ubound
				itemName = arrcItems[i].name
				itemName.substitute("_", "-", true)
				containerName = allItemContainers[y].name
				if GetParameterBool("has_name_from_siffux") then
					dim suffixStartPos = containerName.Find(suffixSeparator) + suffixSeparator.Length
					containerName = containerName.Right(containerName.Length - suffixStartPos)
				end if
				containerName.substitute("_", "-", true)
				containerName.substitute(":", "-", true)
			
				currentId = arrpi[k].GetParameterString("field_id")
				
				
				if skipRegex <> "" AND currentId.Match(skipRegex) then
					'skippng
					console &= "[SKIPPED] " & currentId
				else
					newId = itemName & "-" & containerName
					
					
					if GetParameterBool("root_prefix") then
						dim prefix = cRoot.name
						prefix.substitute("_", "-", true)
						newId = cRoot.name & "-" & newId
					end if
					if GetParameterBool("add_type") then
						newId &= appsmPluginNames[arrpi[k].PluginName]
					end if
					
					if currentId == newId then
						console &= "[HAVEN'T CHANGED] " & newId
					else
						console &= currentId & "  --->  " & newId
					end if
					
					if GetParameterBool("rename_id") then	
						arrpi[k].SetParameterString("field_id", newId)
					end if
					if GetParameterBool("rename_description") then	
						arrpi[k].SetParameterString("description", newId)
					end if
				end if
				console &= "\n"
			next
		next
	next
	
	this.ScriptPluginInstance.SetParameterString("console",console)
end sub




function GetControlPlugins(_c as Container) as Array[PluginInstance]
	dim _arrpiResult as Array[PluginInstance]
	_c.GetFunctionPluginInstances(_arrpiResult)
	
	'search for allowed control plugins
	for i=0 to _arrpiResult.ubound
		if not appsmPluginNames.ContainsKey(_arrpiResult[i].PluginName) then
			_arrpiResult.erase(i)
			i -= 1
		end if
	next
	
	GetControlPlugins = _arrpiResult
end function



