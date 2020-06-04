RegisterPluginVersion(1,0,0)
'Author: Dmitry Dudin, http://dudin.tv

Dim c_number, c_forms, c_output As Container
Dim s, forms, prev_forms, form_1, form_2, form_3, prefix, suffix As String
Dim arr_s As Array[String]
Dim number, prev_number, number_100, number_10 As Integer
Dim has_different_forms As Boolean

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
	
	RegisterParameterBool("forms_from_container", "Get Forms from container", false)
	RegisterRadioButton("forms_select", "Forms", 0, buttonOutputs)
	RegisterParameterContainer("c_forms", "Forms container (or this)")
	RegisterParameterString("forms", "Forms", "", 60, 999, "")
	
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
	Select Case GetParameterInt("forms_select")
	Case 0
		'Parent
		c_forms = this.ParentContainer
	Case 1
		'Prev
		c_forms = this.PreviousContainer
	Case 2
		c_forms = this
	Case 3
		'Next
		c_forms = this.NextContainer
	Case 4
		c_forms = GetParameterContainer("c_forms")
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
	SendGuiParameterShow("c_forms", CInt( GetParameterBool("forms_from_container") AND GetParameterInt("forms_select") == 4 ))
	SendGuiParameterShow("forms_select", CInt( GetParameterBool("forms_from_container") ))
	SendGuiParameterShow("forms", CInt( NOT GetParameterBool("forms_from_container") ))
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
	
	' GET FORMS
	if GetParameterBool("forms_from_container") then
		forms = c_forms.geometry.text
	else
		forms = GetParameterString("forms")
	end if
	if prev_forms <> forms then
		has_different_forms = forms.find("(") > -1
		if has_different_forms then
			prefix = forms.left(forms.find("("))
			suffix = forms.right(forms.length - forms.find(")") - 1)
			s = forms.GetSubstring( forms.find("(") + 1, forms.find(")") - forms.find("(") - 1 )
			s.Split(",", arr_s)
			form_1 = arr_s[0]
			form_2 = arr_s[1]
			form_3 = arr_s[2]
		end if
	end if

	' OUTPUT
	if has_different_forms then
		if prev_number <> number OR prev_forms <> forms then
			if number_100 > 10 AND number_100 < 20 then
				c_output.geometry.text = form_3
			elseif number_10 > 1 AND number_10 < 5 then
				c_output.geometry.text = prefix & form_2 & suffix
			elseif number_10 == 1 then
				c_output.geometry.text = prefix & form_1 & suffix
			else
				c_output.geometry.text = prefix & form_3 & suffix
			end if
		end if
	else
		c_output.geometry.text = prefix & forms & suffix
	end if
	
	' PREPARE PREVIOUS VALUES
	prev_number = number
	prev_forms = forms
end sub

Function DivisionRemainderInteger(ByVal _input As Integer, _limit As Integer) As Integer
	do while _input > _limit
		_input -= _limit
	loop
	DivisionRemainderInteger = _input
End Function
