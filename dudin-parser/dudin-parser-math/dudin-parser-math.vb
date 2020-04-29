Dim variables As StringMap
variables["base"] = 123

sub OnInitParameters()
	RegisterParameterString("math", "Math string", "", 40, 999, "")
	RegisterPushButton("go", "Recalculate", 1)
	RegisterPushButton("test", "Run tests", 2)
end sub

sub OnInit()
	println("")
	println("")
	println("")
	println("Viz ElapsedTime = " & System.GetElapsedTime())
	println("===========================")
	this.Geometry.Text = GetParameterString("math") & " = " & MathParseWithVariables(GetParameterString("math"), variables)
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub
sub OnExecAction(buttonId As Integer)
	select case buttonId
	case 1
		OnInit()
	case 2
		RunTests()
	end select
end sub

'------------------------------------------------------------------

Function MathParse(ByVal _s As String) As Double
	_s.Trim()
	_s.Substitute("\\s","",true)
	Dim _variables As StringMap
	MathParse = MathParseItem(_s, _variables)
End Function

Function MathParseWithVariables(ByVal _s As String, _variables As StringMap) As Double
	_s.Trim()
	_s.Substitute("\\s","",true)
	MathParseWithVariables = MathParseItem(_s, _variables)
End Function

Function MathParseItem(ByVal s As String, variables As StringMap) As Double
	if s == "" then
		MathParseItem = 0
		exit function
	end if
	
	' IF IT'S SIMPLE VALUE
'	if s.Match("^[\\+\\-]?\\d*$") then
'		' return value
'		println("return simple value = " & s)
'		MathParseItem = CDbl(s)
'		exit function
'	elseif s.Match("^[\\+\\-]?[^\\d\\W]*$") then
'		' return value of variable
'		println("return simple variable value = " & s)
'		MathParseItem = MathGetVariableValue(s, variables)
'		exit function
'	end if
	
	Dim folding_level As Integer
	Dim arr_actions As Array[String]
	Dim arr_values As Array[Double]
	Dim char, collect As String
	
	Dim type_char, prev_type_char As Integer
	Dim NUMBER As Integer = 1
	Dim ACTION As Integer = 2
	Dim PARENTHES As Integer = 3
	Dim VARIABLE As Integer = 4
	'Dim METHOD As Integer = 5
	
	
	' first sign
	if s.GetChar(0) == "+" OR s.GetChar(0) == "-" then
		collect = s.GetChar(0)
		s.Erase(0,1)
	elseif s.GetChar(0) == "*" OR s.GetChar(0) == "/" then
		collect = "+"
		s.Erase(0,1)
	else
		collect = "+"
	end if
	arr_actions.Push(collect)
	collect = ""
	type_char = ACTION
	
	' parse 
	folding_level = 0
	do while s.length > 0
		char = s.GetChar(0)
		s.Erase(0,1)
		
		if char == "(" then
			folding_level += 1
			
			if folding_level == 1 then
				if prev_type_char == NUMBER then
					arr_values.Push(CDbl(collect))
				end if
				if prev_type_char == ACTION then
					arr_actions.Push(collect)
				end if
				if prev_type_char == VARIABLE then
					'detect method name
					arr_actions[-1] &= collect
				end if
				collect = ""
			end if
		end if
		if char == ")" then
			folding_level -= 1
			if folding_level == 0 then
				'return to zero level - it's time to calculate inside the parentheses
				collect.erase(0,1)
				arr_values.Push(MathParseItem(collect, variables))
				collect = ""
			end if
		end if
		
		if folding_level > 0 then
			'inside parentheses
			collect &= char
			prev_type_char = PARENTHES
		elseif folding_level == 0 then
			'get current type of char
			if char.Match("[\\d\\.]") then
				type_char = NUMBER
			elseif char=="+" OR char=="-" OR char=="*" OR char=="/" then
				type_char = ACTION
			elseif char=="(" OR char==")" then
				type_char = PARENTHES
			elseif char.Match("[^\\d\\W]") then
				type_char = VARIABLE
			end if
			
			'println("char = " & char & " and type = " & type_char)
			if type_char <> prev_type_char then
				if prev_type_char == ACTION then
					arr_actions.Push(collect)
				end if
				if prev_type_char == NUMBER then
					arr_values.Push(CDbl(collect))
				end if
				if prev_type_char == VARIABLE then
					arr_values.Push(MathGetVariableValue(collect, variables))
				end if
				collect = char
				prev_type_char = type_char
			elseif s.length == 0 then
				collect &= char
				if type_char == NUMBER then
					arr_values.Push(CDbl(collect))
				end if
				if type_char == ACTION then
					arr_actions.Push(collect)
				end if
				if type_char == VARIABLE then
					arr_values.Push(MathGetVariableValue(collect, variables))
				end if
				collect = ""
			else
				collect &= char
			end if
		else
			'folding_level < 0  ---  ERROR
			MathParseItem = 0
			exit function
		end if
	loop
	if collect <> "" AND prev_type_char == NUMBER then arr_values.Push(CDbl(collect))
	
	println(arr_actions)
	println(arr_values)
	
	'CALCULATIONS
	Dim calc_result As Double
	
	'calculate methods()
	Dim method_action, method As String
	for i=0 to arr_actions.ubound
		if arr_actions[i].length > 1 and arr_actions[i].match(".[^\\d\\W]*") then
			method_action = arr_actions[i].Getchar(0)
			method = arr_actions[i].GetSubstring(1, arr_actions[i].length-1)
			arr_values[i] = MathMethod(method, arr_values[i])
			arr_actions[i] = method_action
		end if
	next
	
	' calculate * and /
	for i=1 to arr_values.ubound
		if arr_actions[i] == "*" then
			calc_result = arr_values[i-1] * arr_values[i]
		elseif arr_actions[i] == "/" then
			calc_result = arr_values[i-1] / arr_values[i]
		end if
		if arr_actions[i] == "*" OR arr_actions[i] == "/" then
			arr_values[i-1] = calc_result
			arr_values.Erase(i)
			arr_actions.Erase(i)
		end if
	next
	
	'calculate + and -
	calc_result = 0
	for i=0 to arr_values.ubound
		if arr_actions[i] == "+" then
			calc_result += arr_values[i]
		elseif arr_actions[i] == "-" then
			calc_result -= arr_values[i]
		end if
	next
	
	MathParseItem = calc_result
