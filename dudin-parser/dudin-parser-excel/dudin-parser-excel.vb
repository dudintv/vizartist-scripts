RegisterPluginVersion(1,2,0)
Dim info As String = "Get value from Excel by DataPool Reader through SharedMemory. Author: Dmitry Dudin.
If ypu chose \"childs texts\" mode you have to name interactive child containers by template \"=X,Y\",
where X and Y - a number or name auto-counter. 
e.g.:  =1,23  =12,2  =12,24  =i,1  =y,1  =2,i  =2,y
Any auto-counters will be auto incremented.
Use different auto-counter in order to get data from several rows or columns.
For example, for the first column use \"=i,1\" and for the second \"=y,2\""

Dim c_reader As Container
Dim input_var_name, row_delimeter, field_delimeter As String
Dim input_var_type As Integer '0=Scene, 1=System, 2=VizComm
Dim s, output As String
Dim arr_rows, arr_fields As Array[String]
Dim data As Array[Array[String]]

Structure Cell
	c As Container
	row As Integer
	column As Integer
End Structure
Dim arr_cells As Array[Cell]
Structure AutoIndex
	name As String
	index As Integer
End Structure
Dim arr_auto_row, arr_auto_column As Array[AutoIndex]

Dim output_buttonNames, mode_buttonNames, plugin_mode_buttonNames, mode_select_mode_buttonNames As Array[String]
output_buttonNames.Push("println")
output_buttonNames.Push("this text")
output_buttonNames.Push("this plugin")
output_buttonNames.Push("childs texts")
mode_buttonNames.Push("one value")
mode_buttonNames.Push("all table")
plugin_mode_buttonNames.Push("Bool")
plugin_mode_buttonNames.Push("Int")
plugin_mode_buttonNames.Push("Double")
plugin_mode_buttonNames.Push("String")
mode_select_mode_buttonNames.Push("By number")
mode_select_mode_buttonNames.Push("Find text")
sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterContainer("reader", "Excel Reader (default is this)")
	RegisterRadioButton("output_type", "Output to", 0, output_buttonNames)
	RegisterParameterInt("start_auto_row", "Start auto-row (e.g. =i,1)", 2, 2, 999)
	RegisterParameterInt("start_auto_column", "Start auto-column (e.g. =2,i)", 1, 1, 999)
	RegisterRadioButton("plugin_type", "Type", 0, plugin_mode_buttonNames)
	RegisterParameterString("plugin_name", "Plugin name (case sensitive)", "", 20, 999, "")
	RegisterParameterString("plugin_value", "Plugin value (case sensitive)", "", 20, 999, "")
	RegisterRadioButton("mode", "Mode", 0, mode_buttonNames)
	RegisterRadioButton("mode_row", "Select row", 0, mode_select_mode_buttonNames)
	RegisterParameterInt("row_number", "Row", 2, 2, 999)
	RegisterParameterString("row_find_text", "Search text", "", 20, 999, "")
	RegisterParameterInt("row_find_num_column", "Search in which column", 1, 1, 999)
	RegisterRadioButton("mode_column", "Select column", 0, mode_select_mode_buttonNames)
	RegisterParameterInt("column_number", "Column", 1, 1, 999)
	RegisterParameterString("column_find_text", "Search text", "", 20, 999, "")
	RegisterParameterInt("column_find_num_row", "Search in which row", 2, 2, 999)
	RegisterParameterBool("ignore_empty", "Keep data if the file is blocked", true)
	RegisterParameterBool("print_legends", "Print numbers rows and columns", true)
	RegisterPushButton("init", "Init", 1)
	RegisterPushButton("output_now", "Output to console now", 2)
end sub

