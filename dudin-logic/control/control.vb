Dim Info As String = "Разработчик: Дудин Дмитрий
Версия 0.3 (20 августа 2018)
-------------------------------------------------------
Скрипт превращает сцену с DudinLogic в сцену поддерживающую
предпросмотр. Для этого шаблон должен быть изготовлен
импортом этой сцены.

FAST MODE вызывает мгновенные выдачи титров с помощью Titr_control = 4
В случае отключенного FAST MODE титры будут выдаваться нормально по Titr_control = 1
"
sub OnInitParameters()
	RegisterInfoText(Info)
	RegisterParameterBool("fast_mode", "FAST MODE. Control = 4(On) or 1(Off)", TRUE)
end sub

Dim counter As Integer
Dim ars_names, ars_fills As Array[String]
Dim c_name As Container
Dim s_names As String
Dim arc_fills As Array[Container]

sub OnInit()
	c_name = this.GetChildContainerByIndex(0)
	c_name.Geometry.RegisterTextChangedCallback()
	
	arc_fills.Clear()
	for i=1 to this.ChildContainerCount-1
		arc_fills.Push(this.GetChildContainerByIndex(i))
		this.GetChildContainerByIndex(i).Geometry.RegisterTextChangedCallback()
	next
end sub

sub OnGeometryChanged(geom As Geometry)
	s_names = c_name.Geometry.Text
	if s_names <> "" Then
		s_names.Split("|", ars_names)
		for i=0 to ars_names.ubound
			ars_names[i].Trim()
		next
		'println(8, "SHOW PREVIEW " & sName & " = " & sFill)
		'если есть что смотреть - запускаем отсчет
		'типа _.debouncer - чтобы не запускалось в каждом кадре
		'10 - значит, что скрипт ждет 9 кадров перед запуском предпросмотра
		counter = 10
	end if
	
	ars_fills.Clear()
	for i=1 to this.ChildContainerCount-1
		ars_fills.Push(this.GetChildContainerByIndex(i).Geometry.Text)
	next
end sub

sub OnExecPerField()
	if counter > 0 then
		counter -= 1
	elseif counter = 0 then
		TakeOnlyOne()
		counter = -1 'выключить счетчик
	end if
end sub

Sub TakeOnlyOne()
	'сначала убрать все
	System.Map["All_control"] = 5
	System.Map["All_control"] = 0
		
	'потом выдать только то, что надо
	for i=0 to ars_names.ubound
		System.Map[ars_names[i]&"_fill"] = ars_fills[i]
		System.Map[ars_names[i]&"_control"] = 5
		if GetParameterBool("fast_mode") then
			System.Map[ars_names[i]&"_control"] = 4
		else
			System.Map[ars_names[i]&"_control"] = 1
		end if
	next
End Sub
