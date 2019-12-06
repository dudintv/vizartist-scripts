RegisterPluginVersion(1,0,0)
Dim info As String = "Get value from Excel by DataPool Reader through SharedMemory. Author: Dmitry Dudin."

Dim c_reader As Container
Dim input_var_name, row_delimeter, field_delimeter As String
Dim input_var_type As Integer '0=Scene, 1=System, 2=VizComm
Dim s, output As String
Dim arr_rows, arr_fields As Array[String]
Dim data As Array[Array[String]]

Dim output_buttonNames, mode_buttonNames, plugin_mode_buttonNames As Array[String]
output_buttonNames.Push("println")
output_buttonNames.Push("this text")
output_buttonNames.Push("this plugin")
mode_buttonNames.Push("one value")
mode_buttonNames.Push("all table")
plugin_mode_buttonNames.Push("Bool")
plugin_mode_buttonNames.Push("Int")
plugin_mode_buttonNames.Push("Double")
plugin_mode_buttonNames.Push("String")
sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterContainer("reader", "Excel Reader (this)")
	RegisterRadioButton("output_type", "Output to", 0, output_buttonNames)
	RegisterRadioButton("plugin_type", "Type", 0, plugin_mode_buttonNames)
	RegisterParameterString("plugin_name", "Plugin name (case sensitive)", "", 20, 999, "")
	RegisterParameterString("plugin_value", "Plugin value (case sensitive)", "", 20, 999, "")
	RegisterRadioButton("mode", "Mode", 0, mode_buttonNames)
	RegisterParameterInt("row", "Row", 2, 2, 999)
	RegisterParameterInt("column", "Column", 1, 1, 999)
	RegisterParameterBool("ignore_empty", "Keep data if the file is blocked", true)
	RegisterPushButton("init", "Init", 1)
	RegisterPushButton("output_now", "Output to console now", 2)
end sub

sub OnInit()
	if GetParameterInt("mode") == 0 then
		'one value
		SendGuiParameterShow("row", SHOW)
		SendGuiParameterShow("column", SHOW)
	elseif GetParameterInt("mode") == 1 then
		'all table
		SendGuiParameterShow("row", HIDE)
		SendGuiParameterShow("column", HIDE)
	end if
	
	if GetParameterInt("output_type") == 2 then
		'one value
		SendGuiParameterShow("plugin_type", SHOW)
		SendGuiParameterShow("plugin_name", SHOW)
		SendGuiParameterShow("plugin_value", SHOW)
	else
		'all table
		SendGuiParameterShow("plugin_type", HIDE)
		SendGuiParameterShow("plugin_name", HIDE)
		SendGuiParameterShow("plugin_value", HIDE)
	end if
	
	if GetParameterContainer("reader") <> null then
		c_reader = GetParameterContainer("reader")
	else
		c_reader = this
	end if
	input_var_name  = c_reader.GetFunctionPluginInstance("DataReader").GetParameterString("shmvar")
	input_var_type  = c_reader.GetFunctionPluginInstance("DataReader").GetParameterInt("shmtype")
	field_delimeter = c_reader.GetFunctionPluginInstance("DataReader").GetParameterString("delimiter")
	row_delimeter   = c_reader.GetFunctionPluginInstance("DataReader").GetParameterString("rowsDelimiter")
	if input_var_type == 0 then
		Scene.Map.RegisterChangedCallback(input_var_name)
		Parse(Scene.Map, input_var_name)
	elseif input_var_type == 1 then
		System.Map.RegisterChangedCallback(input_var_name)
		Parse(System.Map, input_var_name)
	elseif input_var_type == 2 then
		VizCommunication.Map.RegisterChangedCallback(input_var_name)
		Parse(VizCommunication.Map, input_var_name)
	end if
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
	Output(GetParameterInt("output_type"))
end sub

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	if mapKey == input_var_name then
		Parse(map, mapKey)
		Output(GetParameterInt("output_type"))
	end if
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		OnInit()
		Output(GetParameterInt("output_type"))
	elseif buttonId == 2 then
		Output(0)
	end if
end sub

'-----------------------------------------------------------------------

Sub Parse(map As SharedMemory, mapKey As String)
	s = map[mapKey]
	s.Trim()
	if s == "" then
		if GetParameterBool("ignore_empty") then
			exit sub
		else
			arr_rows.Clear()
		end if	
	end if
	s.Split(row_delimeter, arr_rows)
	data.Clear()
	for i=0 to arr_rows.ubound
		arr_rows[i].split(field_delimeter, arr_fields)
		data.Push(arr_fields)
	next
End Sub

Sub Output(output_to As Integer)
	if data.size > 0 AND GetParameterInt("row")-1 <= data.size AND GetParameterInt("column") <= data[GetParameterInt("row")-2].size then
		if GetParameterInt("mode") == 0 then
			'one value
			output = data[GetParameterInt("row")-2][GetParameterInt("column")-1]
		elseif GetParameterInt("mode") == 1 then
			'all table
			output = FormatAllTable()
		end if
	else
		output = ""
	end if
	
	if output_to == 0 then
		println(output)
	elseif output_to == 1 then
		this.Geometry.Text = output
	elseif output_to == 2 then
		Dim _p As PluginInstance =  this.GetFunctionPluginInstance(GetParameterString("plugin_name"))
		if GetParameterInt("plugin_type") == 0 then
			'Bool
			_p.SetParameterBool(GetParameterString("plugin_value"), CBool(output))
		elseif GetParameterInt("plugin_type") == 1 then
			'Int
			_p.SetParameterInt(GetParameterString("plugin_value"), CInt(output))
		elseif GetParameterInt("plugin_type") == 2 then
			'Double
			_p.SetParameterDouble(GetParameterString("plugin_value"), CDbl(output))
		elseif GetParameterDouble("plugin_type") == 3 then
			'String
			_p.SetParameterString(GetParameterString("plugin_value"), CStr(output))
		end if
	end if
End Sub

Function FormatAllTable() As String
	Dim max_lengths As Array[Integer]
	for i=0 to data.ubound
		for y=0 to data[i].ubound
			if y > max_lengths.ubound then
				max_lengths.Push(data[i][y].Length)
			else
				if data[i][y].Length > max_lengths[y] then max_lengths[y] = data[i][y].Length
			end if
		next
	next
	s = ""
	for i=0 to data.ubound
		Dim line As String = ""
		for y=0 to data[i].ubound
			line = line & FillSpacesRight(data[i][y], max_lengths[y]) & "|"
		next
		line.Erase(line.Length-1,1)
		s = s & line & "\n"
	next
	s.Trim()
	FormatAllTable = s
End Function

Function FillSpacesRight(_s As String, _length As Integer) As String
	do while _s.Length < _length
		_s = _s & " "
	loop
	FillSpacesRight = _s
End Function
