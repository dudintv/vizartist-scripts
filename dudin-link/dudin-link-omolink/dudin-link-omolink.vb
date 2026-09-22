RegisterPluginVersion(1,1,0)
Dim info As String = "Developer: Dmitry Dudin

Omo Link. It gets Omo value from \"this\" container 
and spread the value to 
all sub-conatainers of the Root container.

It filters the target containes by name. 
You can specify multiply names separated 
by comma \",\" and use asterisks \"*\" as wildcards. 
All extra spaces will be trimed.
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
	
	if cRoot <> null then
		cRoot.GetContainerAndSubContainers(arrcChildren, false)
		for n=0 to cTargetNames.ubound
			findContainersByName(arrcChildren, cTargetNames[n], arrcTargets)
		next
	end if
	
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

' Evaluates wildcards (e.g. *Name, Name*, *Name*)
Function MatchPattern(text As String, pattern As String) As Boolean
	' Exact match
	If text == pattern Then 
		MatchPattern = true
		exit function
	End If
	
	' Catch all
	If pattern == "*" Then 
		MatchPattern = true
		exit function
	End If

	Dim parts As Array[String]
	pattern.Split("*", parts)

	' No wildcard was found, and exact match failed
	If parts.size == 1 Then
		MatchPattern = false
		exit function
	End If

	' Single wildcard (e.g. *suffix, prefix*, prefix*suffix)
	If parts.size == 2 Then
		If parts[0] == "" Then
			MatchPattern = text.EndsWith(parts[1])
			exit function
		ElseIf parts[1] == "" Then
			MatchPattern = text.StartsWith(parts[0])
			exit function
		Else
			MatchPattern = text.StartsWith(parts[0]) And text.EndsWith(parts[1]) And text.length >= (parts[0].length + parts[1].length)
			exit function
		End If
	End If
	
	' Wrap-around wildcards (e.g. *keyword*)
	If parts.size == 3 And parts[0] == "" And parts[2] == "" Then
		MatchPattern = (text.Find(parts[1]) <> -1)
		exit function
	End If

	' Fallback for complex multi-wildcards
	For i = 0 To parts.ubound
		If parts[i] <> "" And text.Find(parts[i]) == -1 Then
			MatchPattern = false
			exit function
		End If
	Next
	
	MatchPattern = true
End Function


sub findContainersByName(_arrc as Array[Container], _name as String, ByRef _arrcOut as Array[Container])
	dim bExists as Boolean
	
	for i=0 to _arrc.ubound
		' Pass the container name and the requested string into the MatchPattern function
		if MatchPattern(_arrc[i].name, _name) then
			
			' Avoid pushing duplicates if multiple wildcards match the same container
			bExists = false
			for j=0 to _arrcOut.ubound
				if _arrcOut[j] == _arrc[i] then
					bExists = true
					exit for
				end if
			next
			
			if not bExists then
				_arrcOut.push(_arrc[i])
			end if
			
		end if
	next
end sub
