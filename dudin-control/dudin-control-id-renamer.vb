RegisterPluginVersion(1,0,0)

dim cRoot as Container
dim arrcItems, allItemContainers as Array[Container]
dim pi as PluginInstance
dim currentId, newId, itemName, containerName as String
dim console as String

sub OnInitParameters()
	RegisterParameterContainer("root", "Root container")
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
			pi = allItemContainers[y].GetFunctionPluginInstance("ControlText")
			if pi <> null then
				itemName = arrcItems[i].name
				itemName.substitute("_", "-", true)
				containerName = allItemContainers[y].name
				containerName.substitute("_", "-", true)
			
				currentId = pi.GetParameterString("field_id")
				newId = itemName & "-" & containerName
				
				if currentId == newId then
					console &= "[HAVEN'T CHANGE] " & newId
				else
					console &= currentId & "  --->  " & newId
				end if
				console &= "\n"
				pi.SetParameterString("field_id", newId)
			end if
		next
	next
	
	this.ScriptPluginInstance.SetParameterString("console",console)
end sub

