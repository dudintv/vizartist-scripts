Structure JSONObject
	type As String
	name As String
	raw_value As String
	value As Variant
	arr_objects As Array[JSONObject]
End Structure

Dim json_object As JSONObject
Dim s_json_source, s_json_path, s_output As String

sub OnInitParameters()
	RegisterParameterString("shm_input_name", "SHM input name", "", 40, 999, "")
	RegisterParameterBool("test_input_mode", "Test input mode", false)
	RegisterParameterText("test_json", "{}", 40, 40)
	RegisterParameterString("json_path", "JSON path", "", 40, 999, "")
	RegisterParameterString("shm_output_name", "SHM output name", "", 40, 999, "")
	RegisterParameterBool("test_output_mode", "Test output mode (to text)", false)
	RegisterPushButton("parse", "Parse", 1)
	RegisterPushButton("run_test", "Run Tests", 99)
end sub


sub OnInit()
	System.Map.RegisterChangedCallback(GetParameterString("shm_input_name"))
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
	if GetParameterBool("test_input_mode") Then
		SendGuiParameterShow("shm_input_name", HIDE)
		SendGuiParameterShow("test_json", SHOW)
	Else
		SendGuiParameterShow("shm_input_name", SHOW)
		SendGuiParameterShow("test_json", HIDE)
	end If
	
	if GetParameterBool("test_output_mode") Then
		SendGuiParameterShow("shm_output_name", HIDE)
	Else
		SendGuiParameterShow("shm_output_name", SHOW)
	end if
end sub


sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		' PARSE
		println("")
		println("------------------" &  GetCurrentTime().ToString())
		if GetParameterBool("test_input_mode") Then
			s_json_source = GetParameterString("test_json")
		Else
			s_json_source = System.Map[GetParameterString("shm_input_name")]
		end if
		s_json_source.Trim()
		s_json_path = GetParameterString("json_path")
		s_json_path.Trim()
		
		
		
		println("PrimitiveValidateJSON = " & PrimitiveValidateJSON(s_json_source))
		'Println("s_json_source = " & s_json_source)
		s_json_source = MinifyJSON(s_json_source)
		Println("minified s_json_source = " & s_json_source)
		
		
		json_object = ParseJSON(s_json_source)
		PrintObject(json_object, 0)
		
		s_output.join( GetValueByPath(json_object, s_json_path), " | ")
		println(1, "s_output = " & s_output)
		
		
		if GetParameterBool("test_output_mode") Then
			this.Geometry.Text = s_output
		Else
			System.Map[GetParameterString("shm_output_name")] = s_output
		end If
	ElseIf buttonId == 99 Then
		RunAllTests()
	end If
end sub







Function PrimitiveValidateJSON(ByVal json_input As String) As Boolean
	Dim curly_braces_count As Integer = 0
	Dim quote_count As Integer = 0
	Dim escape As Boolean
	Dim char As String
	json_input.Substitute("\s", "", true)
	'Println("START validation. json_input = "&json_input&" AND length = " & json_input.Length)
	for i=0 to json_input.Length-1
		char = json_input.GetChar(i)
		
		if quote_count > 0 then
			if NOT escape AND char == "\"" Then quote_count -= 1
			escape = (NOT escape AND char == "\\")
		Else
			' quote_count == 0
			if char == "\\" Then quote_count += 1
			
			if char == "{" then curly_braces_count += 1
			if char == "}" then curly_braces_count -= 1
		end if
		'Println(8, "CHAR STEP. char = "&char&" AND curly_braces_count = "& curly_braces_count & " AND quote_count = " & quote_count)
	next
	PrimitiveValidateJSON = (curly_braces_count == 0 AND quote_count == 0)	
End Function 



Function MinifyJSON(ByVal _json_input As String) As String
	_json_input.Trim()
	Dim _char As String
	Dim _escape As Boolean
	Dim _quote_count As Integer = 0
	Dim _json_result As String = ""
	
	for i=0 to _json_input.length-1
		_char = _json_input.GetChar(i)
		
		if _quote_count > 0 then
			if NOT _escape AND _char == "\"" Then _quote_count -= 1
			_escape = (NOT _escape AND _char == "\\")
			
			_json_result.Append(_char)
		Else
			' quote_count == 0
			if _char == "\"" Then _quote_count += 1
			
			if _char.Match("\\S") then _json_result.Append(_char)
		end if
	next
	MinifyJSON = _json_result
