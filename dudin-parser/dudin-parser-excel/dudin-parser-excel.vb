RegisterPluginVersion(1,5,0)
Dim info As String = "Get value from Excel by DataPool Reader through SharedMemory. Author: Dmitry Dudin.
If ypu chose \"childs texts\" mode you have to name interactive child containers by template \"=X,Y\",
where X and Y - a number or name auto-counter. 
e.g.:  =1,23  =12,2  =12,24
or with auto-increments  =i,1  =y,1
Any auto-counters will be auto incremented each time.
Use different auto-counter in order to get data from several rows or columns.
For example, for the first column use \"=i,1\" and for the second \"=y,2\"

Supported types for all table:
:omo
:color
"

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
	type As String
End Structure
Dim arr_cells As Array[Cell]

Structure AutoIndex
	name As String
	index As Integer
End Structure
Dim arr_auto_row, arr_auto_column As Array[AutoIndex]

Dim arr_shm_arrays_names As Array[String]

Dim output_one_buttonNames, output_table_buttonNames, mode_buttonNames, plugin_mode_buttonNames, mode_select_mode_buttonNames, shm_mode As Array[String]
mode_buttonNames.Push("one value to this")
mode_buttonNames.Push("all table")
mode_buttonNames.Push("SHM arrays")

output_one_buttonNames.Push("console")
output_one_buttonNames.Push("text")
output_one_buttonNames.Push("plugin")

output_table_buttonNames.Push("console")
output_table_buttonNames.Push("texts (or :types)")

plugin_mode_buttonNames.Push("Bool")
plugin_mode_buttonNames.Push("Int")
plugin_mode_buttonNames.Push("Double")
plugin_mode_buttonNames.Push("String")

mode_select_mode_buttonNames.Push("By number")
mode_select_mode_buttonNames.Push("Find text")

shm_mode.Push("Scene")
shm_mode.Push("System")
shm_mode.Push("VizComm")

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterContainer("reader", "Excel Reader [this]")
	RegisterRadioButton("mode", "Mode", 0, mode_buttonNames)
	RegisterRadioButton("output_one_type", "Output to", 0, output_one_buttonNames)
	RegisterRadioButton("output_table_type", "Output to", 0, output_table_buttonNames)
	
	RegisterParameterInt("start_auto_row", "Start auto-row (e.g. =i,1)", 2, 2, 999)
	RegisterParameterInt("start_auto_column", "Start auto-column (e.g. =2,i)", 1, 1, 999)
	
	RegisterRadioButton("plugin_type", "Type", 0, plugin_mode_buttonNames)
	RegisterParameterString("plugin_name", "Plugin name (case sensitive)", "", 20, 999, "")
	RegisterParameterString("plugin_value", "Plugin value (case sensitive)", "", 20, 999, "")
	
	RegisterRadioButton("mode_row", "Select row", 0, mode_select_mode_buttonNames)
	RegisterParameterInt("row_number", "Row", 2, 2, 999)
	RegisterParameterString("row_find_text", "Search text", "", 20, 999, "")
	RegisterParameterInt("row_find_num_column", "Search in which column", 1, 1, 999)
	
	RegisterRadioButton("mode_column", "Select column", 0, mode_select_mode_buttonNames)
	RegisterParameterInt("column_number", "Column", 1, 1, 999)
	RegisterParameterString("column_find_text", "Search text", "", 20, 999, "")
	RegisterParameterInt("column_find_num_row", "Search in which row", 2, 2, 999)

	RegisterRadioButton("shm_mode", "SHM mode", 0, shm_mode)
	RegisterParameterString("shm_arrays_names", "SHM columns names [,,x,y]", "", 50, 999, "")
	RegisterParameterString("shm_delimeter", "SHM values delimeter", ";", 10, 999, "")
	
	RegisterParameterBool("ignore_empty", "Keep data if the file is blocked", true)
	RegisterPushButton("init", "Init", 1)
	RegisterPushButton("output_table", "Print table to console", 2)
	RegisterPushButton("output_shm", "Print SHM to console", 3)
end sub