sub OnInit()
	if GetParameterInt("mode") == 0 AND GetParameterInt("output_type") <> 3 then
		'one value
		SendGuiParameterShow("row_number", SHOW)
		SendGuiParameterShow("column_number", SHOW)
		
		SendGuiParameterShow("mode_row", SHOW)
		SendGuiParameterShow("mode_column", SHOW)
		
		if GetParameterInt("mode_row") == 0 then
			SendGuiParameterShow("row_number", SHOW)
			SendGuiParameterShow("row_find_text", HIDE)
			SendGuiParameterShow("row_find_num_column", HIDE)
		else
			SendGuiParameterShow("row_number", HIDE)
			SendGuiParameterShow("row_find_text", SHOW)
			SendGuiParameterShow("row_find_num_column", SHOW)
		end if
		
		if GetParameterInt("mode_column") == 0 then
			SendGuiParameterShow("column_number", SHOW)
			SendGuiParameterShow("column_find_text", HIDE)
			SendGuiParameterShow("column_find_num_row", HIDE)
		else
			SendGuiParameterShow("column_number", HIDE)
			SendGuiParameterShow("column_find_text", SHOW)
			SendGuiParameterShow("column_find_num_row", SHOW)
		end if
	elseif GetParameterInt("mode") == 1 OR GetParameterInt("output_type") == 3 then
		'all table
		SendGuiParameterShow("row_number", HIDE)
		SendGuiParameterShow("column_number", HIDE)
		
		SendGuiParameterShow("mode_row", HIDE)
		SendGuiParameterShow("row_number", HIDE)
		SendGuiParameterShow("row_find_text", HIDE)
		SendGuiParameterShow("row_find_num_column", HIDE)
		
		SendGuiParameterShow("mode_column", HIDE)
		SendGuiParameterShow("column_number", HIDE)
		SendGuiParameterShow("column_find_text", HIDE)
		SendGuiParameterShow("column_find_num_row", HIDE)
	end if
	
	if GetParameterInt("output_type") == 2 then
		'output to plugin
		SendGuiParameterShow("start_auto_row", HIDE)
		SendGuiParameterShow("start_auto_column", HIDE)
		
		SendGuiParameterShow("plugin_type", SHOW)
		SendGuiParameterShow("plugin_name", SHOW)
		SendGuiParameterShow("plugin_value", SHOW)
		
		SendGuiParameterShow("mode", SHOW)
	elseif GetParameterInt("output_type") == 3 then
		'output to childs
		SendGuiParameterShow("start_auto_row", SHOW)
		SendGuiParameterShow("start_auto_column", SHOW)
		
		SendGuiParameterShow("plugin_type", HIDE)
		SendGuiParameterShow("plugin_name", HIDE)
		SendGuiParameterShow("plugin_value", HIDE)
		
		SendGuiParameterShow("mode", HIDE)
	else
		'output to console OR this.text
		SendGuiParameterShow("start_auto_row", HIDE)
		SendGuiParameterShow("start_auto_column", HIDE)
		
		SendGuiParameterShow("plugin_type", HIDE)
		SendGuiParameterShow("plugin_name", HIDE)
		SendGuiParameterShow("plugin_value", HIDE)
		
		SendGuiParameterShow("mode", SHOW)
	end if
	
	if GetParameterInt("output_type") == 3 then
		'output to childs
		FindCellSubContainers()
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
	Output(GetParameterInt("output_type"), GetParameterInt("mode"))
end sub

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	if mapKey == input_var_name then
		Parse(map, mapKey)
		Output(GetParameterInt("output_type"), GetParameterInt("mode"))
	end if
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		'Init
		OnInit()
		Output(GetParameterInt("output_type"), GetParameterInt("mode"))
	elseif buttonId == 2 then
		'debug print in console
		if GetParameterInt("output_type") == 3 then
			Output(0, 1)
			'0 - output to console, '1 - "all table" mode
		else
			Output(0, GetParameterInt("mode"))
			'0 - outout to console
		end if
	end if
end sub

'-----------------------------------------------------------------------

Function GetAutoIndex(_arr As Array[AutoIndex], _name As String, _default_start_index As Integer) As Integer
	Dim _result As Integer = -1
	for i=0 to _arr.ubound
		if _arr[i].name == _name then
			_arr[i].index += 1
			_result = _arr[i].index
		end if
	next
	if _result == -1 then
		Dim _auto_index As AutoIndex
		_auto_index.name = _name
		_auto_index.index = _default_start_index
		_arr.Push(_auto_index)
		_result = _default_start_index
	end if
	GetAutoIndex = _result
End Function

