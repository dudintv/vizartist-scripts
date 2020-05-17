RegisterPluginVersion(1,0,0)

Dim c As Container
Dim p As PluginInstance
Dim s_plugin, s_param, s_prefix As String

Dim buttonPlugins As Array[String]
buttonPlugins.Push("Script")
buttonPlugins.Push("Geom")
buttonPlugins.Push("Other Plugin")
Dim buttonTypes As Array[String]
buttonTypes.Push("Bool")
buttonTypes.Push("Int")
buttonTypes.Push("Double")
buttonTypes.Push("String")
buttonTypes.Push("Container")
buttonTypes.Push("Color")
sub OnInitParameters()
	RegisterParameterContainer("c", "Container")
	RegisterParameterString("prefix", "Prefix", "", 40, 999, "")
	RegisterRadioButton("plugin", "Plugin", 0, buttonPlugins)
	RegisterRadioButton("type", "Param Type", 0, buttonTypes)
	RegisterParameterString("plugin_name", "Plugin Name", "", 40, 999, "")
	RegisterParameterString("param_name", "Param Name", "", 40, 999, "")
end sub

sub OnInit()
	SendGuiParameterShow("plugin_name", CInt(GetParameterInt("plugin") == 2))
	
	c = GetParameterContainer("c")
	
	s_plugin = GetParameterString("plugin_name")
	s_param = GetParameterString("param_name")
	s_prefix = GetParameterString("prefix")
	
	Select case GetParameterInt("plugin")
	Case 0
		'Script
		p = c.ScriptPluginInstance
	Case 1
		'Geom
		p = c.GetGeometryPluginInstance()
	Case 2
		'Other plugin
		p = c.GetFunctionPluginInstance(s_plugin)
	End Select
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

Dim result As String
sub OnExecPerField()
	if c <> null then
		Select Case GetParameterInt("type")
		Case 0
			'Bool
			result = CStr(p.GetParameterBool(s_param))
		Case 1
			'Int
			result = CStr(p.GetParameterInt(s_param))
		Case 2
			'Double
			result = DoubleToString(p.GetParameterDouble(s_param),1)
		Case 3
			'String
			result = p.GetParameterString(s_param)
		Case 4
			'Container
			result = p.GetParameterContainer(s_param).name
		Case 5
			'Color
			result = CStr(p.GetParameterColor(s_param))
		End Select
		
		this.Geometry.Text = s_prefix & result
	end if
end sub