End Function



Function SplitJSON(_input As String, _separator As String) As Array[String]
	Dim _char As String
	Dim _escape As Boolean
	Dim _quote_count As Integer = 0
	Dim _curly_braces_count As Integer = 0
	Dim _square_braces_count As Integer = 0
	Dim _arr_s As Array[String]
	Dim _current_part As String = ""
	
	for i=0 to _input.Length-1
		_char = _input.GetChar(i)
		
		if _quote_count > 0 then
			if NOT _escape AND _char == "\"" Then _quote_count -= 1
			_escape = (NOT _escape AND _char == "\\")
		else
			' quote_count == 0
			if _char == "\"" Then _quote_count += 1
			
			if _char == "{" then _curly_braces_count += 1
			if _char == "}" then _curly_braces_count -= 1
			
			if _char == "[" then _square_braces_count += 1
			if _char == "]" then _square_braces_count -= 1
		end if
		
		if _curly_braces_count == 0 AND _quote_count == 0 AND _square_braces_count == 0 AND _char == _separator then
			_arr_s.Push(_current_part)
			_current_part = ""
		else
			_current_part.Append(_char)
		end if
		
		if i == _input.Length-1 then _arr_s.Push(_current_part)
	next
	SplitJSON = _arr_s
End Function




Function ParseJSON(ByVal _json_input As String) As JSONObject
  Dim _arr_s As Array[String]
	Dim _object As JSONObject
	Dim _inner_object AS JSONObject
	_json_input.Trim()

	' EXTRACT NAME:
	' search the NAME with REGEXP ^".+"\s*:\s*
	if _json_input.Match("^\\\".+\\\"\\s*:\\s*") then
		Dim _char As String
		Dim _colon, _open_quote, _close_quote As Integer
		Dim _espace As Boolean
		_open_quote = 0
		for i=1 to _json_input.length-1
			_char = _json_input.GetChar(i)
			if NOT _espace AND _char == "\"" then
				'" - disble iditiotizm of color highlighter
				_close_quote = i
			end if
			if _close_quote > 0 and _char == ":" then
				_colon = i
				exit for
			end if
		next
		_object.name = _json_input.GetSubstring(_open_quote+1, _close_quote - _open_quote-1)
		_json_input = _json_input.Right(_json_input.length - _colon - 1)
	end if
	_json_input.Trim()
	
	if _json_input == "" then
		_object.type = "empty"
		ParseJSON = _object
		Exit Function 
	end if

	' FIND OUT THE TYPE:
	_object.type = "unknown"
	
	Select Case _json_input
	' OBJECT REGEXP ^{[\s\S]*}$
	Case Is.Match("^\\{[\\s\\S]*\\}$")
		_object.type = "object"
	' ARRAY REGEXP  ^\[[\s\S]*\]$
	Case Is.Match("^\\[[\\s\\S]*\\]$")
		_object.type = "array"
	' STRING REGEXP ^"[\s\S]*"$
	Case Is.Match("^\\\"[\\s\\S]*\\\"$")
		_object.type = "string"
	' NUMBER REGEXP ^[\d\.\,]*$
	Case Is.Match("^[\\d\\.\\,]*$")
		_object.type = "number"
	' BOOLEAN REGEXP ^(true|false)$
	Case Is.Match("^(true|false)$")
		_object.type = "boolean"
	End Select
	'println(4, "current parsing value = " & _json_input & " #"&_object.type&"")

	if _object.type == "unknown" then
		ParseJSON = _object
		Exit Function 
	end if

	' EXTRACT RAW_VALUE and VALUE:
	_object.raw_value = _json_input
	Select Case _object.type
	Case "object"
		' remove "{" and "}"
		_json_input = _json_input.GetSubstring(1, _json_input.length-2)
		'println(8, "OBJECT: _json_input = " & _json_input)
		_arr_s = SplitJSON(_json_input, ",")
		'println(8, "OBJECT: _arr_s  = " & CStr(_arr_s))
		for i=0 to _arr_s.ubound
			_inner_object = ParseJSON(_arr_s[i])
			if _inner_object.type <> "empty" then _object.arr_objects.Push(_inner_object)
		next
		_object.value = _json_input
	Case "array"
		' remove "[" and "]"
		_json_input = _json_input.GetSubstring(1, _json_input.length-2)
		'println(8, "ARRAY: _json_input = " & _json_input)
		_arr_s = SplitJSON(_json_input, ",")
		'println(8, "ARRAY: _arr_s  = " & CStr(_arr_s))
		for i=0 to _arr_s.ubound
			'println(8, "_arr_s[i] = " & _arr_s[i])
			_inner_object = ParseJSON(_arr_s[i])
			if _inner_object.type <> "empty" then _object.arr_objects.Push(_inner_object)
		next
		_object.value = _json_input
	Case "string"
		' remove " and "
		_object.value = _json_input.GetSubstring(1, _json_input.length-2)
	Case "number"
		_json_input.Substitute(",", ".", false)
		if _json_input.Match("^\\d*\\.\\d*$") then
			_object.value = CDbl(_json_input)
		else
			_object.value = CInt(_json_input)
		end if
	Case "boolean"
		_object.value = (_json_input == "true")
	Case Else
		_object.value = _json_input
	End Select
	ParseJSON = _object