Function GetRow() As Integer
	if GetParameterInt("mode_row") == 0 then
		'static number
		GetRow = GetParameterInt("row_number")
	else
		'find
		Dim _index As Integer = -1
		Dim _search As String = GetParameterString("row_find_text")
		_search.Trim()
		if _search <> "" then
			for i=0 to data.ubound
				if data[i][GetParameterInt("row_find_num_column")-1] == _search then _index = i
			next
		end if
		GetRow = _index+2
	end if
End Function

Function GetColumn() As Integer
	if GetParameterInt("mode_column") == 0 then
		'static number
		GetColumn = GetParameterInt("column_number")
	else
		'find
		Dim _index As Integer = -1
		Dim _search As String = GetParameterString("column_find_text")
		_search.Trim()
		if _search <> "" then
			for i=0 to data[GetParameterInt("column_find_num_row")-2].ubound
				if data[GetParameterInt("column_find_num_row")-2][i] == _search then _index = i
			next
		end if
		GetColumn = _index+1
	end if
End Function

Sub FindCellSubContainers()
	arr_cells.Clear()
	Dim _cell As Cell
	Dim _arr_childs As Array[Container]
	Dim _name, _row, _column As String
	this.GetContainerAndSubContainers(_arr_childs, false)
	_arr_childs.Erase(0)
	arr_auto_row.Clear()
	arr_auto_column.Clear()
	for i=0 to _arr_childs.ubound
		_name = _arr_childs[i].name
		_name.Trim()
		if _name.Match("^=(\\d+|\\w+)\\,(\\d+|\\w+)$") then
			'e.g.:
			'=1,23
			'=12,2
			'=12,24
			'=i,1
			'=y,1
			'=2,i
			_cell.c = _arr_childs[i]
			_row = _name.GetSubstring(1, _name.Find(",")-1)
			_column = _name.GetSubstring(_name.Find(",")+1, _name.Length-_name.Find(",")-1)
			if _row.Match("\\D+") then
				_cell.row = GetAutoIndex(arr_auto_row, _row, GetParameterInt("start_auto_row"))
			else
				_cell.row = CInt(_row)
			end if
			if _column.Match("\\D+") then
				_cell.column = GetAutoIndex(arr_auto_column, _column, GetParameterInt("start_auto_column"))
			else
				_cell.column = CInt(_column)
			end if
			if _cell.row >= 2 AND _cell.column >= 1 then	arr_cells.Push(_cell)
		end if
	next
End Sub

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

Sub Output(_output_to As Integer, _mode As Integer)
	Dim _row As Integer = GetRow()
	Dim _column As Integer = GetColumn()
	if _row >= 2 AND _column >= 1 AND data.size > 0 AND _row-1 <= data.size AND _column <= data[_row-2].size then
		if _mode == 0 then
			'one value
			output = data[_row-2][_column-1]
		elseif _mode == 1 then
			'all table
			output = FormatAllTable()
		end if
	else
		output = ""
	end if
	
	if _output_to == 0 then
		println(output)
	elseif _output_to == 1 then
		this.Geometry.Text = output
	elseif _output_to == 2 then
		'to plugin
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
	elseif _output_to == 3 then
		'to childs
		for i=0 to arr_cells.ubound
			if arr_cells[i].row-2 < data.size AND arr_cells[i].column-1 < data[arr_cells[i].row-2].size then
				arr_cells[i].c.Geometry.Text = data[arr_cells[i].row-2][arr_cells[i].column-1]
			end if
		next
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
	Dim _line As String = ""
	if GetParameterBool("print_legends") then
		_line = _line & FillSpacesRight("", CStr(data.size).Length)
		for i=0 to max_lengths.ubound
			_line = _line & "|" & FillSpacesRight(CStr(i+1), max_lengths[i])
		next
		s = _line & "\n"
	end if
	for i=0 to data.ubound
		_line = ""
		if GetParameterBool("print_legends") then _line = _line & FillSpacesRight(CStr(i+2), CStr(data.size).Length)
		for y=0 to data[i].ubound
			_line = _line & "|" & FillSpacesRight(data[i][y], max_lengths[y])
		next
		s = s & _line & "\n"
	next
	FormatAllTable = s
End Function

Function FillSpacesRight(_s As String, _length As Integer) As String
	do while _s.Length < _length
		_s = _s & " "
	loop
	FillSpacesRight = _s
End Function
