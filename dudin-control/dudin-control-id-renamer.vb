RegisterPluginVersion(1,1,0)

dim cRoot as Container
dim arrcItems, allItemContainers as Array[Container]
dim arrpi as Array[PluginInstance]
dim currentId, newId, itemName, containerName as String
dim console as String

dim appsmPluginNames As StringMap
appsmPluginNames["ControlText"] = "(TXT)"
appsmPluginNames["ControlContainer"] = "(CTNR)"

sub OnInitParameters()
	RegisterParameterContainer("root", "Root container")
	RegisterParameterBool("add_type", "Add Control plugin type as suffix", true)
	RegisterPushButton("go", "Rename Control Ids", 1)
	RegisterParameterText("console", "", 999, 999)
end sub

sub OnExecAction(buttonId As Integer)
	console = ""
	cRoot = GetParameterContainer("root")
	arrcItems.clear()
	if cRoot == null then cRoot = this
	
	for i=1 to cRoot.ChildContainerCount
		arrcItems.Push(cRoot.GetChildContainerByIndex(i-1))
	next
	
	for i=0 to arrcItems.ubound
		arrcItems[i].GetContainerAndSubContainers(allItemContainers, false)
		for y=0 to allItemContainers.ubound
			arrpi = GetControlPlugins(allItemContainers[y])
			println("---")
			for k=0 to arrpi.ubound
				itemName = arrcItems[i].name
				itemName.substitute("_", "-", true)
				containerName = allItemContainers[y].name
				containerName.substitute("_", "-", true)
			
				currentId = arrpi[k].GetParameterString("field_id")
				newId = itemName & "-" & containerName
				if GetParameterBool("add_type") then
					newId &= appsmPluginNames[arrpi[k].PluginName]
				end if
				
				if currentId == newId then
					console &= "[HAVEN'T CHANGE] " & newId
				else
					console &= currentId & "  --->  " & newId
				end if
				console &= "\n"
				arrpi[k].SetParameterString("field_id", newId)
			next
		next
	next
	
	this.ScriptPluginInstance.SetParameterString("console",console)
end sub




function GetControlPlugins(_c as Container) as Array[PluginInstance]
	dim _arrpiResult as Array[PluginInstance]
	_c.GetFunctionPluginInstances(_arrpiResult)
	for i=0 to _arrpiResult.ubound
		println("_arrpiResult[i].PluginName = " & _arrpiResult[i].PluginName)
		if not appsmPluginNames.ContainsKey(_arrpiResult[i].PluginName) then
			_arrpiResult.erase(i)
			i -= 1
		end if
	next
	
	for i=0 to _arrpiResult.ubound
		println("_arrpiResult i = " & _arrpiResult[i].PluginName)
	next
	
	GetControlPlugins = _arrpiResult
end function