End Function




Function GetValueByPath(_input_object As JSONObject, _json_path As String) As Variant
	Dim _arr_path As Array[String]
	Dim _is_wild_output As Boolean
	Dim _wild_path_index As Integer
	
	
	_json_path.Split(".", _arr_path)
	for i=0 to _arr_path.ubound
		if _arr_path[i].Match("\\[\\]$") then
			_arr_path[i] = _arr_path[i].Left(_arr_path[i].length-2)
			_wild_path_index = i
			_is_wild_output = true
			exit for
		end if
	next
	
	if _is_wild_output then
		Dim _arr_temp_path As Array[String]
		for i=0 to _wild_path_index
			_arr_temp_path.Push(_arr_path[i])
		next
		
		Dim _wild_name As String
		_wild_name.Join(_arr_temp_path, ".")
		
		_arr_temp_path.Clear()
		for i=_wild_path_index+1 to _arr_path.ubound
			_arr_temp_path.Push(_arr_path[i])
		next
		Dim _unwild_name As String
		_unwild_name.Join(_arr_temp_path, ".")
		
		Dim _wild_object As JSONObject = GetObjectByPath(_input_object, _wild_name)
		Dim _arr_output As Array[Variant]
		for i=0 to _wild_object.arr_objects.ubound
			_arr_output.Push( GetValueByPath(_wild_object.arr_objects[i], _unwild_name) )
		next
		GetValueByPath = _arr_output
	elseif _arr_path[_arr_path.ubound] == "count()" then

		_arr_path.Erase(_arr_path.ubound)
		Dim _pre_function_path As String
		_pre_function_path.Join(_arr_path,".")
		
		println(1, "PRE-COUNT OBJECT:")
		PrintObject(GetObjectByPath(_input_object, _pre_function_path), 0)
		GetValueByPath = GetObjectByPath(_input_object, _pre_function_path).arr_objects.size
	else
		GetValueByPath = GetObjectByPath(_input_object, _json_path).value
	end if
End Function



Function GetObjectByPath(_input_object As JSONObject, _json_path As String) As JSONObject
	Dim _arr_path As Array[String]
	Dim _current_name As String
	Dim _is_array As Boolean
	Dim _current_array_index, _open_bracket, _close_bracket As Integer
	Dim _current_object As JSONObject = _input_object

	_json_path.Split(".", _arr_path)
	for i=0 to _arr_path.ubound
		_current_name = _arr_path[i]
		_current_name.Trim()
		if _current_name.Match("\\[\\d*\\]$") then
			_open_bracket = _current_name.FindFirstOf("[")
			_close_bracket = _current_name.FindLastOf("]")
			_is_array = true
			_current_array_index = CInt(_current_name.GetSubstring(_open_bracket+1, _close_bracket - _open_bracket-1))
			_current_name = _current_name.Left(_open_bracket)
		else
			_is_array = false
		end if
		
		_current_object = ExtractObjectByName(_current_object, _current_name)
		if _is_array then _current_object = ExtractArrayElementByIndex(_current_object, _current_array_index)
	next

	GetObjectByPath = _current_object
