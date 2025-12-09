RegisterPluginVersion(1,0,0)

Structure SceneContainer
    c As Container
    level As Integer
End Structure

dim arrc as Array[Container]
dim name, console, levelShift as String
dim arrSceneContainers as Array[SceneContainer]
dim maxNameLength, level as Integer

dim textureMessage, textMessage, omoMessage as String
dim hasTexture, hasImageControl, isTextureIgnored as Boolean
dim hasText, hasTextControl, isTextIgnored as Boolean
dim hasOmo, hasOmoControl, isOmoIgnored as Boolean

sub OnInitParameters()
	RegisterParameterContainer("root", "Root Container (or whole scene)")
	RegisterPushButton("check", "Check the scene", 1)
	RegisterParameterText("console", "", 999, 299)
end sub

sub OnExecAction(buttonId As Integer)
	if GetParameterContainer("root") == null then
		MakeFullSceneTree(arrSceneContainers)
	else
		MakeSceneSubTree(arrSceneContainers, GetParameterContainer("root"), 0)  ' 0?
	end if
	
	console = ""
	
	for i=0 to arrSceneContainers.ubound
		name = arrSceneContainers[i].c.name
		if name.length > maxNameLength then	maxNameLength = name.length
	next
	for i=0 to arrSceneContainers.ubound
		level = arrSceneContainers[i].level
		name = arrSceneContainers[i].c.name
		
		'CONTROL IMAGE
		hasTexture = arrSceneContainers[i].c.Texture <> null
		textureMessage = ""
		if hasTexture then
			hasImageControl = arrSceneContainers[i].c.GetFunctionPluginInstance("ControlImage") <> null
			isTextureIgnored = name.Find("[I]") >= 0
			if hasImageControl then
				textureMessage = " [I ✔]"
			elseif isTextureIgnored then
				textureMessage = " [I ~]"
			else
				textureMessage = " [I ✘✘✘✘✘]"
			end if
		end if
		
		'CONTROL TEXT
		hasText = arrSceneContainers[i].c.Geometry <> null AND System.SendCommand("0 #"&arrSceneContainers[i].c.vizId&"*GEOM*OBJECT_TYPE GET") == "GEOM_TEXT"
		textMessage = ""
		if hasText then
			hasTextControl = arrSceneContainers[i].c.GetFunctionPluginInstance("ControlText") <> null
			isTextIgnored = name.Find("[T]") >= 0
			if hasTextControl then
				textMessage = " [T ✔]"
			elseif isTextIgnored then
				textMessage = " [T ~]"
			else
				textMessage = " [T ✘✘✘✘✘]"
			end if
		end if
		
		'CONTROL OMO
		hasOmo = arrSceneContainers[i].c.GetFunctionPluginInstance("Omo") <> null
		omoMessage = ""
		if hasOmo then
			hasOmoControl = arrSceneContainers[i].c.GetFunctionPluginInstance("ControlOmo") <> null
			isOmoIgnored = name.Find("[O]") >= 0
			if hasOmoControl then
				omoMessage = " [O ✔]"
			elseif isOmoIgnored then
				omoMessage = " [O ~]"
			else
				omoMessage = " [O ✘✘✘✘✘]"
			end if
		end if
		
		levelShift = ""
		for k=0 to level-1
			levelShift &= "   "
		next
		console &= levelShift & name & " — " & CStr(arrSceneContainers[i].c.Texture) & textureMessage & textMessage & omoMessage
		console &= "\n"
	next
	
	this.ScriptPluginInstance.SetParameterString("console",console)
end sub

sub MakeFullSceneTree(ByRef _out As Array[SceneContainer]) 
	dim _root = Scene.RootContainer
	
	do while _root <> null
		MakeSceneSubTree(_out, _root, 0)
		_root = _root.NextContainer
	loop
end sub

sub MakeSceneSubTree(ByRef _out As Array[SceneContainer], _cRoot as Container, _level as Integer)
	dim _sceneContainer as SceneContainer
	_sceneContainer.c = _cRoot
	_sceneContainer.level = _level
	_out.push(_sceneContainer)
	
	for i=0 to _cRoot.ChildContainerCount - 1
		MakeSceneSubTree(_out, _cRoot.GetChildContainerByIndex(i), _level + 1)
	next
end sub

