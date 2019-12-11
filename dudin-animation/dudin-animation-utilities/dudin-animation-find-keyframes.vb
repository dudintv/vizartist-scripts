' Author: Dmitry Dudin, dudin.tv

' FindKeyframes — function return all keyframes with particular name in a director

Function FindKeyframes(_dir_name As String, _keyframe_name As String) As Array[Keyframe]
	Dim _arr_k As Array[Keyframe]
	if Stage.FindDirector(_dir_name) <> null then
		Stage.FindDirector(_dir_name).GetKeyframes(_arr_k)
		for i=0 to _arr_k.ubound
			if _arr_k[i].name <> _keyframe_name then _arr_k.Erase(i)
		next
	end if
	FindKeyframes = _arr_k
End Function

' USING: 
' Dim arr_k As Array[Keyframe] = FindKeyframes("Shift", "loop_end")
