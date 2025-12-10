RegisterPluginVersion(1,4,0)

dim cRoot as Container
dim arrcItems, allItemContainers as Array[Container]
dim arrpi as Array[PluginInstance]
dim currentId, newId, itemName, containerName as String
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
	RegisterParameterContainer("root", "Root container of the list")
	RegisterParameterBool("root_prefix", "Prefix from parent", false)
	RegisterParameterBool("add_type", "Add Control plugin type as suffix", true)
	RegisterParameterBool("rename_id", "Rename IDs", true)
	RegisterParameterBool("rename_description", "Rename descriptions", true)
	RegisterParameterString("skip_regex", "Skip if ID has regex:", "", 99, 999, "")
	RegisterPushButton("go", "   Rename   ", 1)
	RegisterParameterText("console", "", 999, 999)
end sub

sub OnInit()
	this.ScriptPluginInstance.SetParameterString("console", "")
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
				containerName.substitute("_", "-", true)
			
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
