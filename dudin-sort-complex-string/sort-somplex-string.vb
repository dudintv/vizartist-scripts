RegisterPluginVersion(1,1,0)
Dim info As String = "Sorting multiline text according specific value in each line
Developer: Dmitry Dudin, http://dudin.tv"

Dim cinput, cOutput As Container
Dim inputText, resultText, inputVarName, outputVarName, sortableType As String

Dim arrTypes As Array[String]
arrTypes.Push("Container")
arrTypes.Push("SHM")
Dim TYPE_CONTAINER = 0
Dim TYPE_SHM = 1

Dim arrSortableType As Array[String]
arrSortableType.Push("string")
arrSortableType.Push("integer")
arrSortableType.Push("float")
Dim SORTABLE_TYPE_STRING = 0
Dim SORTABLE_TYPE_INTEGER = 1
Dim SORTABLE_TYPE_FLOAT = 2

sub OnInitParameters()
	RegisterParameterString("lines_separator", "Lines separator", "\\n", 999, 999, "")
	RegisterParameterString("fields_separator", "Felds separator", "|", 999, 999, "")
	RegisterParameterInt("sortable_field", "Sort by field #",1, -999, 999)
	RegisterRadioButton("sortable_type", "Sort as", SORTABLE_TYPE_FLOAT, arrSortableType)
	RegisterRadioButton("input_type", "Input from", TYPE_CONTAINER, arrTypes)
	RegisterParameterContainer("input_container", " └ Input container")
	RegisterParameterString("input_var", " └ Input SHM system name", "", 999, 999, "")
	RegisterRadioButton("output_type", "Output to", TYPE_CONTAINER, arrTypes)
	RegisterParameterContainer("output_container", " └ Output container")
	RegisterParameterString("output_var", " └ Output SHM system name", "", 999, 999, "")
	RegisterParameterBool("direction", "Direction A-Z", false)
	RegisterParameterBool("auto_sort", "Auto sorting (when input is changing)", false)
	RegisterPushButton("go", "Sort it!", 1)
	RegisterPushButton("test", "Run tests", 2)
end sub

sub OnInit()
	cinput = GetParameterContainer("input_container")
	cOutput = GetParameterContainer("output_container")
	
	inputVarName = GetParameterString("input_var")
	inputVarName.Trim()
	outputVarName = GetParameterString("output_var")
	outputVarName.Trim()
	
	Select Case GetParameterInt("input_type")
	Case TYPE_CONTAINER
		if inputVarName <> "" then System.Map.UnregisterChangedCallback(inputVarName)
		cinput.Geometry.RegisterTextChangedCallback()
	Case TYPE_SHM
		cinput.Geometry.UnregisterChangedCallback()
		if inputVarName <> "" then System.Map.RegisterChangedCallback(inputVarName)
	End Select
	
	
	Select Case GetParameterInt("sortable_type")
	Case SORTABLE_TYPE_STRING
		sortableType = "string"
	Case SORTABLE_TYPE_INTEGER
		sortableType = "integer"
	Case SORTABLE_TYPE_FLOAT
		sortableType = "float"
	End Select
	
	if GetParameterBool("auto_sort") then
		Sort()
	end if
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
	
	SendGuiParameterShow("input_container", CInt(GetParameterInt("input_type") == TYPE_CONTAINER))
	SendGuiParameterShow("input_var", CInt(GetParameterInt("input_type") == TYPE_SHM))
	SendGuiParameterShow("output_container", CInt(GetParameterInt("output_type") == TYPE_CONTAINER))
	SendGuiParameterShow("output_var", CInt(GetParameterInt("output_type") == TYPE_SHM))
end sub

sub OnGeometryChanged(geom As Geometry)
	if GetParameterBool("auto_sort") then
		Sort()
	end if
end sub

Sub Sort()
	if GetParameterInt("input_type") == TYPE_CONTAINER Then
		if cInput == null then exit sub
		inputText = cInput.Geometry.Text
	elseif GetParameterInt("input_type") == TYPE_SHM Then
		if inputVarName <> "" then inputText = System.Map[inputVarName]
	end if
	
	resultText = SortArrayLines(inputText, GetParameterString("lines_separator"), GetParameterString("fields_separator"), GetParameterInt("sortable_field"), GetParameterBool("direction"), sortableType)
	
	if GetParameterInt("output_type") == TYPE_CONTAINER Then
		if cOutput == null then exit sub
		cOutput.geometry.text = resultText
	elseif GetParameterInt("output_type") == TYPE_SHM Then
		if outputVarName <> "" then System.Map[outputVarName] = resultText
	end if
End Sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		Sort()
	elseif buttonId == 2 then
		Test1()
		Test2()
		Test3()
		Test4()
		Test5()
		Test6()
		Test7()
		Test8()
		Test9()
		Test10()
	end if
end sub