End Function

Function MathGetVariableValue(name As String, variables As StringMap) As Double
	if variables.ContainsKey(name) then
		MathGetVariableValue = CDbl(variables[name])
	else
		MathGetVariableValue = 0
	end if
End Function

Function MathMethod(name As String, input As Double) As Double
	select case name
	case "rand", "random"
		MathMethod = input*Random()
	case "wiggle"
		MathMethod = input*Random() - input/2.0
	end select
End Function


Dim count_tests, count_ok_tests, count_fail_tests As Integer
Sub RunTests()
	count_tests = 0
	count_ok_tests = 0
	count_fail_tests = 0
	println("")
	println("RUN TESTS:")
	
	Test("", 0)
	Test("-", 0)
	Test("+", 0)
	'Test("+-1", 0)
	Test("2+3", 5)
	Test("-7", -7)
	Test("-7+1", -6)
	Test("1+1000", 1001)
	
	Test("(-1+2)", 1)
	Test("-(-1+2)", -1)
	Test("1+(1+2)", 4)
	Test("0-(-1+2)", -1)
	Test("1000-(-15)-(10-5)", 1010)
	Test("100+(10+(1+1))", 112)
	Test("100-(10-(1+1))", 92)
	Test("100-(10-(4+5-(3-2)))", 98)
	Test("-(10-(-(3-2)+4+5))+100", 98)
	
	Test("2*3", 6)
	Test("-2*3", -6)
	Test("2*(-3)", -6)
	Test("1+2*3", 7)
	Test("2*3+10", 16)
	
	Test("2*(3+10)", 26)
	Test("-2*(3+10)-10", -36)
	Test("(1+2)*10", 30)
	Test("-(1+2)*10", -30)
	Test("(10)+((20-5)*3-5*(-50+10))-(10)", 245)
	
	Test("base", 123)
	Test("base+10", 133)
	Test("base+base", 246)
	Test("-2*(10-base)", 226)
	Test("-(base)*2", -246)
	Test("base*1", 123)
	Test("+(base*base)", 123*123)
	
	TestWithRange("rand(10)", 0, 10)
	TestWithRange("100*rand(1)", 0, 100)
	TestWithRange("10+rand(10)", 10, 20)
	TestWithRange("wiggle(10)", -10, 10)
	
	println("------------------")
	println(2, count_tests & " tests. " & count_ok_tests & " ok. " & count_fail_tests & " fail.")
	if count_fail_tests > 0 then
		println(5, "AT LEAST ONE TEST IS FAIL!")
	else
		println("ALL TESTS OK :)")
	end if
	println("------------------")
End Sub

Sub Test(s As String, expectation As Double)
	count_tests += 1
	Dim result = MathParseWithVariables(s, variables)
	if result == expectation then
		count_ok_tests += 1
		println(9, "OK: " & s & " == " & expectation)
	else
		count_fail_tests += 1
		println(5, "FAIL: " & s & " = " & result & ". But expected " & expectation)
	end if
End Sub

Sub TestWithRange(s As String, expectation_min As Double, expectation_max As Double)
	count_tests += 1
	Dim result = MathParseWithVariables(s, variables)
	if result >= expectation_min AND result <= expectation_max then
		count_ok_tests += 1
		println(9, "OK: " & s & " == " & result & " is inside range ["&expectation_min&","&expectation_max&"]")
	else
		count_fail_tests += 1
		println(5, "FAIL: " & s & " = " & result & ". But expected in range ["&expectation_min&","&expectation_max&"]")
	end if
End Sub

