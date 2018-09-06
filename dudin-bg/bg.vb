Dim info As String = "В случае использования для текст_с_подложкой скрипт кидается на ТЕКСТовый контейнер!
Скрипт постоянно измеряет размеры self-контейнера/потомков и изменяет scaling указанного контейнера по X и Y.
Изменения scaling происходят анимационно с замедлением в конце (см. конец скрипта)
Разработчик: Дудин Дмитрий. Vizart co.     Версия 1.6 (07 сентября 2015)
"
 
Dim fon, child, c_min_x, c_min_y As Container
Dim mode_size, mode, mode_min_x, mode_min_y, mode_shange_fon As Integer
Dim x,y, x_multy, y_multy, x_padding, y_padding, x_min, y_min As Double
Dim pos As Vertex
Dim size, child_size, min_size As Vertex
Dim newSize, v1, v2, vthis1, vthis2 As Vertex
Dim newPosX, newPosY As Double
Dim sizeTreshold, animTreshold As Double
 

Dim arr_s As Array[String]
arr_s.Push("X")
arr_s.Push("Y")
arr_s.Push("XY")
Dim arr_ss As Array[String]
arr_ss.Push("Общий размер")
arr_ss.Push("Макс. из потомков")
arr_ss.Push("Один из потомков")
Dim arr_sss As Array[String]
arr_sss.Push("Число")
arr_sss.Push("Контейнер")
Dim arr_sFonShangeMode As Array[String]
arr_sFonShangeMode.Push("Scaling")
arr_sFonShangeMode.Push("Geometry")

 
sub OnInitParameters()
    RegisterInfoText(info)
	RegisterParameterContainer("fon","Фоновый контейнер:")
	RegisterRadioButton("fon_change_mode", "└ Как изменять его размеры", 0, arr_sFonShangeMode)
	RegisterRadioButton("mode_size", "Брать размеры: ", 0, arr_ss)
	RegisterParameterInt("num_child", "└ Номер потомка (-1=никакой)", 0, -1, 100)
	RegisterRadioButton("mode", "└ Какие размеры учитывать:", 0, arr_s)
	RegisterParameterDouble("x_multy", "   └ Множитель по X", 1.0, 0.0, 10000000.0)
	RegisterParameterDouble("y_multy", "   └ Множитель по Y", 1.0, 0.0, 10000000.0)
	RegisterParameterDouble("x_padding", "   └ Отступ по X", 0.0, -100000.0, 10000000.0)
	RegisterParameterDouble("y_padding", "   └ Отступ по Y", 0.0, -100000.0, 10000000.0)
	
	RegisterRadioButton("x_min_mode", "Минимальный фон по X", 0, arr_sss)
	RegisterParameterDouble("x_min", "└ Мин. X", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("x_min_c", "└ Мин. X")
	RegisterRadioButton("y_min_mode", "Минимальный фон по Y", 0, arr_sss)
	RegisterParameterDouble("y_min", "└ Мин. Y", 0.0, 0.0, 10000000.0)
	RegisterParameterContainer("y_min_c", "└ Мин. Y")
	RegisterParameterBool("hide_by_zero", "Скрыть фон если нул.размер", TRUE)
	RegisterParameterDouble("treshold", "└ Типа-нулевой размер this", 0.1, 0.0, 1000.0)
	RegisterParameterDouble("inertion", "Инерция анимации", 2.0, 1.0, 100.0)
	
	RegisterParameterBool("position_y", "Подстраивать позицию по Y", FALSE)
	RegisterParameterDouble("position_y_shift", "Смещение позиции по Y", 0, -99999, 99999)
end sub
 
sub OnGuiStatus()
	mode_shange_fon = GetParameterInt("fon_change_mode")
	
	mode_size = GetParameterInt("mode_size")
	If mode_size == 0 Then
		SendGuiParameterShow("num_child",HIDE)
	ElseIf mode_size == 1 Then
		SendGuiParameterShow("num_child",HIDE)
	ElseIf mode_size == 2 Then
		SendGuiParameterShow("num_child",SHOW)
	End If
 
	mode = GetParameterInt("mode")
	If mode == 0 Then
		'X
		SendGuiParameterShow("x_multy",SHOW)
		SendGuiParameterShow("y_multy",HIDE)
		SendGuiParameterShow("x_padding",SHOW)
		SendGuiParameterShow("y_padding",HIDE)
		SendGuiParameterShow("x_min_mode",SHOW)
		SendGuiParameterShow("x_min",SHOW)
		SendGuiParameterShow("x_min_c",SHOW)
		SendGuiParameterShow("y_min_mode",HIDE)
		SendGuiParameterShow("y_min",HIDE)
		SendGuiParameterShow("y_min_c",HIDE)
	ElseIf mode == 1 Then
		'Y
		SendGuiParameterShow("x_multy",HIDE)
		SendGuiParameterShow("y_multy",SHOW)
		SendGuiParameterShow("x_padding",HIDE)
		SendGuiParameterShow("y_padding",SHOW)
		SendGuiParameterShow("x_min_mode",HIDE)
		SendGuiParameterShow("x_min",HIDE)
		SendGuiParameterShow("x_min_c",HIDE)
		SendGuiParameterShow("y_min_mode",SHOW)
		SendGuiParameterShow("y_min",SHOW)
		SendGuiParameterShow("y_min_c",SHOW)
	ElseIf mode == 2 Then
		'XY
		SendGuiParameterShow("x_multy",SHOW)
		SendGuiParameterShow("y_multy",SHOW)
		SendGuiParameterShow("x_padding",SHOW)
		SendGuiParameterShow("y_padding",SHOW)
		SendGuiParameterShow("x_min_mode",SHOW)
		SendGuiParameterShow("x_min",SHOW)
		SendGuiParameterShow("x_min_c",SHOW)
		SendGuiParameterShow("y_min_mode",SHOW)
		SendGuiParameterShow("y_min",SHOW)
		SendGuiParameterShow("y_min_c",SHOW)
	End If
	
	If mode == 0 OR mode == 2 Then
		mode_min_x = GetParameterInt("x_min_mode")
		If mode_min_x == 0 Then
			SendGuiParameterShow("x_min",SHOW)
			SendGuiParameterShow("x_min_c",HIDE)
		ElseIf mode_min_x == 1 Then
			SendGuiParameterShow("x_min",HIDE)
			SendGuiParameterShow("x_min_c",SHOW)
		End If
	End If
	
	If mode == 1 OR mode == 2 Then
		mode_min_y = GetParameterInt("y_min_mode")
		If mode_min_y == 0 Then
			SendGuiParameterShow("y_min",SHOW)
			SendGuiParameterShow("y_min_c",HIDE)
		ElseIf mode_min_y == 1 Then
			SendGuiParameterShow("y_min",HIDE)
			SendGuiParameterShow("y_min_c",SHOW)
		End If
	End If
	
	If GetParameterBool("position_y") then
		SendGuiParameterShow("position_y_shift",SHOW)
	Else
		SendGuiParameterShow("position_y_shift",HIDE)
	End if
end sub
 
Function GetLocalSize (c_gabarit as Container, c_fon as Container) As Vertex
	c_gabarit.GetTransformedBoundingBox(v1,v2)
	v1 = c_fon.WorldPosToLocalPos(v1)
	v2 = c_fon.WorldPosToLocalPos(v2)
	GetLocalSize = CVertex(v2.x-v1.x,v2.y-v1.y,v2.z-v1.z)
End Function
 
sub OnExecPerField()
	fon = GetParameterContainer("fon")
	If fon == null Then exit sub
	mode = GetParameterInt("mode")
	sizeTreshold = GetParameterDouble("treshold")/100.0
	mode_shange_fon = GetParameterInt("fon_change_mode")
	
	
	
	'логика режима ("Общий размер", "Максимальный из потомков", "Один из потомков")
	mode_size = GetParameterInt("mode_size")
	If mode_size == 0 Then
		'режим: "Общий размер"
		size = GetLocalSize (this, fon)
	ElseIf mode_size == 1 Then
		'режим: "Максимальный из потомков"
		child = this.FirstChildContainer
		child.RecomputeMatrix()
		size = GetLocalSize (child, fon)
		child = child.NextContainer
		Do While child <> null
			child.RecomputeMatrix()
			child_size = GetLocalSize (child, fon)
			If child_size.X > size.X Then size.X = child_size.X
			If child_size.Y > size.Y Then size.Y = child_size.Y
			If child_size.Z > size.Z Then size.Z = child_size.Z
			child = child.NextContainer
		Loop 
	ElseIf mode_size == 2 Then
		'режим: "Один из потомков"
		child = GetChildContainerByIndex(GetParameterInt("num_child"))
		child.RecomputeMatrix()
		size = GetLocalSize (child, fon)
	End If
 
 
	x_multy = GetParameterDouble("x_multy")
	y_multy = GetParameterDouble("y_multy")
	x_padding = GetParameterDouble("x_padding")
	y_padding = GetParameterDouble("y_padding")
	c_min_x = GetParameterContainer("x_min_c")
	c_min_y = GetParameterContainer("y_min_c")
	
	If mode_min_x == 1 Then
		'надо брать минимальный размер у контейнера
		If c_min_x <> null Then
			min_size = GetLocalSize (c_min_x, fon)
			If min_size.X > sizeTreshold AND min_size.Y > sizeTreshold Then
				x_min = min_size.X/100.0
			Else
				x_min = 0
			End If
		Else
			x_min = 0
		End If
	Else
		'численный минимальный размер
		x_min = GetParameterDouble("x_min")
	End If
	
	If mode_min_y == 1 Then
		'надо брать минимальный размер у контейнера
		If c_min_y <> null Then
			min_size = GetLocalSize (c_min_y, fon)
			y_min = min_size.Y/100.0
			If min_size.X > sizeTreshold AND min_size.Y > sizeTreshold Then
				y_min = min_size.Y/100.0
			Else
				y_min = 0
			End If
		Else
			y_min = 0
		End If
	Else
		'численный минимальный размер
		y_min = GetParameterDouble("y_min")
	End If
	
	
	
	'-------------------------------------------------------------------------------------------------
	'обработаем ширину
	If mode == 0 OR mode == 2 Then
		If size.X < sizeTreshold Then
			'если ширина отсутсвует то скрыть фон
			If GetParameterBool("hide_by_zero") Then
				fon.Active = false
				exit sub
			Else
				fon.Active = true
				newSize.X = 0
			End If
		Else
			fon.Active = true
			x = size.X/100.0 * x_multy + x_padding/10.0
			If x < x_min Then x = x_min
			newSize.X = x
		End If
	End If
 
	'обработаем высоту
	If mode == 1 OR mode == 2 Then
		If size.Y < sizeTreshold Then
			'если высота отсутсвует то скрыть фон
			If GetParameterBool("hide_by_zero") Then
				fon.Active = false
				exit sub
			Else
				fon.Active = true
				newSize.Y = 0
			End If
		Else
			fon.Active = true
			y = size.Y/100.0 * y_multy + y_padding/100.0
			If y < y_min Then y = y_min
			newSize.Y = y
		End If
	End If
	
	'-------------------------------------------------------------------------------------------------
	'позиционирование
	if GetParameterBool("position_y") then
		this.GetTransformedBoundingBox(vthis1,vthis2)
		vthis2 = this.LocalPosToWorldPos(vthis2)
		vthis2 = fon.WorldPosToLocalPos(vthis2)
		fon.GetTransformedBoundingBox(v1,v2)
		fon.position.y = vthis2.y - (v2.y-fon.position.y) + GetParameterDouble("position_y_shift")
	end if
 	
 
	'-------------------------------------------------------------------------------------------------
	'анимация и реальное изменение размеров
 
	animTreshold = 0.001
	
	if mode_shange_fon == 1 then
		newSize.x = 100*newSize.x
		newSize.y = 100*newSize.y
	end if
	
	if mode == 0 OR mode == 2 Then
		'X
		if mode_shange_fon == 0 then
			if newSize.x<(fon.scaling.x-animTreshold) OR newSize.x>(fon.scaling.x+animTreshold) Then
				fon.scaling.x -= (fon.scaling.x - newSize.x)/GetParameterDouble("inertion")
			else
				fon.scaling.x = newSize.x
			end if
		elseif mode_shange_fon == 1 then
			if newSize.x<(fon.geometry.GetParameterDouble("width")-animTreshold) OR newSize.x>(fon.geometry.GetParameterDouble("width")+animTreshold) Then
				fon.geometry.SetParameterDouble(  "width", fon.geometry.GetParameterDouble("width") - (fon.geometry.GetParameterDouble("width") - newSize.x)/GetParameterDouble("inertion")  )
			else
				fon.geometry.SetParameterDouble(  "width", newSize.x  )
			end if
		end if
	end if
	if mode == 1 OR mode == 2 Then
		'Y
		if mode_shange_fon == 0 then
			if newSize.y<(fon.scaling.y-animTreshold) OR newSize.y>(fon.scaling.y+animTreshold) Then
				fon.scaling.y -= (fon.scaling.y - newSize.y)/GetParameterDouble("inertion")
			else
				fon.scaling.y = newSize.y
			end if
		elseif mode_shange_fon == 1 then
			if newSize.y<(fon.geometry.GetParameterDouble("height")-animTreshold) OR newSize.y>(fon.geometry.GetParameterDouble("height")+animTreshold) Then
				fon.geometry.SetParameterDouble(  "height", fon.geometry.GetParameterDouble("height") - (fon.geometry.GetParameterDouble("height") - newSize.y)/GetParameterDouble("inertion")  )
			else
				fon.geometry.SetParameterDouble(  "height", newSize.y  )
			end if
		end if
	end if
	
	
	'-------------------------------------------------------------------------------------------------
	fon.RecomputeMatrix()
End Sub