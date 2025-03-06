RegisterPluginVersion(1,1,0)
Dim info As String = "UTF8 Text colorizing
Developer: Dmitry Dudin
http://dudin.tv"

Structure HashItem
	sourceText As String
	iStart, iEnd As Integer
	paint As Color
End Structure
Dim arrHashItems As Array[HashItem]

Dim c As Container
Dim s, geomId As String
Dim arrChars As Array[String]
Dim baseColor = CColor(1.0, 1.0, 1.0, 1.0)
Dim BUTTON_COLORIZE = 1
Dim BUTTON_CLEAR = 2

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterBool("is_realtime", "Colorize in realtime?", false)
	RegisterParameterColor("base_color", "Base Color", baseColor)
	RegisterParameterContainer("container_with_text", "Container with text (or this)")
	
	RegisterParameterBool("is_hash_enabled", "Colorize #hash", true)
	RegisterParameterColor("hash_color", " └ Color", baseColor)
	RegisterParameterColor("hash_color_in_urgent", " └ Color within urgent", baseColor)
	
	RegisterParameterBool("is_urgent_enabled", "Colorize !urgent", true)
	RegisterParameterBool("should_remove_urgent_mark", " └ Remove leading '!' mark", true)
	RegisterParameterColor("ugrent_color", " └ Color", baseColor)
	
	
	RegisterPushButton("colorize", "Colorize", BUTTON_COLORIZE)
	RegisterPushButton("clear", "Clear (to base color)", BUTTON_CLEAR)
	RegisterParameterText("console", "", 100, 300)
end sub

sub OnInit()
	baseColor = GetParameterColor("base_color")
	c = GetParameterContainer("container_with_text")
	if c == null then c = this
	if c <> null AND c.geometry <> null then c.geometry.RegisterTextChangedCallback()
	
	if GetParameterBool("is_realtime") then
		Colorize()
	end if
end sub

sub OnParameterChanged(parameterName As String)
	SendGuiParameterShow("hash_color", CInt(GetParameterBool("is_hash_enabled")))
	SendGuiParameterShow("should_remove_urgent_mark", CInt(GetParameterBool("is_urgent_enabled")))
	SendGuiParameterShow("ugrent_color", CInt(GetParameterBool("is_urgent_enabled")))
	SendGuiParameterShow("hash_color_in_urgent", CInt(GetParameterBool("is_urgent_enabled")))
	
	if GetParameterBool("is_realtime") then
		if parameterName <> "console" then Colorize()
	end if
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == BUTTON_COLORIZE then
		Colorize()
	elseif buttonId == BUTTON_CLEAR then
		PrepareText()
		ColorizeFullText()
	end if
end sub

Sub PrepareText()
	geomId = "#" & c.Geometry.VizId
	s = c.Geometry.Text
	println("s = " & s)
	arrChars.clear()
	for i=0 to s.length-1
		'dim sAnsi = s.GetSubstring(i, 1)
		'sAnsi.Utf8ToAnsi()
		'if sAnsi == "" then
		if IsDoubleByteChar(s.GetSubstring(i, 1)) then
			'if non-latin char
			arrChars.push(s.GetSubstring(i, 2))
			i += 1
		else
			arrChars.push(s.GetChar(i))
		end if
	next
End Sub
function IsDoubleByteChar(char as String) as Boolean
    IsDoubleByteChar = Asc(char) > 127
end function

Sub Colorize()
	PrepareText()
	ColorizeFullText()
	
	if  GetParameterBool("is_urgent_enabled") then 
		ColorizeUrgents()
	end if
	
	if GetParameterBool("should_remove_urgent_mark") AND s.Left(1) == "!" then
		c.Geometry.Text = s.GetSubstring(1, s.length-1)
		arrChars.Erase(0)
	end if
	
	if GetParameterBool("is_hash_enabled") then 
		PrepareHashes()
		ColorizeHashes()
	end if
	
	Dim console = ""
	for i=0 to arrHashItems.ubound
		console = console & HashItemDisplay(arrHashItems[i]) 
	next
	this.ScriptPluginInstance.SetParameterString("console", console)
End Sub

Sub ColorizeFullText()
	baseColor = GetParameterColor("base_color")
	System.SendCommand(geomId & "*CURSORPOS SET " & 1)
	System.SendCommand(geomId & "*PROP*COLOR SET " & "1.0:1." & s.length & " " & ColorToText(baseColor))
	System.SendCommand(geomId & "*PROP*ALPHA SET " & "1.0:1." & s.length & " " & AlphaToText(baseColor))
End Sub

Dim canStartHash, hasEnd, isStringEnd As Boolean
Dim currentChar As String
Sub PrepareHashes()
	arrHashItems.clear()
	canStartHash = true ' because of the string beginning
	for i=0 to arrChars.ubound
		currentChar = arrChars[i]
		if currentChar == "#" AND canStartHash then
			hasEnd = false
			for j=i to s.length-1
				isStringEnd = j >= arrChars.ubound
				if arrChars[j] == " " OR arrChars[j] == "\n" OR isStringEnd then
					Dim item As HashItem
					item.iStart = i
					item.iEnd = j + CInt(isStringEnd)
					if GetParameterBool("is_urgent_enabled") AND s.Left(1) == "!" then
						item.paint = GetParameterColor("hash_color_in_urgent")
					else
						item.paint =  GetParameterColor("hash_color")
					end if
					item.sourceText = GetSubstring(item.iStart, item.iEnd-1)
					arrHashItems.push(item)
					i = j+1
					hasEnd = true
				end if
				if hasEnd then exit for
			next
		else
			canStartHash = currentChar == " "
		end if
	next
End Sub

Sub ColorizeHashes()
	for i=0 to arrHashItems.ubound
		Dim selectionRange = "1." & arrHashItems[i].iStart & ":1." & arrHashItems[i].iEnd
		System.SendCommand(geomId & "*CURSORPOS SET " & 1)
		System.SendCommand(geomId & "*PROP*COLOR SET " & selectionRange & " " & ColorToText(arrHashItems[i].paint))
		System.SendCommand(geomId & "*PROP*ALPHA SET " & selectionRange & " " & AlphaToText(arrHashItems[i].paint))
	next
End Sub

Sub ColorizeUrgents()
	if s.GetSubstring(0, 1) == "!" then
		Dim selectionRange = "1.0:1." & s.length
		Dim paint = GetParameterColor("ugrent_color")
		System.SendCommand(geomId & "*PROP*COLOR SET " & selectionRange & " " & ColorToText(paint))
		System.SendCommand(geomId & "*PROP*ALPHA SET " & selectionRange & " " & AlphaToText(paint))
	end if
End Sub

Function GetSubstring(iFrom As Integer, iTo As Integer) As String
	Dim result = ""
	for k=iFrom to iTo
		result &= arrChars[k]
	next
	GetSubstring = result
End Function

function ColorToText(cPaint as color) As String
    Dim r as double = cPaint.red
    Dim g as double = cPaint.green
    Dim b as double = cPaint.blue
    ColorToText = CStr(r) & " " & CStr(g) & " " & CStr(b)
End Function

function AlphaToText(cPaint as color) As String
    Dim a as integer = cPaint.alpha * 100
    AlphaToText = CStr(a)
End Function

Function HashItemDisplay(item As HashItem) As String
	Dim toPrint = "FOUND HASH = " & item.sourceText & "\n"
	toPrint = toPrint & "i: " & "[" & item.iStart & "..." & item.iEnd & "]" & "\n"
	HashItemDisplay = toPrint
End Function
