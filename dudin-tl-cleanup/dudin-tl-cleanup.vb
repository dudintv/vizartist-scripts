RegisterPluginVersion(1,0,0)
Dim info As String = "Developer: Dmitry Dudin
http://dudin.tv/scripts/tl-cleanup
"

Dim arrP As Array[PluginInstance]
Dim arrC As Array[Container]
Dim arrD As Array[Director]
Dim d As Director
Dim c As Container
Dim pToggle As PluginInstance

sub OnInitPlugin()
    RegisterPlugin("TL-cleanup")
    RegisterPluginDisplayName("Transition Logic Cleanup")
    RegisterPluginFolder("DudinPlugins")
    RegisterPluginType(PLUGIN_TYPE_FUNCTION)
end sub

sub OnInitParameters()
	RegisterPushButton("clear", "Clear geometries in all TL layers", 1)
	RegisterPushButton("zero", "Set all directors to zero", 2)
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		ResetLayers()
	elseif buttonId == 2 then
		ResetDirectors()
	end if
end sub


Sub ResetLayers()
	arrC.Clear()
	arrP.Clear()
	c = Scene.RootContainer
	c.GetContainerAndSubContainers(arrC, false)
	for i=0 to arrC.ubound
		pToggle = arrC[i].GetFunctionPluginInstance("Toggle")
		if pToggle <> null then
			arrP.Push(pToggle)
		end if
	next
	for i=0 to arrP.ubound
		arrP[i].PushButton("delete_cached")
	next
End Sub

Sub ResetDirectors()
	d = Stage.RootDirector
	do while d <> null
		d.StopAnimation()
		d.Show(0)
		d = d.NextDirector
	loop
End Sub
