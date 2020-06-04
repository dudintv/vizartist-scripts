RegisterPluginVersion(1,1,0)
'Author: Dmitry Dudin, http://dudin.tv
Dim c_number, c_source, c_output As Container
Dim s, source, prev_source, prefix, inside_par, suffix, output As String
Dim arr_s As Array[String]
Dim number, prev_number, number_100, number_10 As Integer
Dim has_different_source As Boolean
Dim open_par, close_par As Integer

Dim buttonOutputs As Array[String]
buttonOutputs.Push("Parent")
buttonOutputs.Push("Prev")
buttonOutputs.Push("This")
buttonOutputs.Push("Next")
buttonOutputs.Push("Other")
sub OnInitParameters()
	RegisterParameterBool("number_from_container", "Get Number from container", false)
	RegisterRadioButton("number_select", "Number", 0, buttonOutputs)
	RegisterParameterContainer("c_number", "Number container (or this)")
	RegisterParameterInt("number", "Number", 0, -999999, 999999)
	
	RegisterParameterBool("source_from_container", "Get Source Text from container", false)
	RegisterRadioButton("source_select", "Source", 0, buttonOutputs)
	RegisterParameterContainer("c_source", "Source container (or this)")
	RegisterParameterString("source", "Source", "", 60, 999, "")
	
	RegisterRadioButton("output_select", "Output", 0, buttonOutputs)
	RegisterParameterContainer("c_output", "Output container (or this)")
end sub

sub OnInit()
	Select Case GetParameterInt("number_select")
	Case 0
		'Parent
		c_number = this.ParentContainer
	Case 1
		'Prev
		c_number = this.PreviousContainer
	Case 2
		c_number = this
	Case 3
		'Next
		c_number = this.NextContainer
	Case 4
		c_number = GetParameterContainer("c_number")
	End Select
	'-----------------------------------------------
	Select Case GetParameterInt("source_select")
	Case 0
		'Parent
		c_source = this.ParentContainer
	Case 1
		'Prev
		c_source = this.PreviousContainer
	Case 2
		c_source = this
	Case 3
		'Next
		c_source = this.NextContainer
	Case 4
		c_source = GetParameterContainer("c_source")
	End Select
	'-----------------------------------------------
	Select Case GetParameterInt("output_select")
	Case 0
		'Parent
		c_output = this.ParentContainer
	Case 1
		'Prev
		c_output = this.PreviousContainer
	Case 2
		c_output = this
	Case 3
		'Next
		c_output = this.NextContainer
	Case 4
		c_output = GetParameterContainer("c_output")
	End Select
end sub
sub OnParameterChanged(parameterName As String)
	SendGuiParameterShow("c_number", CInt( GetParameterBool("number_from_container") AND GetParameterInt("number_select") == 4 ))
	SendGuiParameterShow("number_select", CInt( GetParameterBool("number_from_container") ))
	SendGuiParameterShow("number", CInt( NOT GetParameterBool("number_from_container") ))
	SendGuiParameterShow("c_source", CInt( GetParameterBool("source_from_container") AND GetParameterInt("source_select") == 4 ))
	SendGuiParameterShow("source_select", CInt( GetParameterBool("source_from_container") ))
	SendGuiParameterShow("source", CInt( NOT GetParameterBool("source_from_container") ))
	SendGuiParameterShow("c_output", CInt( GetParameterInt("output_select") == 4 ))
	OnInit()
end sub

sub OnExecPerField()
	' CALC NUMBER
	if GetParameterBool("number_from_container") then
		number = CInt(c_number.geometry.text)
	else
		number = GetParameterInt("number")
	end if
	if prev_number <> number then
		if number < 0 then number *= -1
		number_100 = DivisionRemainderInteger(number, 100)
		number_10  = DivisionRemainderInteger(number, 10)
	end if
	
	' GET source
	if GetParameterBool("source_from_container") then
		source = c_source.geometry.text
	else
		source = GetParameterString("source")
	end if
	if prev_source <> source then
		has_different_source = source.find("(") > -1
	end if

	' OUTPUT
	if prev_number <> number OR prev_source <> source then
		if has_different_source then
			output = source
			Dim anti_infinity As Integer = 0
			do while output.find("(") > -1 AND output.find(")") > -1 AND anti_infinity < 20
				open_par = output.FindFirstOf("(")
				close_par = output.FindFirstOf(")")
				prefix = output.left(open_par)
				suffix = output.right(output.length - close_par - 1)
				inside_par = output.GetSubstring( open_par + 1, close_par - open_par - 1 )
				output = prefix & GetDeclentionForm(inside_par) & suffix
				anti_infinity += 1
			loop
			c_output.geometry.text = output
		else
			'without any parsing:
			c_output.geometry.text = source
		end if
	end if
	
	' PREPARE PREVIOUS VALUES
	prev_number = number
	prev_source = source
end sub

Function GetDeclentionForm(forms As String) As String
	forms.Split(",", arr_s)
	if number_100 > 10 AND number_100 < 20 then
		GetDeclentionForm =  arr_s[2]
	elseif number_10 > 1 AND number_10 < 5 then
		GetDeclentionForm = arr_s[1]
	elseif number_10 == 1 then
		GetDeclentionForm = arr_s[0]
	else
		GetDeclentionForm =  arr_s[2]
	end if
End Function

Function DivisionRemainderInteger(ByVal _input As Integer, _limit As Integer) As Integer
	do while _input > _limit
		_input -= _limit
	loop
	DivisionRemainderInteger = _input
End Function
