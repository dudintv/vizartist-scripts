RegisterPluginVersion(1,0,0)
Dim info As String = "
Omo for switching certain plugins in child containers
Developer: Dmitry Dudin.
"

Dim arr_child As Array[Container]
Dim arr_plugin As Array[PluginInstance]

sub OnInitParameters()
	RegisterParameterString("plugin", "Plugin name", "", 40, 999, "")
	RegisterParameterInt("num", "Visible number", 0, 0, 999)
end sub

sub OnInit()
	GetFirstLevelChilds(this, arr_child)
end sub

sub OnParameterChanged(parameterName As String)
	if parameterName == "plugin" then
		GetPlugins()
	elseif parameterName == "num" then
		for i=0 to arr_plugin.ubound
			arr_plugin[i].Active = ( GetParameterInt("num") == i )
		next
	end if
end sub

Sub GetPlugins()
	arr_plugin.Clear()
	for i=0 to arr_child.ubound
		arr_plugin.Push( arr_child[i].GetFunctionPluginInstance(GetParameterString("plugin")) )
	next
End Sub

Sub GetFirstLevelChilds(root as Container, ByRef _arr_childs As Array[Container])
	_arr_childs.Clear()
	Dim _c As Container
	_c = this.FirstChildContainer
	Do 
		_arr_childs.Push(_c)
		_c = _c.NextContainer
	Loop While _c <> null
End Sub
