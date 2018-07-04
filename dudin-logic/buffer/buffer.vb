Dim info As String = "скрипт запоминает состояние набора титров и может их востанавливать
Разработчик: Дудин Дмитрий.
Версия 0.31 (26 октября 2013)"	
 
sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterString("bufferName", "Имя буфера", "", 30, 100, "")
	RegisterParameterString("bufferList", "Список титров для буфера", "", 50, 100, "")
	RegisterParameterString("bufferBase", "Что выдавать если непонятно", "", 50, 100, "")
end sub
 
Dim s, name As String
Dim arr_titrs As Array[String]
Dim arr_base As Array[String]
Dim arr_states As Array[Integer]
Dim i_show As Integer
 
sub OnInit()
	'запоминаем список контролируемых титров
	s = GetParameterString("bufferList")
	s.trim()
	s.split(",",arr_titrs)
	for i = 0 to arr_titrs.UBound
		arr_titrs[i].trim()
		System.Map.RegisterChangedCallback(arr_titrs[i] & "_control")
	next
	
	'запоминаем список что выдавать по_умолчанию
	s = GetParameterString("bufferBase")
	s.trim()
	s.split(",",arr_base)
	for i = 0 to arr_base.UBound
		arr_base[i].trim()
	next
	
	
	name = GetParameterString("bufferName")
	System.Map.RegisterChangedCallback(name & "_control")
	System.Map.RegisterChangedCallback(name & "_store")
	System.Map.RegisterChangedCallback(name & "_restore")
	
	System.Map[name & "_status"] = "0"
	For i = 0 to arr_titrs.UBound
		System.Map[arr_titrs[i] & "_status"] = "0"
	Next
	'println("STATUS TO ZERO: " & CStr(arr_titrs))
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub
 
sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	'STORE & RESTORE
	If mapKey == (name & "_store") AND System.Map[mapKey] == "1" Then
		If IsSomeTake() > 0 Then
			arr_states.clear()
			for i = 0 to arr_titrs.UBound
				arr_states.Push(CInt(System.Map[arr_titrs[i] & "_status"]))
				System.Map[arr_titrs[i] & "_control"] = 5
				System.Map[arr_titrs[i] & "_control"] = 0
			next
		End If
		System.Map[name&"_store"] = 0
		System.Map[name&"_state"] = 0
	ElseIf mapKey == (name & "_restore") AND System.Map[mapKey] == "1" Then
		i_show = 0
		If arr_states.Size > 0 Then
			for i = 0 to arr_titrs.UBound
				System.Map[arr_titrs[i] & "_control"] = 5
				System.Map[arr_titrs[i] & "_control"] = arr_states[i]
				i_show += arr_states[i]
			next
		End If
		If i_show <= 0 Then
			'если нечего выдать из буфера...
			for i = 0 to arr_base.UBound
				System.Map[arr_base[i] & "_control"] = 5
				System.Map[arr_base[i] & "_control"] = 1
			next
		End if
		arr_states.clear()
		System.Map[name&"_restore"] = 0
		System.Map[name&"_state"] = 1
		
	'CONTROL this buffer
	ElseIf mapKey == name&"_control" Then
		If System.Map[mapKey] == "0" Then
			'Off
			System.Map[name&"_store"] = 0
			System.Map[name&"_store"] = 1
		ElseIf System.Map[mapKey] == "1" OR System.Map[mapKey] == "" Then
			'On
			System.Map[name&"_restore"] = 0
			System.Map[name&"_restore"] = 1
			
		ElseIf System.Map[mapKey] == "3" Then
			'On/Off
			If IsSomeTake() > 0 Then
				'если что-то из списка выдано, то считаем что это есть ВЫДАННОЕ СОСТОЯНИЕ
				'и буферизируем это состояние и все убираем
				System.Map[name&"_store"] = 0
				System.Map[name&"_store"] = 1
				'println("STORE")
			Else
				'если ничего не выдано их этого списка, то 
				'считаем что надо выдать забуферизированное состояние
				'конечно, если буффер заполенен...
				System.Map[name&"_restore"] = 0
				System.Map[name&"_restore"] = 1
				'println("RESTORE")
			End If
		End If
		
		System.Map[name&"_control"] = 5
		
		
	'CONTROL titrs
	ElseIf mapKey.Right(8) == "_control" Then
		Dim takeout_titr As String = mapKey.Left(mapKey.Length-8)
		
		If System.Map[takeout_titr&"_status"] == 0 AND System.Map[name&"_state"] == 0 Then
			for i = 0 to arr_titrs.UBound
				if takeout_titr == arr_titrs[i] then
					arr_states[i] = 0
				end if
			next
		End If
		
		If System.Map[takeout_titr&"_status"] == 1 AND System.Map[name&"_state"] == 0 Then
			arr_states.clear()
			for i = 0 to arr_titrs.UBound
				arr_states.Push(CInt(System.Map[arr_titrs[i] & "_status"]))
			next
		End If
	End If
	
end sub
 
 
'возвращает кол-во выданных титров из своего списка
Function IsSomeTake() As Integer
	Dim compare As Integer = 0
	for i = 0 to arr_titrs.UBound
		If System.Map[arr_titrs[i] & "_status"] == 1 Then compare += 1
	next
	IsSomeTake = compare
	
End Function