Function SortArrayLines(input_string As String, line_separator As String, data_separator As String, element_to_sort As Integer, direction As Boolean, datatype As String) As String
	Dim arr_input_string As Array[String]
	Dim arr_output_string As Array[String]
	Dim arr_line_string As Array[String]
	Dim arr_sorter_data As Array[String]
	Dim temp_string, temp_value, temp_i, temp_j As String
	Dim result_compare As Boolean
	
	if line_separator == "\\n" then line_separator = "\n"
	input_string.Trim()
	input_string.Split(line_separator, arr_input_string)
	
	if element_to_sort == 0 Then
		SortArrayLines = input_string
		exit function
	end if
	
	arr_output_string.Clear()
	arr_sorter_data.Clear()
	for i=0 to arr_input_string.ubound
		arr_output_string.push(arr_input_string[i])
		if data_separator <> "" then
			'if there is the separator — extract data for sroting
			arr_input_string[i].Split(data_separator, arr_line_string)
			if element_to_sort > arr_line_string.ubound then
				arr_sorter_data.Push(arr_line_string[arr_line_string.ubound])
			elseif element_to_sort < -arr_line_string.ubound then
				arr_sorter_data.Push(arr_line_string[0])
			elseif element_to_sort < 0 Then
				arr_sorter_data.Push(arr_line_string[arr_line_string.size + element_to_sort])
			else
				arr_sorter_data.Push(arr_line_string[element_to_sort-1])
			end if
		else
			'if no separation — put the whole string in the sortable array
			arr_sorter_data.Push(arr_input_string[i])
		end if
	next
	
	for i=0 to arr_sorter_data.ubound
		for j=i+1 to arr_sorter_data.ubound
			temp_i = arr_sorter_data[i]
			temp_j = arr_sorter_data[j]
			temp_i.Trim()
			temp_j.Trim()
			
			datatype.MakeLower()
			if datatype == "string" or datatype == "str" or datatype == "s" then
				if direction then
					result_compare = temp_i > temp_j
				else
					result_compare = temp_i < temp_j
				end if
			elseif datatype == "integer" or datatype == "int" or datatype == "i" then
				if direction then
					result_compare = CInt(temp_i) > CInt(temp_j)
				else
					result_compare = CInt(temp_i) < CInt(temp_j)
				end if
			elseif datatype == "float" or datatype == "f" then
				temp_i.Substitute(",", ".", true)
				temp_j.Substitute(",", ".", true)
				if direction then
					result_compare = CDbl(temp_i) > CDbl(temp_j)
				else
					result_compare = CDbl(temp_i) < CDbl(temp_j)
				end if
			end if
			
			if result_compare then
				temp_string = arr_sorter_data[i]
				arr_sorter_data[i] = arr_sorter_data[j]
				arr_sorter_data[j] = temp_string
				
				temp_value = arr_output_string[i]
				arr_output_string[i] = arr_output_string[j]
				arr_output_string[j] = temp_value
			end if
		next
	next
	
	SortArrayLines.Join(arr_output_string, line_separator)
End Function



'************************************************************
'************************************************************
'************************************************************

'  TEST

Sub TestIt(test_input As string, expect_output As string, test_output As String, name_test As string)
	println(3, "test_input = " & test_input)
	println(3, "test_output = " & test_output)
	if test_output == expect_output then
		println(2, "TEST "&name_test&" PASSED")
	else
		println(4, "TEST "&name_test&" FAIL")
	end if
End Sub

'----------------------------------------------

Sub Test1()
	Dim test_input As String = "bt|Second|2
df|First|1
rh|Thirds|3
rh|Fife|5
rh|Four|4"
	Dim expect_output As String = "rh|Fife|5
rh|Four|4
rh|Thirds|3
bt|Second|2
df|First|1"
	TestIt(test_input, expect_output, SortArrayLines(test_input, "\n", "|", 3, false, "string"), "#1")
End Sub

Sub Test2()
	Dim test_input As String = "bt|Second|2
df|First|1
rh|Thirds|3
rh|Fife|5
rh|Four|4"
	Dim expect_output As String = "df|First|1
bt|Second|2
rh|Thirds|3
rh|Four|4
rh|Fife|5"
	TestIt(test_input, expect_output, SortArrayLines(test_input, "\n", "|", 3, true, "string"), "#2")
End Sub

Sub Test3()
	Dim test_input As String    = "a,1,b|c,4,d|e,2,f"
	Dim expect_output As String = "a,1,b|e,2,f|c,4,d"
	TestIt(test_input, expect_output, SortArrayLines(test_input, "|", ",", 2, true, "s"), "#3")
End Sub

Sub Test4()
	Dim test_input As String    = "a,  1,b|c,4  ,d|e,  2,f"
	Dim expect_output As String = "c,4  ,d|e,  2,f|a,  1,b"
	TestIt(test_input, expect_output, SortArrayLines(test_input, "|", ",", 2, false, "str"), "#4 (ignore whitespaces)")
End Sub

Sub Test5()
	Dim test_input As String    = "a,10,b|c,04,d|e,7,f"
	Dim expect_output As String = "c,04,d|e,7,f|a,10,b"
	TestIt(test_input, expect_output, SortArrayLines(test_input, "|", ",", 2, true, "integer"), "#5")
End Sub

Sub Test6()
	Dim test_input As String    = "a,010,b|c,04,d|e,07,f"
	Dim expect_output As String = "a,010,b|e,07,f|c,04,d"
	TestIt(test_input, expect_output, SortArrayLines(test_input, "|", ",", 2, false, "i"), "#6")
End Sub

Sub Test7()
	Dim test_input As String    = "Titr,Name,Hrip,,Geo,"
	Dim expect_output As String = ",,Geo,Hrip,Name,Titr"
	TestIt(test_input, expect_output, SortArrayLines(test_input, ",", "", 1, true, "str"), "#7")
End Sub

Sub Test8()
	Dim test_input As String    = ",10,102,,5,70,"
	Dim expect_output As String = "102,70,10,5,,,"
	TestIt(test_input, expect_output, SortArrayLines(test_input, ",", "", 1, false, "int"), "#8")
End Sub

Sub Test9()
	Dim test_input As String    = "-5,-50,15,0,1"
	Dim expect_output As String = "-50,-5,0,1,15"
	TestIt(test_input, expect_output, SortArrayLines(test_input, ",", "", 1, true, "int"), "#9 (as unteger)")
End Sub

Sub Test10()
	Dim test_input As String    = "-50,-5,15,0,1"
	Dim expect_output As String = "-5,-50,0,1,15"
	TestIt(test_input, expect_output, SortArrayLines(test_input, ",", "", 1, true, "string"), "#10 (as string)")
End Sub