sub OnInit()
	if GetParameterInt("mode") == 0 then
		' one single cell
		SendGuiParameterShow("output_one_type", SHOW)
		SendGuiParameterShow("output_table_type", HIDE)
		
		SendGuiParameterShow("mode_row", SHOW)
		SendGuiParameterShow("mode_column", SHOW)
		
		if GetParameterInt("output_one_type") == 0 OR GetParameterInt("output_one_type") == 1 then
			'output to console OR text
			SendGuiParameterShow("plugin_type", HIDE)
			SendGuiParameterShow("plugin_name", HIDE)
			SendGuiParameterShow("plugin_value", HIDE)
		elseif GetParameterInt("output_one_type") == 2 then
			'output to plugin
			SendGuiParameterShow("plugin_type", SHOW)
			SendGuiParameterShow("plugin_name", SHOW)
			SendGuiParameterShow("plugin_value", SHOW)
		end if
		
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
		
		' hide useles
		SendGuiParameterShow("start_auto_row", HIDE)
		SendGuiParameterShow("start_auto_column", HIDE)
		
		SendGuiParameterShow("shm_mode", HIDE)
		SendGuiParameterShow("shm_arrays_names", HIDE)
		SendGuiParameterShow("shm_delimeter", HIDE)
		
	elseif GetParameterInt("mode") == 1 then
		' all table
		SendGuiParameterShow("output_one_type", HIDE)
		SendGuiParameterShow("output_table_type", SHOW)
		
		SendGuiParameterShow("start_auto_row", SHOW)
		SendGuiParameterShow("start_auto_column", SHOW)
		
		if GetParameterInt("output_table_type") == 0 then
			'console
			SendGuiParameterShow("start_auto_row", HIDE)
			SendGuiParameterShow("start_auto_column", HIDE)
		elseif GetParameterInt("output_table_type") == 1 then
			'output to children texts (or :types)
			SendGuiParameterShow("start_auto_row", SHOW)
			SendGuiParameterShow("start_auto_column", SHOW)
			
			FindCellSubContainers()
		end if
		
		' hide useles
		SendGuiParameterShow("mode_row", HIDE)
		SendGuiParameterShow("mode_column", HIDE)
		SendGuiParameterShow("plugin_type", HIDE)
		SendGuiParameterShow("plugin_name", HIDE)
		SendGuiParameterShow("plugin_value", HIDE)
		SendGuiParameterShow("row_number", HIDE)
		SendGuiParameterShow("row_find_text", HIDE)
		SendGuiParameterShow("row_find_num_column", HIDE)
		SendGuiParameterShow("column_number", HIDE)
		SendGuiParameterShow("column_find_text", HIDE)
		SendGuiParameterShow("column_find_num_row", HIDE)
		
		SendGuiParameterShow("shm_mode", HIDE)
		SendGuiParameterShow("shm_arrays_names", HIDE)
		SendGuiParameterShow("shm_delimeter", HIDE)

	elseif GetParameterInt("mode") == 2 then
		' SHM arrays
		
		SendGuiParameterShow("shm_mode", SHOW)
		SendGuiParameterShow("shm_arrays_names", SHOW)
		SendGuiParameterShow("shm_delimeter", SHOW)
		

		SendGuiParameterShow("output_one_type", HIDE)
		SendGuiParameterShow("output_table_type", HIDE)
		
		SendGuiParameterShow("start_auto_row", HIDE)
		SendGuiParameterShow("start_auto_column", HIDE)

		' hide useles
		SendGuiParameterShow("mode_row", HIDE)
		SendGuiParameterShow("mode_column", HIDE)
		SendGuiParameterShow("plugin_type", HIDE)
		SendGuiParameterShow("plugin_name", HIDE)
		SendGuiParameterShow("plugin_value", HIDE)
		SendGuiParameterShow("row_number", HIDE)
		SendGuiParameterShow("row_find_text", HIDE)
		SendGuiParameterShow("row_find_num_column", HIDE)
		SendGuiParameterShow("column_number", HIDE)
		SendGuiParameterShow("column_find_text", HIDE)
		SendGuiParameterShow("column_find_num_row", HIDE)
			
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
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
	Parse()
	Output()
end sub

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	if mapKey == input_var_name then
		Parse()
		Output()
	end if
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		' Init
		OnInit()
		Parse()
		Output()
	elseif buttonId == 2 then
		' debug print all TABLE to console
		Parse()
		println(FormatAllTable())
	elseif buttonId == 3 then
		' debug print VALUE to console
		Parse()
		println(FormatAllSHM())
	end if
