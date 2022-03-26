Dim jTable as Json

Sub Parse()
	'jTable.Load("")
  jTable.LoadFile("c:/data.json")
	this.geometry.text = CollectDataFromArray(jTable.At(0)["data"], "party id|party name|logo filename|percent")
End Sub

Function CollectDataFromArray(_j As Json, keys As String) as String
	Dim _arr_keys As Array[String]
	keys.Split("|", _arr_keys)
	
	Dim arr_s, arr_line As Array[String]
	
	arr_s.Clear()
	for i=0 to _j.size-1
		arr_line.Clear()
		for k=0 to arr_keys.ubound
			if _j.At(i)[_arr_keys[k]].IsString() then
				arr_line.Push( _j.At(i).GetString(_arr_keys[k]) )
			elseif _j.At(i)[_arr_keys[k]].IsBoolean() then
				arr_line.Push(CStr(_j.At(i).GetBoolean(_arr_keys[k])))
			elseif _j.At(i)[_arr_keys[k]].IsDouble() then
				arr_line.Push(DoubleToString(_j.At(i).GetDouble(_arr_keys[k]), 2))
			elseif _j.At(i)[_arr_keys[k]].IsInteger() then
				arr_line.Push(CStr(_j.At(i).GetInteger(_arr_keys[k])))
			end if
		next
		s.Join(arr_line, "|")
		arr_s.Push(s)
	next
	s.Join(arr_s, "\n")
	CollectDataFromArray = s
End Function
