Dim info As String = "
Скприт подстраивается под внешние габариты и должен регулировать:
1. ширину блока типа как MaxSize, но без тормозов
2. анимацию скроллинга по всей своей поверхности в пределах фона
Версия 0.5 (18 апреля 2018)
"

Dim fon As Container
'Dim screen1, screen2 As Vertex
'Dim old_screen1, old_screen2 As Vertex
Dim v1, v2, old_v1, old_v2, v_local1, v_local2 As Vertex
Dim v_me1, v_me2, old_v_me1, old_v_me2 As Vertex
Dim paddingX, paddingY, old_paddingX, old_paddingY, new_x, new_y, new_width, new_height As Double
Dim meSize, targetSize, newScaleX, newScaleY, minScale, maxScale As Double
Dim scroll, old_scroll As Double
Dim timeStart, timeEnd As Double
Dim chScroll As Channel
Dim pMax As PluginInstance
Dim geom As Geometry
Dim liveMode, inverseAnim As Boolean
Dim lessMode, moreMode As Integer
Dim keyStart, keyEnd As Keyframe

'for inertion
Dim target_y, inertion As Double
Dim treshold As Double = 0.01

Dim arrMode As Array[String]
arrMode.Push("Наверху")
arrMode.Push("По центру")
arrMode.Push("Внизу")
arrMode.Push("По проценту")

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterContainer("fon", "Фоновый контейнер:")
	RegisterParameterDouble("paddingX", "Смещение границ X", 0.0, -1000.0, 1000.0)
	RegisterParameterDouble("paddingY", "Смещение границ Y", 0.0, -1000.0, 1000.0)
	RegisterParameterDouble("min_scale", "Мин. scale", 0, 0, 999999.0)
	RegisterParameterDouble("max_scale", "Макс. scale", 999999.0, 0, 999999.0)
	RegisterParameterBool("live_mode", "Постоянный режим", true)
	RegisterPushButton("set_up", "Показать верх", 1)
	RegisterPushButton("set_down", "Показать низ", 2)
	RegisterPushButton("show_percent", "Показать на процент", 3)
	RegisterParameterDouble("scroll", "Процент скролла", 0.0, -100.0, 200.0)
	RegisterParameterBool("inverse_anim", "Инвертировать направление анимации?", true)
	RegisterRadioButton("less_mode", "Когда высота меньше", 1, arrMode)
	RegisterRadioButton("more_mode", "Когда высота больше", 1, arrMode)
	RegisterParameterDouble("time_start_scroll", "Начало скролла (сек)", 0, 0, 100000.0)
	RegisterParameterDouble("time_period_scroll", "Длительность скролла (сек)", 5.0, 0, 100000.0)
	RegisterPushButton("create_keys", "Сделать ключи анимации", 4)
	RegisterParameterDouble("inertion", "Инерция", 0.0, 0, 1000.0)
end sub

sub OnInit()
	fon = GetParameterContainer("fon")
	pMax = this.GetFunctionPluginInstance("Maxsize")
	liveMode = GetParameterBool("live_mode")
	lessMode = GetParameterInt("less_mode")
	moreMode = GetParameterInt("more_mode")
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
	PrepareAndPosX()
	SetPosY()
	minScale = GetParameterDouble("min_scale")
	maxScale = GetParameterDouble("max_scale")
	inverseAnim = GetParameterBool("inverse_anim")
end sub


'-----------------------------------------------------------------


sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		PrepareAndPosX()
		SetByPercent(0)
	elseif buttonId == 2 then
		PrepareAndPosX()
		SetByPercent(1)
	elseif buttonId == 3 then
		PrepareAndPosX()
		SetByPercent(scroll)
	elseif buttonId == 4 then
		CreateKeys()
	end if
end sub