end sub

'----------------------------------------------------------

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
	Dim _comma_pos, _colon_pos, _equal_pos As Integer
	this.GetContainerAndSubContainers(_arr_childs, false)
	_arr_childs.Erase(0)
	arr_auto_row.Clear()
	arr_auto_column.Clear()
	for i=0 to _arr_childs.ubound
		_name = _arr_childs[i].name
		_name.Trim()
		if _name.Match("^.*=(-?\\d+|\\w+)\\,(-?\\d+|\\w+)\\:?(\\S*)$") then
			'e.g.:
			'=1,23
			'=12,2
			'=12,24
			'=i,1
			'=1,y
			'=2,an_increment
			'=12,24:omo
			'=i,y:omo
			'=i,4:color
			_cell.c = _arr_childs[i]
			
			_equal_pos = _name.Find("=")
			_comma_pos = _name.Find(",")
			_colon_pos = _name.Find(":")
			
			_row = _name.GetSubstring(_equal_pos+1, _comma_pos-_equal_pos-1)
			
			if _name.Find(":") > 0 then
				_column = _name.GetSubstring(_comma_pos+1, _colon_pos-_comma_pos-1)
				_cell.type = _name.GetSubstring(_colon_pos+1, _name.length-_colon_pos-1)
			else
				_column = _name.GetSubstring(_comma_pos+1, _name.Length-_comma_pos-1)
				_cell.type = "text"
			end if
			if _row.Match("-?\\d+") then
				_cell.row = CInt(_row)
			else
				_cell.row = GetAutoIndex(arr_auto_row, _row, GetParameterInt("start_auto_row"))
			end if
			
			
			if _column.Match("\\D+") then
				_cell.column = GetAutoIndex(arr_auto_column, _column, GetParameterInt("start_auto_column"))
			else
				_cell.column = CInt(_column)
			end if
			if (_cell.row >= 2 OR _cell.row < 0) AND _cell.column >= 1 then	arr_cells.Push(_cell)
		end if
	next
End Sub

Sub Parse()
	Dim _s As String
	select case input_var_type
	case 0
		Scene.Map.RegisterChangedCallback(input_var_name)
		_s = Scene.Map[input_var_name]
	case 1
		System.Map.RegisterChangedCallback(input_var_name)
		_s = System.Map[input_var_name]
	case 2
		VizCommunication.Map.RegisterChangedCallback(input_var_name)
		_s = VizCommunication.Map[input_var_name]
	end select

	_s.Trim()
	if _s == "" then
		if GetParameterBool("ignore_empty") then
			exit sub
		else
			arr_rows.Clear()
		end if
	end if
	_s.Split(row_delimeter, arr_rows)
	data.Clear()
	for i=0 to arr_rows.ubound
		arr_rows[i].split(field_delimeter, arr_fields)
		data.Push(arr_fields)
	next
End Sub

