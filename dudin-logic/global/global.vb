RegisterPluginVersion(4,0,2)
Dim Info As String = "Developer: Dmitry Dudin
http://dudin.tv/scripts/logic

This is the Global Script for the Dudin Logic.
It should be placed ABOVE all the Element Scripts.

It is a vital part of AutoTakeout logic. 
If you do not need this interaction across Elements — you can use only Element Scripts, without this Global.

Also, you can check the content main arrays. Press \"PRINT\" button.
"

' You can use the prefix to distinguish two Dudin Logic systems on one machine.
' The prefix can be any. Keep in mind, that it should be identical to the corresponding Element Scripts.
Dim prefix = ""

sub OnInitParameters()
	'by default these main arrays should be empty
	System.Map[prefix & "AUTOTAKEOUTonTAKE"] = ""
	System.Map[prefix & "AUTOTAKEOUTonTAKEOUT"] = ""
	System.Map[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = ""
	System.Map[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"] = ""	
	
	Scene.Map[prefix & "AUTOTAKEOUTonTAKE"] = ""
	Scene.Map[prefix & "AUTOTAKEOUTonTAKEOUT"] = ""
	Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = ""
	Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"] = ""	
	
	RegisterPushButton("print","print",1)
	RegisterPushButton("printdeep","deeper",2)
	RegisterParameterText("console", "", 600, 300)
	RegisterInfoText(Info)
	RegisterParameterBool("print_in_console", "Print in console", FALSE)
end sub
 
Sub OnInit()
	'let's capture ALL SharedMemory variables by using ""(empty string)
	'because this script should be able to react to any variable
	System.Map.RegisterChangedCallback("")
	Scene.Map.RegisterChangedCallback("")
 
	'force to reinitialize all Element Scripts below in the scene
	Scene.Map[prefix & "AUTOTAKEOUT_ALL_RECALCULATE"] = ""
	Scene.Map[prefix & "AUTOTAKEOUT_ALL_RECALCULATE"] = "GLOBAL SCRIPT START RELOAD ALL ELEMENTS" ' just a random string to trigger the variable
 
	this.ScriptPluginInstance.SetParameterString("console", Info)
End Sub
 
'--------------------------------------------------------------------------------
Sub OnExecAction(buttonId As Integer)
	If buttonId == 1 OR buttonId == 2 Then
		'print out both main arrays in the format:
		'{{action event} {first-element-reaction} {second-element-reaction(first-condtion,second-condition)} } {} {} ...
		Dim console As String = ""
		Dim printArray As Array[Array[String]]

		'print the prefix out
		console &= "prefix = " & prefix & "\n----------------------\n"
		'print the first main "TAKE" array
		printArray = (Array[Array[String]])(Scene.Map[prefix & "AUTOTAKEOUTonTAKE"])
		console &= "AUTOTAKEOUTonTAKE\n----------------------\n"
		For i = 0 to printArray.UBound
			console &= "+" & printArray[i][0] & "\n"
			For k = 1 to printArray[i].UBound
				console &= "  └ " & printArray[i][k] & "\n"
				If buttonId == 2 Then
					'if we want to print out more nested dependency
					Dim arrDo As Array[String] = GetArrDoByName(printArray[i][k])
					For t = 0 to arrDo.UBound
						console &= "      └ " & arrDo[t] & "\n"
					Next
				End If
			Next
			console &= "\n"
		Next
		console &= "\n\n"

		'print out the second main "TAKEOUT" array
		printArray = (Array[Array[String]])(Scene.Map[prefix & "AUTOTAKEOUTonTAKEOUT"])
		console &= "AUTOTAKEOUTonTAKEOUT\n----------------------\n"
		For i = 0 to printArray.UBound
			console &= "-" & printArray[i][0] & "\n"
			For k = 1 to printArray[i].UBound
				console &= "  └ " & printArray[i][k] & "\n"
				If buttonId == 2 Then
					'if we want to print out more nested dependency
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
'BELOW there are helper functions
 
'the function of finding the occurrence index of the Element name in the first main array
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
 
'remove duplicated Elements to avoid increasing the array
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
 
'add new Elements in one of the main arrays
Sub AddingNewElements (_mainArray As Array[Array[String]], _addArray As Array[String])
	'remove duplicates before adding
	RemoveSubDublicate(_addArray)
	_mainArray.Push(_addArray)
End Sub
'----------------------------------------------------------------------------------

'add sub-Elements for the reaction on the firs Element
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
			'add the Element if it is not found
			If i_cur > arr_cur.UBound Then
				arr_cur.Push(s_curName3)
			End If
		End If
	Next
	ARR_ARR[cur_element] = arr_cur
End Sub
'--------------------------------------------------------------------------------
 
'add new Elements
Sub AddElements (ARR_ARR As Array[Array[String]], ADD_ARR As Array[Array[String]])
	Dim arr_cur As Array[String]
	Dim s_curName As String
	Dim i_curElement1 As Integer
	'add Elements from the temporal array to the main
	For i_add_element = 0 to ADD_ARR.UBound
		s_curName = ADD_ARR[i_add_element][0]
		s_curName.trim()
		If s_curName <> "" Then
			
			
			If s_curName.Left(1) == "(" Then
				'if there is no Element name than let's react on the conditions
				Dim arr_curNames As Array[String]
				Dim s_curNames = s_curName.GetSubstring(1, s_curName.Length-2)
				s_curNames.Split("&",arr_curNames)
				For i_name = 0 to arr_curNames.UBound
					arr_curNames[i_name].Trim()
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
						Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = arrarr_new
					end if
				Next
			Else
				'if there is the Element name
				'find this Element in the final array
				i_curElement1 = FindIndexInclusion(ARR_ARR,s_curName)
				If i_curElement1 < 0 Then
					'if it's needed to add a new Element
					AddingNewElements(ARR_ARR,ADD_ARR[i_add_element])
				Else
					'if the Element already exists then add any new sub-Elements
					AddingNewSubelements(ARR_ARR,i_curElement1,ADD_ARR[i_add_element])
				End If
			End If
			
			
		End If
	Next
End Sub
 
'--------------------------------------------------------------------------------
 
'the main function for AutoTakeout according to the main arrays
Sub Do_AUTOTAKEOUT (arr_DO As Array[String])
	Dim y_index As Integer
	Dim fullName As String
	Dim condis_permission As Boolean
	If arr_DO.UBound < 1 Then exit sub
	
	'ignore "0" index becasuse it has the reason, not a consequence 
	For y_index = 1 to arr_DO.UBound
		fullName = arr_DO[y_index]
		fullName.trim()
		
		'check is there are conditions
		condis_permission = TRUE
		If fullName.Find("(") > 0 AND fullName.Find(")") > 0 Then
			Dim s_condis As String = fullName.GetSubstring(fullName.Find("(")+1,fullName.Find(")") - fullName.Find("(") - 1)
			If CheckCondis(s_condis) == FALSE Then condis_permission = FALSE
 
			'cleanup the name from parentesis and conditions
			fullName = fullName.Left (fullName.Find("("))
			fullName.Trim()
		End If
 
		If condis_permission Then
			If fullName.Left(1) = "+" Then
				'remove useless plus sign
				fullName = fullName.Right(fullName.Length - 1)
				
				If CInt(Scene.Map[fullName & "_status"]) <> 1 Then
					'it's not possible to change the status because it's not entering for sure... need to check...
					Scene.Map[fullName & "_status"] = 1
					Scene.Map[fullName & "_control"] = 5
					Scene.Map[fullName & "_control"] = 1
				End If
			Else
				
				If fullName.Left(1) = "-" Then fullName = fullName.Right(fullName.Length - 1)
	 
				If CInt(Scene.Map[fullName & "_status"]) <> 0 Then
					'but, it's possible to change on "Takeout" because the Element ALWAYS exit
					Scene.Map[fullName & "_status"] = 0
					Scene.Map[fullName & "_control"] = 5
					Scene.Map[fullName & "_control"] = 0
				End If
			End If
		End If
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
			'processing of the _fill
			's_condisItem = s_condisItem.Left(s_condisItem.Length - 5)
 
			If s_condisItem.Left(1) = "+" Then
				s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				s_condisFill = CStr(System.Map[s_condisItem])
				s_condisFill.Trim()
				If s_condisFill == "" Then
					'condition not met
					CheckCondis = FALSE
					Exit Function
				ElseIf s_condisItem.Right(5) <> "_fill" AND s_condisFill == "0" Then
					'condition not met
					CheckCondis = FALSE
					Exit Function
				End If
			Else
				If s_condisItem.Left(1) = "-" Then s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				s_condisFill = CStr(System.Map[s_condisItem])
				s_condisFill.Trim()
				If s_condisItem.Right(5) <> "_fill" AND s_condisFill <> "0" Then
					'condition not met
					CheckCondis = FALSE
					Exit Function
				ElseIf s_condisFill <> "" Then
					'condition not met
					CheckCondis = FALSE
					Exit Function
				End If
			End If
			'end of processing of _fill
		Else
			'processing of _status
			If s_condisItem.Left(1) = "+" Then
				s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				If CInt(Scene.Map[s_condisItem & "_status"]) <> 1 Then
					'condition not met
					CheckCondis = FALSE
					Exit Function
				End If
			Else
				If s_condisItem.Left(1) = "-" Then s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				If CInt(Scene.Map[s_condisItem & "_status"]) <> 0 Then
					'condition not met
					CheckCondis = FALSE
					Exit Function
				End If
			End If
			'end of processing of _status
		End If
	Next
 
	CheckCondis = TRUE
End Function

Function GetArrDoByName(curName As String) As Array[String]
	Dim arrDo As Array[String]
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

		'cut extra parentesis
		If nameFromArray.Find("(") > 0 Then nameFromArray = nameFromArray.Left (nameFromArray.Find("("))
		nameFromArray.Trim()
			
		'if this is the desired Element
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
		'ignore variables belonging to ClipChannels
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
		'cleanup arrays
		AUTOTAKEOUTonTAKE.Clear()
		AddTo_AUTOTAKEOUTonTAKE.Clear()
						
		'if the array is empty then consider it as a fake
		If Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = "" Then
			Exit Sub
		End If

		'check exising the array, create if it's needed
		If Scene.Map.ContainsKey(prefix & "AUTOTAKEOUTonTAKE") = false Then
			Scene.Map[prefix & "AUTOTAKEOUTonTAKE"] = AUTOTAKEOUTonTAKE
		End If

		'get existing arrays
		AUTOTAKEOUTonTAKE = (Array[Array[String]]) Scene.Map[prefix & "AUTOTAKEOUTonTAKE"]
		AddTo_AUTOTAKEOUTonTAKE = (Array[Array[String]]) Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKE"]

		AddElements(AUTOTAKEOUTonTAKE,AddTo_AUTOTAKEOUTonTAKE)
		If AUTOTAKEOUTonTAKE <> arrarr_null Then
			Scene.Map[prefix & "AUTOTAKEOUTonTAKE"] = AUTOTAKEOUTonTAKE
			Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = ""
		End If
		
		
	ElseIf mapKey = prefix & "AddTo_AUTOTAKEOUTonTAKEOUT" Then
		'cleanup arrays
		AUTOTAKEOUTonTAKEOUT.Clear()
		AddTo_AUTOTAKEOUTonTAKEOUT.Clear()
							
		'if the array is empty then consider it as a fake
		If Scene.Map[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"] = "" Then Exit Sub

		'check exising the array, create if it's needed
		If Scene.Map.ContainsKey(prefix & "AUTOTAKEOUTonTAKEOUT") = false Then
			Scene.Map[prefix & "AUTOTAKEOUTonTAKEOUT"] = AUTOTAKEOUTonTAKEOUT
		End If
									
		'get existing arrays
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
 
		'react on a control event
		If curChangedName.Right(8) == "_control" Then
			'5 (reset) is always ignored
			'-1 (unconditional exit) is always ignored
			If map[mapKey] == 5 Then exit sub
			If map[mapKey] == -1 Then exit sub
			
			curChangedName = curChangedName.Left(curChangedName.Length - 8)
			If map[mapKey] == "1" Then
				'-----------------------------------------------------------------------------------------------	
				AUTOTAKEOUTonTAKE = (Array[Array[String]]) Scene.Map[prefix & "AUTOTAKEOUTonTAKE"]
				For i_i = 0 to AUTOTAKEOUTonTAKE.UBound
					nameFromArray = (AUTOTAKEOUTonTAKE[i_i])[0]
					fullName 	  = (AUTOTAKEOUTonTAKE[i_i])[0]

					'cut conditions
					If nameFromArray.Find("(") > 0 Then nameFromArray = nameFromArray.Left (nameFromArray.Find("("))
					nameFromArray.Trim()
 					
 					'if it's wanted Element:
					If nameFromArray == curChangedName Then
						'first, change the _status in advance, save the current just in case
						curStatus = CInt(System.Map[curChangedName & "_status"])
						Scene.Map[curChangedName & "_status"] = 1
						
						'check conditions
						If fullName.Find("(") > 0 AND fullName.Find(")") > 0 Then
							s_condis = fullName.GetSubstring(fullName.Find("(")+1,fullName.Find(")") - fullName.Find("(") - 1)
							If CheckCondis(s_condis) == FALSE Then
								'if the condition is not met — return the current _status
								Scene.Map[curChangedName & "_status"] = curStatus
								exit sub
							End If
						End If
 
						Do_AUTOTAKEOUT (AUTOTAKEOUTonTAKE[i_i])	
					End If
				Next

			ElseIf map[mapKey] = "0" Then
				'-----------------------------------------------------------------------------------------------	
				AUTOTAKEOUTonTAKEOUT = (Array[Array[String]]) Scene.Map[prefix & "AUTOTAKEOUTonTAKEOUT"]
				For i_i = 0 to AUTOTAKEOUTonTAKEOUT.UBound
					nameFromArray = (AUTOTAKEOUTonTAKEOUT[i_i])[0]
					fullName 	  = (AUTOTAKEOUTonTAKEOUT[i_i])[0]

					'cut conditions
					If nameFromArray.Find("(") > 0 Then nameFromArray = nameFromArray.Left (nameFromArray.Find("("))
					nameFromArray.Trim()
 					
 					'if it's wanted Element:
					If nameFromArray == curChangedName Then
						'first, change the _status in advance, save the current just in case
						curStatus = CInt(Scene.Map[curChangedName & "_status"])
						map[curChangedName & "_status"] = 0
						
						'check conditions
						If fullName.Find("(") > 0 AND fullName.Find(")") > 0 Then
							s_condis = fullName.GetSubstring(fullName.Find("(")+1,fullName.Find(")") - fullName.Find("(") - 1)
							If CheckCondis(s_condis) == FALSE Then
								'if the condition is not met — return the current _status
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