End Function 


Function ExtractObjectByName(ByVal _input_object As JSONObject, _name As String) As JSONObject
	for i=0 to _input_object.arr_objects.ubound
		if _input_object.arr_objects[i].name = _name then
			ExtractObjectByName = _input_object.arr_objects[i]
			Exit Function
		end if
	next
	Dim _empty_object As JSONObject
	_empty_object.type = "empty"
	ExtractObjectByName = _empty_object
End Function

Function ExtractArrayElementByIndex(ByVal _input_object As JSONObject, _index As Integer) As JSONObject
	if _index < _input_object.arr_objects.size then
		ExtractArrayElementByIndex = _input_object.arr_objects[_index]
	else
		Dim _empty_object As JSONObject
		_empty_object.type = "empty"
		ExtractArrayElementByIndex = _empty_object
	end if
End Function









'------------------------------------
' TESTS

Sub PrintObject(_object As JSONObject, _tab As Integer)
	Dim _spaces As String
	for i=0 to _tab-1
		_spaces &= "  "
	next
	
	Dim _array_info As String
	if _object.type == "array" then
		_array_info = ":" & _object.arr_objects.size
	end if

	println(_spaces & _object.name & "["&_object.type&_array_info&"] = " & _object.value) ' & "  RAW: " & _object.raw_value)
	for i=0 to _object.arr_objects.ubound
		PrintObject(_object.arr_objects[i], _tab+1)
	next
End Sub

Dim tests_num, test_ok_count, test_error_count As Integer

Function RunOneTestGetValueByPath(input As String, path As String, expected_result As String) As Boolean
	Dim result As String
	
	result = GetObjectByPath(ParseJSON(input), path).value
	
	tests_num += 1
	If 	result == expected_result Then
		test_ok_count += 1
		Println("# "&tests_num&" OK. Expect & get: " & result)
	Else
		test_error_count += 1
		Println(4, "# "&tests_num&" ERROR. Expect |" & expected_result & "|, but get |" & result & "|. Input |" & input & "|, path |" & path & "|")
	End If
End Function

Function RunOneValidationTest(input As String, expected_result As Boolean) As Boolean
	Dim result As Boolean = PrimitiveValidateJSON(input)
	
	tests_num += 1
	If result == expected_result Then
		test_ok_count += 1
		Println("# "&tests_num&" PASS validation. Expect & get: " & result)
	Else
		test_error_count += 1
		Println(4, "# "&tests_num&" ERROR. Expect |" & expected_result & "|, but get |" & result & "|. Input |" & input & "|")
	End If
End Function

Sub RunAllTests()
	tests_num = 0
	test_ok_count = 0
	test_error_count = 0
	
	Println(1, "--- START RUN TESTS --- " & GetCurrentTime().ToString())
	
	RunOneValidationTest("{\"id\": 1}", true)
	RunOneValidationTest("{\"id\": 1}}", false)
	RunOneValidationTest("{{\"id\": 1}", false)
	RunOneValidationTest("{\"\"id\": 1}", false)
	RunOneValidationTest("{\"id\"\": 1}", false)
	
	RunOneTestGetValueByPath("{\"id\": 1}", "id", "1")
	RunOneTestGetValueByPath("{\"title\": \"sunt aut facere\"}", "title", "sunt aut facere")
	
	if test_error_count <= 0 Then
		println("OK. GOOD. ALL "&tests_num&" TESTS PASSED.")
	else
		Println(4, "SOMETHING WENT WRONG IN TESTS. Errors count = " & test_error_count)
	end if
End Sub
