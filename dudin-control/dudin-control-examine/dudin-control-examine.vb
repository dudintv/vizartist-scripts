RegisterPluginVersion(1,0,0)

Structure SceneContainer
    c As Container
    level As Integer
End Structure

dim arrc as Array[Container]
dim name, console as String
dim arrSceneContainers as Array[SceneContainer]
dim maxNameLength, level as Integer
dim hasTexture as Boolean

sub OnInitParameters()
	RegisterParameterContainer("root", "Root Container (or whole scene)")
	RegisterPushButton("check", "Check the scene", 1)
	RegisterParameterText("console", "", 999, 999)
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
		hasTexture = arrSceneContainers[i].c.Texture <> null
		console &= level & " | " & name & "|" & CStr(arrSceneContainers[i].c.Texture) & hasTexture
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