Sub PrepareAndPosX()
	fon.RecomputeMatrix()
	this.RecomputeMatrix()
	
	fon.GetTransformedBoundingBox(v1, v2)
	this.GetBoundingBox(v_me1, v_me2)
	paddingX = GetParameterDouble("paddingX")
	paddingY = GetParameterDouble("paddingY")
	
	v_local1 = this.WorldPosToLocalPos(v1)
	v_local2 = this.WorldPosToLocalPos(v2)
	new_width  = v_local2.x - v_local1.x
	new_height = v_local2.y - v_local1.y
	
	If new_width > 0.001 AND new_height > 0.001 Then
		'установка ширины
		'pMax.SetParameterDouble("WIDTH_X",new_width + paddingX)
		targetSize = v_local2.x - v_local1.x
		meSize = v_me2.x - v_me1.x
		
		newScaleX = targetSize/(meSize - paddingX*10.0)
		newScaleY = newScaleX
		if newScaleX > maxScale then newScaleX = maxScale
		if newScaleY > maxScale then newScaleY = maxScale
		if newScaleX < minScale then newScaleX = minScale
		if newScaleY < minScale then newScaleY = minScale
		this.scaling.x = newScaleY
		this.scaling.y = newScaleY
		'установка позиции по X
		this.Position.X = v_local1.x - v_me1.x*this.scaling.x - this.scaling.x*paddingX*10.0/2.0
	End If
End Sub

Function IsNearToTarget() As Boolean
	IsNearToTarget = this.Position.Y > (target_y - treshold) AND this.Position.Y < (target_y + treshold)
End Function

Sub SetByPercent(scrl as double)
	'scrl = [0..1]
	this.RecomputeMatrix()
	target_y = (v_local2.y+scrl*(v_local1.y-v_local2.y)) - (v_me2.y+scrl*(v_me1.Y-v_me2.Y))*this.Scaling.Y - paddingY*(scrl-0.5)
	
	if IsNearToTarget() then
		'set to target
		this.Position.Y = target_y
		target_y = this.Position.Y
	else
		'inertion animation
		this.Position.y += (target_y - this.Position.y)/(1.0 + GetParameterDouble("inertion"))
	end if
End Sub

Sub SetPosy()
	targetSize = v_local2.y - v_local1.y + paddingY
	meSize = (v_me2.y - v_me1.y)*this.scaling.y
	
	if meSize > targetSize then
		select case moreMode
		case 0
			SetByPercent(0)
		case 1
			SetByPercent(0.5)
		case 2
			SetByPercent(1)
		case 3
			SetByPercent(scroll)
		end select
	else
		select case lessMode
		case 0
			SetByPercent(0)
		case 1
			SetByPercent(0.5)
		case 2
			SetByPercent(1)
		case 3
			SetByPercent(scroll)
		end select
	end if
End Sub

Sub CreateKeys()
	timeStart = GetParameterDouble("time_start_scroll")
	timeEnd = timeStart + GetParameterDouble("time_period_scroll")


	chScroll = this.FindOrCreateChannelOfObject("scroll")
	if chScroll <> null then chScroll.Delete()
	chScroll = this.ScriptPluginInstance.FindOrCreateChannelOfObject("scroll")
	
	keyStart = chScroll.AddKeyframe(timeStart)
	keyEnd   = chScroll.AddKeyframe(timeEnd)
	if inverseAnim then
		keyStart.FloatValue = 100
		keyEnd.FloatValue = 0
	else
		keyStart.FloatValue = 0
		keyEnd.FloatValue = 100
	end if
End Sub

sub OnExecPerField()
	If fon == null OR liveMode == false Then Exit Sub
	
	fon.GetTransformedBoundingBox(v1, v2)
	this.GetBoundingBox(v_me1, v_me2)
	paddingX = GetParameterDouble("paddingX")
	paddingY = GetParameterDouble("paddingY")
	scroll = GetParameterDouble("scroll")/100.0
	
	If v1 == old_v1 AND v2 == old_v2 AND paddingX == old_paddingX AND paddingY == old_paddingY AND v_me1 == old_v_me1 AND v_me2 == old_v_me2 AND old_scroll == scroll AND this.position.y == target_y Then
		Exit Sub
	End if
	
	PrepareAndPosX()
	SetPosY()
	
	old_v_me1 = v_me1
	old_v_me2 = v_me2
	old_v1 = v1
	old_v2 = v2
	old_paddingX = paddingX
	old_paddingY = paddingY
	old_scroll = scroll
end sub
