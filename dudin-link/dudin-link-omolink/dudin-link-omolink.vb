RegisterPluginVersion(1,0,0)
Dim info As String = "Developer: Dmitry Dudin

Omo Link. It gets Omo value from \"this\" container 
and spread the value to 
all sub-conatainers of the Root container.

It filters the target containes by name. 
You can specify multiply names separated 
by comma \",\". All extra spaces will be trimed.
"

dim cRoot as Container
dim cTargetNames as Array[String]
dim s, console as String
dim arrcTargets, arrcChildren as Array[Container]
dim pCurrentOmo as PluginInstance
dim iOmo as Integer

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterString("target_names", "Target names (,)", "", 60, 999, "")
	RegisterParameterContainer("target_root" , "Target's Root")
	RegisterPushButton("init", "Initialize", 1)
	
	RegisterPushButton("debug", "Show debug info", 2)
	RegisterParameterText("console", "Debug console", 600, 200)
end sub

sub OnInit()
	cRoot = GetParameterContainer("target_root")
	s = GetParameterString("target_names")
	s.split(",", cTargetNames)
	for i=0 to cTargetNames.ubound
		cTargetNames[i].trim()
	next
	
	arrcTargets.clear()
	cRoot.GetContainerAndSubContainers(arrcChildren, false)
	for n=0 to cTargetNames.ubound
		findContainersByName(arrcChildren, cTargetNames[n], arrcTargets)
	next

	
	pCurrentOmo = this.GetFunctionPluginInstance("Omo")
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	iOmo = pCurrentOmo.GetParameterInt("vis_con")
	for i=0 to arrcTargets.ubound
		arrcTargets[i].GetFunctionPluginInstance("Omo").SetParameterInt("vis_con", iOmo)
	next
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		OnInit()
	elseif buttonId == 2 then
		console = "Targets amount = " & arrcTargets.size & "\n"
		console &= "---\n"
		for i=0 to arrcTargets.ubound
			console &= arrcTargets[i].name & "\n"
		next
	end if
	this.ScriptPluginInstance.SetParameterString("console",console)
end sub

'----------------------------------------------------

sub findContainersByName(_arrc as Array[Container], _name as String, ByRef _arrcOut as Array[Container])
	dim _result as Array[Container]
	for i=0 to _arrc.ubound
		if _arrc[i].name == _name then
			_arrcOut.push(_arrc[i])
		end if
	next
end sub



