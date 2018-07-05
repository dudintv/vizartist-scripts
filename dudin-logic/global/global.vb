Dim Info As String = "Разработчик: Дудин Дмитрий
Версия 4.0.1 (5 июля 2018)
-------------------------------------------------------
Это основной скрипт для работы системы автоубирания - 
он должен находится в дереве выше титровальных скриптов.
 
Еще тут можно посмотреть содержимое основных массивов. Жми print.
"
 
'Просто перенос строки - этой стандратной переменной мне так часто не хватает...
Dim vbNewLine = "\n"

'Разный префикс нужен для одновременной независимой работы двух титровальных систем
'например для сцены "ньюсбара" и сцены "титров"
'Он может быть произвольным. Главное чтобы совпадал с префиксом в элементных скриптах.
Dim prefix = ""
 
'при инициализации скрипта всегда сначала вызывается OnInitParameters() и только потом OnInit()
sub OnInitParameters()
	'изначально два основных массива пустые
	'и переменные для добавления новых элементов тоже очищаются
	System.Map[prefix & "AUTOTAKEOUTonTAKE"] = ""
	System.Map[prefix & "AUTOTAKEOUTonTAKEOUT"] = ""
	System.Map[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = ""
	System.Map[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"] = ""	
	
	Scene.Map[prefix & "AUTOTAKEOUTonTAKE"] = ""
	Scene.Map[prefix & "AUTOTAKEOUTonTAKEOUT"] = ""
	Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = ""
	Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"] = ""	
	
 
	'----------------------------------------------
	'создаем кнопку для печати содержимого двух массивов в текстовую область "console"
	RegisterPushButton("print","print",1)
	RegisterPushButton("printdeep","deeper",2)
	RegisterParameterText("console", "", 600, 300)
	RegisterInfoText(Info)
	RegisterParameterBool("print_in_console", "Print in console", FALSE)
end sub
 
Sub OnInit()
	'будем преехватывать ВСЕ переменные,
	'потому что это скрипт должен уметь реагировать на любой возможный элемент
	System.Map.RegisterChangedCallback("")
	Scene.Map.RegisterChangedCallback("")
 
	'при инициализации этого скрипта заставляем проинициализироваться вниз по списку
	'все скрипты относящиеся к системе автоубирания
	Scene.Map[prefix & "AUTOTAKEOUT_ALL_RECALCULATE"] = ""
	Scene.Map[prefix & "AUTOTAKEOUT_ALL_RECALCULATE"] = "GLOBAL SCRIPT START RELOAD ALL ELEMENTS"
 
	this.ScriptPluginInstance.SetParameterString("console", Info)
End Sub
 
'--------------------------------------------------------------------------------
Sub OnExecAction(buttonId As Integer)
	If buttonId == 1 OR buttonId == 2 Then
		'выводим в текстовую console обе переменнные массивов автоубирания:
		'{{на_что_реагировать} {реакция_1элемента} {реакция_2элемента(условие1,условие2)} } {} {} ...
		Dim console As String = ""
		Dim printArray As Array[Array[String]]

		'печатаем текущий префикс
		console &= "prefix = " & prefix & "\n----------------------\n"
		'печатаем ПЕРВЫЙ-TAKE массив автоубирания
		printArray = (Array[Array[String]])(Scene.Map[prefix & "AUTOTAKEOUTonTAKE"])
		console &= "AUTOTAKEOUTonTAKE\n----------------------\n"
		For i = 0 to printArray.UBound
			console &= "+" & printArray[i][0] & "\n"
			For k = 1 to printArray[i].UBound
				console &= "  └ " & printArray[i][k] & "\n"
				If buttonId == 2 Then
					'если нужно распечатать взаимодействия глубже на уровень
					Dim arrDo As Array[String] = GetArrDoByName(printArray[i][k])
					For t = 0 to arrDo.UBound
						console &= "      └ " & arrDo[t] & "\n"
					Next
				End If
			Next
			console &= "\n"
		Next
		console &= "\n\n"

		'печатаем ВТОРОЙ-TAKEOUT массив автоубирания
		printArray = (Array[Array[String]])(Scene.Map[prefix & "AUTOTAKEOUTonTAKEOUT"])
		console &= "AUTOTAKEOUTonTAKEOUT\n----------------------\n"
		For i = 0 to printArray.UBound
			console &= "-" & printArray[i][0] & "\n"
			For k = 1 to printArray[i].UBound
				console &= "  └ " & printArray[i][k] & "\n"
				If buttonId == 2 Then
					'если нужно распечатать взаимодействия глубже на уровень
					Dim arrDo As Array[String] = GetArrDoByName(printArray[i][k])
					For t = 0 to arrDo.UBound
						console &= "      └ " & arrDo[t] & "\n"
					Next
				End If
			Next
			console &= "\n"
		Next

		this.ScriptPluginInstance.SetParameterString("console", console)
	End If
End Sub

'--------------------------------------------------------------------------------
'--------------------------------------------------------------------------------
'--------------------------------------------------------------------------------
'--------------------------------------------------------------------------------
'--------------------------------------------------------------------------------
'--------------------------------------------------------------------------------
'ДАЛЕЕ пошли вспомогателные функции, для основного алгоритма...
 
 
'функция нахождения вхождения названия элемента в первый элемент основного массива
Function FindIndexInclusion (ARR_ARR As Array[Array[String]], ELEMENT_NAME As String) As Integer
	Dim arr_cur2 As Array[String]
	Dim ii AS Integer
	For ii = 0 to ARR_ARR.UBound
		arr_cur2 = ARR_ARR[ii]
		If arr_cur2[0] = ELEMENT_NAME Then Exit For
	Next
	If ii <= ARR_ARR.UBound Then
		FindIndexInclusion = ii
		Exit Function
	End If
	FindIndexInclusion = -1
End Function
'--------------------------------------------------------------------------------
 
'удаляет повторяющиеся элементы (а зачем раздувать массив и вводить путанницу? :)
Sub RemoveSubDublicate (ARR As Array[String])
	Dim s_curY,s_curZ As String
	For y = 1 to ARR.UBound
		s_curY = ARR[y]
		s_curY.Trim
		ARR[y] = s_curY
		If s_curY = "" OR s_curY = "+" OR s_curY = "-" Then
			ARR.Erase(y)
			y -= 1
		Else
			For z = y+1 to ARR.UBound
				s_curZ = ARR[z]
				s_curZ.Trim()
				If s_curZ = s_curY Then
					ARR.Erase(z)
					z -= 1
				End If
			Next
		End If
	Next
End Sub
'--------------------------------------------------------------------------------
 
'просто добавляет новый элемент в основной массив
Sub AddingNewElements (ARR_ARR As Array[Array[String]], ADD_ARR As Array[String])
	'прежде чем добавлять, упростим добавляемый массив:
	RemoveSubDublicate(ADD_ARR)
	ARR_ARR.Push(ADD_ARR)
End Sub
'----------------------------------------------------------------------------------
 
'эта функция занимается добавлением подъелементов, т.е. того что будет реагировать на первый элемент
Sub AddingNewSubelements (ByRef ARR_ARR As Array[Array[String]], cur_element As Integer, ADD_ARR As Array[String])
	Dim arr_cur As Array[String]
	Dim s_curName3 As String
	Dim i_cur As Integer
	arr_cur = ARR_ARR[cur_element]
	For add_i = 1 to ADD_ARR.UBound
		s_curName3 = ADD_ARR[add_i]
		If s_curName3 <> "" AND s_curName3 <> "+" AND s_curName3 <> "-" Then
			For i_cur = 1 to arr_cur.UBound
				If arr_cur[i_cur] = s_curName3 Then Exit For
			Next
			'если такого элемента не нашлось - надо добавить
			If i_cur > arr_cur.UBound Then
				arr_cur.Push(s_curName3)
			End If
		End If
	Next
	ARR_ARR[cur_element] = arr_cur
End Sub
'--------------------------------------------------------------------------------
 
'добавляет целый новый элемент
Sub AddElements (ARR_ARR As Array[Array[String]], ADD_ARR As Array[Array[String]])
	'println("AddElements | ADD_ARR = " & CStr(ADD_ARR))
	Dim arr_cur As Array[String]
	Dim s_curName As String
	Dim i_curElement1 As Integer
	'---добавляю елементы из временного массива в основной
	For i_add_element = 0 to ADD_ARR.UBound
		'println("ADD_ARR[i_add_element] = " & CStr(ADD_ARR[i_add_element]))
		s_curName = ADD_ARR[i_add_element][0]
		s_curName.trim()
		If s_curName <> "" Then
			
			
			If s_curName.Left(1) == "(" Then
				'если нет названия титра, на который надо реагировать
				'то надо реагировать на перечисления условий...
				Dim arr_curNames As Array[String]
				Dim s_curNames = s_curName.GetSubstring(1, s_curName.Length-2)
				s_curNames.Split("&",arr_curNames)
				For i_name = 0 to arr_curNames.UBound
					arr_curNames[i_name].Trim()
					'println("arr_curNames[i_name] = " & arr_curNames[i_name])
					'ADD_ARR[i_add_element] = {Logo_the} {-Marker_the}
					If arr_curNames[i_name].right(5) == "_fill" then
						arr_curNames[i_name] = arr_curNames[i_name].Left(arr_curNames[i_name].Length-5)
					End If
					
					Dim arr_new As Array[String]
					arr_new.Push(arr_curNames[i_name] & s_curName)
					arr_new.Push(ADD_ARR[i_add_element][1])
					Dim arrarr_new As Array[Array[String]]
					arrarr_new.Push(arr_new)
					if arr_curNames[i_name].left(1)=="-" then
						arrarr_new[0][0] = arrarr_new[0][0].right(arrarr_new[0][0].Length-1)
						Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"] = arrarr_new
					else
						if arr_curNames[i_name].left(1)=="+" then arrarr_new[0][0] = arrarr_new[0][0].right(arrarr_new[0][0].Length-1)
						'println("")
						'println("+arrarr_new[0][0] and [1] = " & arrarr_new[0][0] & " | " & arrarr_new[0][1])
						Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = arrarr_new
					end if
				Next
			Else
				'если есть имя титра...
				'ищем этот титр в финальном массиве
				i_curElement1 = FindIndexInclusion(ARR_ARR,s_curName)
				'println("- just add  ADD_ARR[i_add_element] = " & CStr(ADD_ARR[i_add_element]))
				If i_curElement1 < 0 Then
					'если надо добавить новый элемент
					AddingNewElements(ARR_ARR,ADD_ARR[i_add_element])
				Else
					'если элемент уже есть в основном массиве
					'т.е. добавляем (при необходимости) новые подэлементы
					AddingNewSubelements(ARR_ARR,i_curElement1,ADD_ARR[i_add_element])
				End If
			End If
			
			
		End If
	Next
End Sub
 
'--------------------------------------------------------------------------------
 
'а вот функция автоубирания, согласно поданному массиву значений
Sub Do_AUTOTAKEOUT (arr_DO As Array[String])
	Dim y_index As Integer
	Dim fullName As String
	Dim condis_permission As Boolean
	If arr_DO.UBound < 1 Then exit sub
 
 	'игнорируем нулевой элемент, потому что там причина действия, а не следствие
	For y_index = 1 to arr_DO.UBound
		fullName = arr_DO[y_index]
		fullName.trim()
		
		'смотрим, если ли УСЛОВИЯ этого действия, т.е. написано ли что-нибудь (в скобочках) после имени
		condis_permission = TRUE
		If fullName.Find("(") > 0 AND fullName.Find(")") > 0 Then
			Dim s_condis As String = fullName.GetSubstring(fullName.Find("(")+1,fullName.Find(")") - fullName.Find("(") - 1)
			If CheckCondis(s_condis) == FALSE Then condis_permission = FALSE
 
			'очищаем имя от скобочек и их внутреностей
			fullName = fullName.Left (fullName.Find("("))
			fullName.Trim()
		End If
 
		If condis_permission Then
			If fullName.Left(1) = "+" Then
				'счищаем уже ненужный плюс
				fullName = fullName.Right(fullName.Length - 1)
				
				If CInt(Scene.Map[fullName & "_status"]) <> 1 Then
					'заранее статус у выдачи менять НЕЛЬЗЯ - потому что не всегда титр точно выдасться
					'так ли это?????
					Scene.Map[fullName & "_status"] = 1
					Scene.Map[fullName & "_control"] = 5
					Scene.Map[fullName & "_control"] = 1
				End If
			Else
				
				If fullName.Left(1) = "-" Then fullName = fullName.Right(fullName.Length - 1)
	 
				If CInt(Scene.Map[fullName & "_status"]) <> 0 Then
					'а вот сменить статус на убираение МОЖНО - потому что титр ВСЕГДА убирается наверняка
					Scene.Map[fullName & "_status"] = 0
					Scene.Map[fullName & "_control"] = 5
					Scene.Map[fullName & "_control"] = 0
				End If
			End If
		End If
		'println("Status "&fullName&" = " & System.Map[fullName & "_status"])
	Next
End Sub
 
Function CheckCondis(s_conditions As String) As Boolean
	Dim arr_condis As Array[String]
	Dim s_condisItem As String
	Dim s_condisFill As String
	s_conditions.split("&",arr_condis)
 
	For i_condis = 0 to arr_condis.UBound
		s_condisItem = arr_condis[i_condis]
		s_condisItem.Trim()
 
 
		If s_condisItem.Find("_fill") > 0 Then
			'начало обработки fill
			's_condisItem = s_condisItem.Left(s_condisItem.Length - 5)
 
			If s_condisItem.Left(1) = "+" Then
				s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				s_condisFill = CStr(System.Map[s_condisItem])
				s_condisFill.Trim()
				If s_condisFill == "" Then
					'условие не соблюдено! ыыы...
					CheckCondis = FALSE
					Exit Function
				ElseIf s_condisItem.Right(5) <> "_fill" AND s_condisFill == "0" Then
					'условие не соблюдено! ыыы...
					CheckCondis = FALSE
					Exit Function
				End If
			Else
				If s_condisItem.Left(1) = "-" Then s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				s_condisFill = CStr(System.Map[s_condisItem])
				s_condisFill.Trim()
				If s_condisItem.Right(5) <> "_fill" AND s_condisFill <> "0" Then
					'условие не соблюдено! ыыы...
					CheckCondis = FALSE
					Exit Function
				ElseIf s_condisFill <> "" Then
					'условие не соблюдено! ыыы...
					CheckCondis = FALSE
					Exit Function
				End If
			End If
			'конец обработки FILL
		Else
			'начало обработки STATUS
			If s_condisItem.Left(1) = "+" Then
				s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				If CInt(Scene.Map[s_condisItem & "_status"]) <> 1 Then
					'условие не соблюдено! ыыы...
					CheckCondis = FALSE
					Exit Function
				End If
			Else
				If s_condisItem.Left(1) = "-" Then s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				If CInt(Scene.Map[s_condisItem & "_status"]) <> 0 Then
					'условие не соблюдено! ыыы...
					CheckCondis = FALSE
					Exit Function
				End If
			End If
			'конец обработки STATUS
		End If
	Next
 
	CheckCondis = TRUE
End Function

Function GetArrDoByName(curName As String) As Array[String]
	Dim arrDo As Array[String]
	
		'println("GetArrDoByName. curName = " & curName)
	Dim arrArrDo As Array[Array[String]]
	If curName.Left(1) == "+" Then 
		arrArrDo = (Array[Array[String]]) Scene.Map[prefix & "AUTOTAKEOUTonTAKE"]
		curName = curName.Right(curName.Length-1)
	ElseIf curName.Left(1) == "-" Then
		arrArrDo = (Array[Array[String]]) Scene.Map[prefix & "AUTOTAKEOUTonTAKEOUT"]
		curName = curName.Right(curName.Length-1)
	Else
		arrArrDo = (Array[Array[String]]) Scene.Map[prefix & "AUTOTAKEOUTonTAKE"]
	End If
	If curName.Find("(") > 0 Then curName = curName.Left (curName.Find("("))
	
	Dim nameFromArray As String
	For i = 0 to arrArrDo.UBound
		nameFromArray = (arrArrDo[i])[0]

		'Обрезаем лишние тут скобочные условия
		If nameFromArray.Find("(") > 0 Then nameFromArray = nameFromArray.Left (nameFromArray.Find("("))
		nameFromArray.Trim()
			
		'если это искомый титр:
		'println("nameFromArray = "&nameFromArray & "   curName = " & curName)
		If nameFromArray == curName Then
			For k = 1 to arrArrDo[i].UBound
				arrDo.Push(arrArrDo[i][k])
			Next
		End If
	Next

	GetArrDoByName = arrDo
End Function
'--------------------------------------------------------------------------------

Dim previous_mapKey As String
Sub OnSharedMemoryVariableChanged (map As SharedMemory, mapKey As String)
	Dim arrarr_null As Array[Array[String]]
	Dim AUTOTAKEOUTonTAKE As Array[Array[String]]
	Dim AUTOTAKEOUTonTAKEOUT As Array[Array[String]]
	Dim AddTo_AUTOTAKEOUTonTAKE As Array[Array[String]]
	Dim AddTo_AUTOTAKEOUTonTAKEOUT As Array[Array[String]]
	
	if mapKey.left(7) == "SHMCLIP" then
		'игнорировать переменные связанные с ClipChannel'ами
		exit sub
	end if
 
	
	if GetParameterBool("print_in_console") then
		if mapKey <> previous_mapKey then
			if mapKey.left(mapKey.find("_")) <> previous_mapKey.left(mapKey.find("_")) then
				println("")
				println("======================")
			else
				println("_")
			end if
		end if
		println("map = " & CStr(map))
		println("changing: "&mapKey&" = " & map[mapKey])
		previous_mapKey = mapKey
	end if
	
 
	If mapKey = prefix & "AddTo_AUTOTAKEOUTonTAKE" Then
	'---очищаю рабочие временые массивы
		AUTOTAKEOUTonTAKE.Clear()
		AddTo_AUTOTAKEOUTonTAKE.Clear()
	'---если во временном массиве ничего не пришло - считать вызов фейковым
		If Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = "" Then
			Exit Sub
		End If
	'---проверяю на наличие глобального массива
	'---если надо создаю его
		If Scene.Map.ContainsKey(prefix & "AUTOTAKEOUTonTAKE") = false Then
			Scene.Map[prefix & "AUTOTAKEOUTonTAKE"] = AUTOTAKEOUTonTAKE
		End If
	'---получаю существующий рабочий массив
	'---и временно-добавочный
		AUTOTAKEOUTonTAKE = (Array[Array[String]]) Scene.Map[prefix & "AUTOTAKEOUTonTAKE"]
		AddTo_AUTOTAKEOUTonTAKE = (Array[Array[String]]) Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKE"]

		AddElements(AUTOTAKEOUTonTAKE,AddTo_AUTOTAKEOUTonTAKE)
		If AUTOTAKEOUTonTAKE <> arrarr_null Then
			Scene.Map[prefix & "AUTOTAKEOUTonTAKE"] = AUTOTAKEOUTonTAKE
			Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = ""
		End If
		
		
	ElseIf mapKey = prefix & "AddTo_AUTOTAKEOUTonTAKEOUT" Then
	'---очищаю рабочие временые массивы
		AUTOTAKEOUTonTAKEOUT.Clear()
		AddTo_AUTOTAKEOUTonTAKEOUT.Clear()
	'---если во временном массиве ничего не пришло - считать вызов фейковым
		If Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"] = "" Then Exit Sub
	'---проверяю на наличие глобального массива
	'---если надо создаю его
		If Scene.Map.ContainsKey(prefix & "AUTOTAKEOUTonTAKEOUT") = false Then
			Scene.Map[prefix & "AUTOTAKEOUTonTAKEOUT"] = AUTOTAKEOUTonTAKEOUT
		End If
	'---получаю существующий рабочий массив
	'---и временно-добавочный
		AUTOTAKEOUTonTAKEOUT = (Array[Array[String]]) Scene.Map[prefix & "AUTOTAKEOUTonTAKEOUT"]
		AddTo_AUTOTAKEOUTonTAKEOUT = (Array[Array[String]]) Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"]

		AddElements(AUTOTAKEOUTonTAKEOUT,AddTo_AUTOTAKEOUTonTAKEOUT)
		If AUTOTAKEOUTonTAKEOUT <> arrarr_null Then
			Scene.Map[prefix & "AUTOTAKEOUTonTAKEOUT"] = AUTOTAKEOUTonTAKEOUT
			Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"] = ""
		End If
 
	ElseIf mapKey = prefix & "AUTOTAKEOUT_ALL_RECALCULATE" then
		If Scene.Map[prefix & "AUTOTAKEOUT_ALL_RECALCULATE"] <> "" Then
			'println("----------")
			println("RELOAD ALL LOGIC-SCRIPTS")
			'println("----------")
			OnInitParameters()
		End If
	Else
		Dim curChangedName as String = mapKey
		Dim nameFromArray, fullName As String
		Dim s_condis As String
		Dim curStatus As Integer
 
 		'реагируем лишь на чьё-либо действие
		If curChangedName.Right(8) == "_control" Then
			'5 всегда игнорируем
			'-1 (безусловное убирание) тоже игнорируем
			If map[mapKey] == 5 Then exit sub
			If map[mapKey] == -1 Then exit sub
			
			curChangedName = curChangedName.Left(curChangedName.Length - 8)
			'println("curChangedName = " & curChangedName)
			If map[mapKey] == "1" Then
				'-----------------------------------------------------------------------------------------------	
				AUTOTAKEOUTonTAKE = (Array[Array[String]]) Scene.Map[prefix & "AUTOTAKEOUTonTAKE"]
				For i_i = 0 to AUTOTAKEOUTonTAKE.UBound
					nameFromArray = (AUTOTAKEOUTonTAKE[i_i])[0]
					fullName 	  = (AUTOTAKEOUTonTAKE[i_i])[0]

					'Обрезаем лишние тут скобочные условия
					If nameFromArray.Find("(") > 0 Then nameFromArray = nameFromArray.Left (nameFromArray.Find("("))
					nameFromArray.Trim()
 					
 					'если это искомый титр:
					If nameFromArray == curChangedName Then
						'println("curChangedName control --- 1 nameFromArray == curChangedName")
						'сначала-заранее меняем статус, но на всякий случай сохраняем текущий
						curStatus = CInt(System.Map[curChangedName & "_status"])
						Scene.Map[curChangedName & "_status"] = 1
						
						'смотрим, есть ли УСЛОВИЯ этого события, т.е. написано ли что-нибудь (в скобочках) после имени
						If fullName.Find("(") > 0 AND fullName.Find(")") > 0 Then
							s_condis = fullName.GetSubstring(fullName.Find("(")+1,fullName.Find(")") - fullName.Find("(") - 1)
							If CheckCondis(s_condis) == FALSE Then
								'по условиям не прошло, значит надо вернуть текущий статус и нафиг выйти.
								Scene.Map[curChangedName & "_status"] = curStatus
								exit sub
							End If
						End If
 
						Do_AUTOTAKEOUT (AUTOTAKEOUTonTAKE[i_i])	
					End If
				Next

			ElseIf map[mapKey] = "0" Then
				'println("curChangedName control --- 0")
				'-----------------------------------------------------------------------------------------------	
				AUTOTAKEOUTonTAKEOUT = (Array[Array[String]]) Scene.Map[prefix & "AUTOTAKEOUTonTAKEOUT"]
				For i_i = 0 to AUTOTAKEOUTonTAKEOUT.UBound
					nameFromArray = (AUTOTAKEOUTonTAKEOUT[i_i])[0]
					fullName 	  = (AUTOTAKEOUTonTAKEOUT[i_i])[0]

					'Обрезаем лишние тут скобочные условия
					If nameFromArray.Find("(") > 0 Then nameFromArray = nameFromArray.Left (nameFromArray.Find("("))
					nameFromArray.Trim()
 					
 					'если это искомый титр:
					If nameFromArray == curChangedName Then
						'сначала-заранее меняем статус, но на всякий случай сохраняем текущий
						curStatus = CInt(Scene.Map[curChangedName & "_status"])
						map[curChangedName & "_status"] = 0
						
						'смотрим, есть ли УСЛОВИЯ этого события, т.е. написано ли что-нибудь (в скобочках) после имени
						If fullName.Find("(") > 0 AND fullName.Find(")") > 0 Then
							s_condis = fullName.GetSubstring(fullName.Find("(")+1,fullName.Find(")") - fullName.Find("(") - 1)
							If CheckCondis(s_condis) == FALSE Then
								'по условиям не прошло, значит надо вернуть текущий статус и нафиг выйти.
								Scene.Map[curChangedName & "_status"] = curStatus
								exit sub
							End If
						End If
 
						Do_AUTOTAKEOUT (AUTOTAKEOUTonTAKEOUT[i_i])	
					End If
				Next

			End If
		End If
	End If
End Sub