Dim _arr_color As Array[String]
Sub Output()
	if GetParameterInt("mode") == 0 then
		' one value
		Dim _row As Integer = GetRow()
		Dim _column As Integer = GetColumn()

		if _row >= 2 AND _column >= 1 AND data.size > 0 AND _row <= data.ubound AND _column <= data[_row-2].size then
			output = data[_row-2][_column-1]
		else
			output = ""
		end if

		select case GetParameterInt("output_one_type")
		case 0
			println(output)
		case 1
			this.Geometry.Text = output
		case 2
			'to plugin
			Dim _p As PluginInstance =  this.GetFunctionPluginInstance(GetParameterString("plugin_name"))
			select case GetParameterInt("plugin_type")
			case 0 ' Bool
				_p.SetParameterBool(GetParameterString("plugin_value"), CBool(output))
			case 1 ' Int
				_p.SetParameterInt(GetParameterString("plugin_value"), CInt(output))
			case 2 ' Double
				_p.SetParameterDouble(GetParameterString("plugin_value"), CDbl(output))
			case 3 ' String
				_p.SetParameterString(GetParameterString("plugin_value"), CStr(output))
			end select
		end select
	elseif GetParameterInt("mode") == 1 then
		' all table
		select case GetParameterInt("output_table_type")
		case 0
			println(FormatAllTable())
		case 1
			' to childs
			Dim _value As String
			Dim _row As Integer
			
			for i=0 to arr_cells.ubound
				if arr_cells[i].row > 0 then
					_row = arr_cells[i].row-2
				else
					_row = data.size + arr_cells[i].row
				end if
				
				if data.size > 0 AND _row < data.size AND arr_cells[i].column-1 < data[_row].size then
					_value = data[_row][arr_cells[i].column-1]
					select case arr_cells[i].type
					case "text"
						arr_cells[i].c.Geometry.Text = _value
					case "omo"
						arr_cells[i].c.GetFunctionPluginInstance("Omo").SetParameterInt("vis_con", CInt(_value))
					case "color"
						if _value.match("\\d+;\\d+;\\d+") then
							_value.Split(";", _arr_color)
							arr_cells[i].c.Material.Emission = CColor(  CDbl(_arr_color[0])/255.0, CDbl(_arr_color[1])/255.0, CDbl(_arr_color[2])/255.0  )  '
						end if
					end select
				end if
			next
		end select
	elseif GetParameterInt("mode") == 2 then
		' SHM arrays
		Dim arr_column_data As Array[String]
		Dim s_column_data As String
		
		GetParameterString("shm_arrays_names").Split(",", arr_shm_arrays_names)
		for i=0 to arr_shm_arrays_names.ubound
			arr_shm_arrays_names[i].Trim()
		next
		
		for i=0 to arr_shm_arrays_names.ubound
			arr_column_data.Clear()
			for j=0 to data.ubound
				arr_column_data.Push(data[j][i])
			next
			s_column_data.Join(arr_column_data, GetParameterString("shm_delimeter"))
			
			if arr_shm_arrays_names[i] <> "" then
				select case GetParameterInt("shm_mode")
				case 0
					Scene.Map[arr_shm_arrays_names[i]] = s_column_data
				case 1
					System.Map[arr_shm_arrays_names[i]] = s_column_data
				case 2
					VizCommunication.Map[arr_shm_arrays_names[i]] = s_column_data
				end select
			end if
		next
	end if
End Sub

'----------------------------------------------------------

Function FormatAllTable() As String
	Dim _max_length_number As Integer = CStr(data.ubound + 2).length
	Dim _max_lengths As Array[Integer]
	
	for i=0 to data.ubound
		for y=0 to data[i].ubound
			if y > _max_lengths.ubound then
				_max_lengths.Push(CStr(y).length)
			end if
			if data[i][y].Length > _max_lengths[y] then _max_lengths[y] = data[i][y].Length
		next
	next
	s = ""
	Dim _line As String = ""
	
	' add legend
	_line = _line & FillSpacesRight("", _max_length_number)
	for i=0 to _max_lengths.ubound
		if CStr(i+1).length > _max_lengths[i] then _max_lengths[i] = CStr(i+1).length
		_line = _line & "|" & FillSpacesRight(CStr(i+1), _max_lengths[i])
	next
	s = _line & "\n"

	for i=0 to data.ubound
		_line = FillSpacesRight(CStr(i+2), _max_length_number)
		for y=0 to data[i].ubound
			_line = _line & "|" & FillSpacesRight(data[i][y], _max_lengths[y])
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


'----------------------------------------------------------

Function FormatAllSHM() As String
	Dim _s As String
	select case GetParameterInt("shm_mode")
	case 0
		_s &= "MODE: Scene.Map"
	case 1
		_s &= "MODE: System.Map"
	case 2
		_s &= "MODE: VizCommunication.Map"
	end select
	_s &= "\n"
	for i=0 to arr_shm_arrays_names.ubound
		if arr_shm_arrays_names[i] <> "" then
			_s &= arr_shm_arrays_names[i] & " = "
			select case GetParameterInt("shm_mode")
			case 0
				_s &= Scene.Map[arr_shm_arrays_names[i]]
			case 1
				_s &= System.Map[arr_shm_arrays_names[i]]
			case 2
				_s &= VizCommunication.Map[arr_shm_arrays_names[i]]
			end select
			_s &= "\n"
		end if
	next
	FormatAllSHM = _s
End Function
