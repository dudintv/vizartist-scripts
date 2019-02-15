RegisterPluginVersion(4,3,4)
Dim info As String = "Developer: Dmitry Dudin
15 february 2019
-------------------------------------------------------
Укажи (через запятую, на пробелы пофиг) какие блоки титров
будет уходить с экрана или наоборот показываться в случаях:

1строка: когда ВЫДАЕТСЯ ЭТОТ_титр в эфир - написать что делать с другими титрами?
2строка: когда УБИРАЕТСЯ ЭТОТ_титр - что делать с другими?
3строка: указать какие титры и при каком событии ВЫДАЮТ ЭТОТ_титр (+ при выдаче, - при убирании)
4строка: указать какие титры и при каком событии УБИРАЮТ ЭТОТ_титр (+ при выдаче, - при убирании)

Знак \"-\" всегда заставляет блок убраться, а \"+\" появиться, 
если не указан знак, по умолчанию, предполагается знак \"-\".

Дополнительно можно указать условия срабатывания [в квадратных скобках]. По умолчанию берется свойство \"_status\", 
но можно указывать другие свойства, например условие выдачи может быть наличие контента \"_fill\".

Примеры:
+Comments[+Comments_fill]
-Comments,-Sinhron,  +OtherMarker

------------------------------------------------
Директор титра дожен называться \"НазваниеТитра\"
У главного директора должно быть 1-2 стопера.
--------------------------------------------------
Серийный режим позволяет выдвать титр по кусочкам (сериями) и зацикленно.
Текст находящийся в переменной ИмяТитра_fill рубится согласну символам Разделителя
и выдается последовательно через опредленную Паузу.
Получается что-то типа \"бегущей строки\"... :)
----------------------------------------------------
Если \"Всегда начинать с первой\" в значении ВЫКЛ - значит:
если титр получил тот же текст, что в последний раз 
он начинает выдачу со следующей серии, после которой был убран!
А если ВКЛ - то титр всегда будет начинать с первой серии.

Если новый титр с другим fill — выдача пойдет с первой серии в любом случае!
"
 
 
'--------------------------------------------------------------------------------------
'определение локальных переменных

'Разный префикс нужен для одновременной независимой работы двух титровальных систем
'например для сцены "ньюсбара" и сцены "титров"
'Он может быть произвольным. Главное чтобы совпадал с префиксом в элементных скриптах.
Dim prefix = ""
Dim memory As SharedMemory = System.Map
Dim local_memory As SharedMemory = Scene.Map

Dim console As String           'сюда можно сливать текст для консоли
 
Dim titr_name As String         'имя титра
Dim separator As String         'разделительный символ / символы
Dim mode      As Integer        '0 - если обычный однотитровый, 1 - если серийный режим
Dim testers   As Integer        '0 - кнопки тестирования НЕ видны, 1 - кнопки видны
Dim firster   As Integer        '0 - начинать серии не обязательно с первой (см. start_by_previous), 1 - всегда начинать с первой
	Dim MODE_SINGLE As Integer = 0
	Dim MODE_SERIES As Integer = 1
Dim take_by_fill As Boolean
Dim d_OnOff   As Director       'основной директор анимации входа/смены/выхода
Dim nav_OnOff As Channel        'в основном директоре action-канал с именем nav, от слова navigation
Dim feelfill  As Boolean        'чувствительность на пустую строку fill. если  feelfill=true то нельзя будет выдать титр в котором нет текста для выдачи!
Dim fill_arr  As Array[String]  'массив серий, туда попадают титры-серии после разбиения по разделительным символам
Dim isCanChange, isCanINtoOUT As Boolean  'разрешения на однократное выполнение change и INtoOUT

	'текстовые поля с соответсвующими значениями... :)
Dim ctrl, fill, take, takeout, takethis, takeoutthis, cur As String
	
	'массивы обозначающие титры взаимодействующие с этим титром
Dim take_arr, takeout_arr, takethis_arr, takeoutthis_arr As Array[String]
	
	'стоперы и графницы главного директора
Dim stoper_a, stoper_b As Double
Dim start_time, end_time As Double
	'переменные для функции вычисления включения стоперов в диапазон
Dim startInclude, endInclude As Boolean
	'плейхэд и его допуск
Dim playhead As Double
Dim playheadTreshold As Double = 0.1

	'коренной контейнер
Dim cRoot As Container

	'директора дополнительных анимаций. Требуется для проигрывания анимаций в объектах DZ_TYPE_OBJECT
Dim arr_dirObjects As Array[Director]

	'anim keys of clipchannels in directors
Dim arr_clipKeys As Array[Keyframe]
 
'-**********************************************************************************-'
	'используется для хранения текущего индекса серии
Dim curSeries As Integer

	'массив серий полученный из fill
Dim arr_fill As Array[String]

	'пауза между сериями, измеряется во фреймах (т.е. 50 в секунду)
Dim pause As Integer

	'используется как счетчик пройденного времени с момента вызова start_delay_series()
	'значние -1 значит что считать не надо...
	'инкреминируется до значения pause
Dim passed As Integer = -1

	'начинать ли серию всегда с первой серии
Dim start_by_first As Boolean

	'если НЕ начинать с первой то начинать либо с последней выданной либо со следующей серии
Dim start_by_previous As Boolean

	'убирать ли титр если последняя серия отыграла
Dim takeout_by_last As Boolean

	'автоматически убирать по таймеру?
Dim auto_takeout As Boolean
	'автоматически убирать, с указанием паузы сколько титр должен простоять
	'стартовое значение для auto_takeout_from
Dim auto_takeout_pause As Double
	'переменная-таймер автоубирания (в филдах)
	'декреминируется до нуля
Dim auto_takeout_from As Integer


	'условия автоматического убирания
Dim takeout_by_last_condis As String
	'кол-во циклов до срабатывания автоматического убирания
Dim takeout_by_last_countLoop As Integer
Dim i_curLoop As Integer = 1
'-***********************************************************************-'

'ВРЕМЕННЫЕ ПЕРЕМЕННЫЕ
Dim s, nametype As String
Dim i AS Integer

'Пример fill:
'Geo:text=Москва
'Name=Дмитрий Дудин|Status=работаю на телеке

'LOG:
Sub Log(message As String)
	if System.Map["AUTOTAKEOUT_LOG"] == "1" then
		println(3, "DL::" & message)
	end if
End Sub

'DROPZONES:
Dim DZ_SIDE_ONLY_ONE = 0
Dim DZ_SIDE_FIRST = 1
Dim DZ_SIDE_SECOND = 2
Function DZ_SIDE(input As String) As Integer
	input.Trim()
	Select Case input
		Case "1"
			DZ_SIDE = DZ_SIDE_FIRST
		Case "2"
			DZ_SIDE = DZ_SIDE_SECOND
		Case Else
			DZ_SIDE = DZ_SIDE_ONLY_ONE
	End Select
End Function

' DZ_TYPE_ACTIVE
' DZ_TYPE_OMO
' DZ_TYPE_ALPHA
' DZ_TYPE_POS
' DZ_TYPE_ROT
' DZ_TYPE_SCALE
' DZ_TYPE_KEYFRAME_TIME
' DZ_TYPE_TEXT
' DZ_TYPE_NUMBER
' DZ_TYPE_IMAGE
' DZ_TYPE_IMAGE_POS
' DZ_TYPE_IMAGE_ROT
' DZ_TYPE_IMAGE_SCALE
' DZ_TYPE_GEOM_WIDTH
' DZ_TYPE_GEOM_HEIGHT
' DZ_TYPE_SOFTCLIP
' DZ_TYPE_CLIP_CHANNEL
' DZ_TYPE_OBJECT
' DZ_TYPE_KEY
' DZ_TYPE_LOOK_AT

Structure Dropzone	
	c As Container   'изменяемый контейнер титра
	order As Integer 'порядок (сверху вниз, в пределах поддерева титра)
	side As Integer  'отношение к анимации изменения — либо титр имеет одну единственную dropzone, либо 1 и 2 для плавной смены DZ_MODE
	name As String   'имя dropzon'ы
	type As String   'тип данных dropzon'ы
End Structure

Dim arr_dropzones As Array[Dropzone]

Sub SetDropzones()
	arr_dropzones.Clear()
	arr_dirObjects.Clear()
	Dim all_childs As Array[Container]
	Dim name_child, types, type As String
	Dim arr_types As Array[String]
	Dim order1, order2 As Integer 
	order1 = 0
	order2 = 0
	cRoot.GetContainerAndSubContainers(all_childs, false)
	for i = 0 to all_childs.ubound
		name_child = all_childs[i].name
		name_child.Trim()
		
		'dropzon'ы обозначаются символом = в начале названия контейнера, 
		'если его нет - значит это НЕ dropzon'а
		if name_child.left(1) <> "=" then
			all_childs.Erase(i)
			i -= 1
		end if
	next
	
	for i = 0 to all_childs.ubound
		
		Dim dz As Dropzone
		dz.c = all_childs[i]
		
		name_child = all_childs[i].name
		
		'Сохранить директора для дополнительной анимации объектов
		if name_child.Find("object") > 0 then
			arr_dirObjects.Push(Stage.FindDirector(name_child))
		end if
		
		name_child.Erase(0,1)
		name_child.MakeLower()
		dz.side = DZ_SIDE(name_child.left(1))
		
		if name_child.StartsWith("1") OR name_child.StartsWith("2") then
			dz.side = CInt(name_child.left(1))
			if dz.side == 1 then
				order1 += 1
				dz.order = order1
			elseif dz.side == 2 then
				order2 += 1
				dz.order = order2
			end if
			name_child.Erase(0,1)
		else
			dz.side = DZ_SIDE_ONLY_ONE '=0
			order1 += 1
			dz.order = order1
		end if
		
		if name_child.Find(":") > 0 then
			dz.name = name_child.left(name_child.Find(":"))
			dz.name.MakeLower()
			types = name_child.GetSubstring(name_child.Find(":")+1, name_child.Length)
			types.Split(",", arr_types)
			
			
			for y = 0 to arr_types.ubound
				type = arr_types[y]
				type.Trim
				type.MakeLower()
				if type == "" then type = "text"
				dz.type = type
				Dim ddzz As Dropzone = dz
				arr_dropzones.Push(ddzz)
			next
		else
			dz.name = name_child
			dz.type = "text"
			arr_dropzones.Push(dz)
		end if
	next
End Sub

Sub SendFillToDropzones(fill As String, side As Integer)
	Dim arr_data As Array[String]
	Dim name, type, data As String
	Dim dz As Dropzone
	Dim arr_xyz As Array[String]
	Dim order, precision As Integer
	
	if fill == "" then
		for y=0 to arr_dropzones.ubound
			dz = arr_dropzones[y]
			if dz.type == "text" then
				dz.c.Geometry.Text = ""
			end if
		next
	end if
	
	fill.split("|",arr_data)
	order = 1
	for i=0 to arr_data.ubound
		if arr_data[i].find("=") > 0 then
			name = arr_data[i].left(arr_data[i].find("="))
			if name.find(":") > 0 then
				type = name.GetSubstring(name.find(":") + 1, name.length)
				name = name.left(name.find(":"))
				if type = "" then type = "text"
			else
				type = "text"
			end if
			if arr_data[i].find("=") >= arr_data[i].length-1 then
				data = ""
			else
				data = arr_data[i].GetSubstring(arr_data[i].find("=")+1, arr_data[i].length)
			end if
		else
			name = ""
			type = "text"
			data = arr_data[i]
		end if
		name.MakeLower()
		type.MakeLower()
		data.Trim()
		
		for y=0 to arr_dropzones.ubound
			dz = arr_dropzones[y]
			'если совпадает имя, тип и сторона типа
			if side == dz.side OR dz.side == DZ_SIDE_ONLY_ONE then
				if name == dz.name OR (name == "" AND order == dz.order) then
					if type == "" then type="text"
					if type == dz.type then
						Select Case type
						Case "active"
							dz.c.Active = CBool(data)
						Case "omo"
							dz.c.GetFunctionPluginInstance("Omo").SetParameterInt("vis_con",CInt(data))
						Case "alpha"
							dz.c.Alpha.Value = CDbl(data)
						Case "pos"
							data.Substitute(",", ".", true)
							data.Split(";",arr_xyz)
							if arr_xyz.size > 0 then dz.c.position.x = CDbl(arr_xyz[0])
							if arr_xyz.size > 1 then dz.c.position.y = CDbl(arr_xyz[1])
							if arr_xyz.size > 2 then dz.c.position.z = CDbl(arr_xyz[2])
						Case "rot"
							data.Substitute(",", ".", true)
							data.Split(";",arr_xyz)
							if arr_xyz.size > 0 then dz.c.rotation.x = CDbl(arr_xyz[0])
							if arr_xyz.size > 1 then dz.c.rotation.y = CDbl(arr_xyz[1])
							if arr_xyz.size > 2 then dz.c.rotation.z = CDbl(arr_xyz[2])
						Case "scale"
							data.Substitute(",", ".", true)
							if data.find(";")>0 then
								data.Split(";",arr_xyz)
								if arr_xyz.size > 0 then dz.c.scaling.x = CDbl(arr_xyz[0])
								if arr_xyz.size > 1 then dz.c.scaling.y = CDbl(arr_xyz[1])
								if arr_xyz.size > 2 then dz.c.scaling.z = CDbl(arr_xyz[2])
							else
								dz.c.scaling.x = CDbl(data)
								dz.c.scaling.y = CDbl(data)
								dz.c.scaling.z = CDbl(data)
							end if
						Case "keyframe:time"
							data.Substitute(",", ".", true)
							dz.c.FindKeyframeOfObject("x").Time = CDbl(data)
						Case "text"
							dz.c.Geometry.Text = data
						Case "number"
							data.Substitute(",", ".", true)
							precision = 0
							if data.FindLastOf(".") >= 0 then precision = data.length-data.FindLastOf(".")-1
							s = DoubleToString(  CDbl(data), precision  )
							s.Substitute("/.", ",", true)
							dz.c.Geometry.Text = s
						Case "image"
							dz.c.CreateTexture(data)
						Case "imagepos"
							data.Substitute(",", ".", true)
							data.Split(";",arr_xyz)
							if arr_xyz.size > 0 then dz.c.texture.mapPosition.x = CDbl(arr_xyz[0])
							if arr_xyz.size > 1 then dz.c.texture.mapPosition.y = CDbl(arr_xyz[1])
						Case "imagerot"
							data.Substitute(",", ".", true)
							if data.find(";")>0 then
								data.Split(";",arr_xyz)
								if arr_xyz.size > 0 then dz.c.texture.mapRotation.x = CDbl(arr_xyz[0])
								if arr_xyz.size > 1 then dz.c.texture.mapRotation.y = CDbl(arr_xyz[1])
								if arr_xyz.size > 2 then dz.c.texture.mapRotation.z = CDbl(arr_xyz[2])
							else
								dz.c.texture.mapRotation.z = CDbl(data)
							end if
						Case "imagescale"
							data.Substitute(",", ".", true)
							if data.find(";")>0 then
								data.Split(";",arr_xyz)
								if arr_xyz.size > 0 then dz.c.texture.mapScaling.x = CDbl(arr_xyz[0])
								if arr_xyz.size > 1 then dz.c.texture.mapScaling.y = CDbl(arr_xyz[1])
							else
								dz.c.texture.mapScaling.x = CDbl(data)
								dz.c.texture.mapScaling.y = CDbl(data)
							end if
						Case "geomwidth"
							data.Substitute(",", ".", true)
							dz.c.GetGeometryPluginInstance().SetParameterDouble("width",CDbl(data))
						Case "geomheight", "geomdiameter", "geomangle"
							data.Substitute(",", ".", true)
							dz.c.GetGeometryPluginInstance().SetParameterDouble(type.Right(type.length - 4),CDbl(data))
						Case "softclip"
							s = dz.c.GetFunctionPluginInstance("SoftClip").GetParameterString("clipFile")
							s.Trim()
							if s <> CStr(data) then
								dz.c.GetFunctionPluginInstance("SoftClip").SetParameterString("clipFile",CStr(data))
							end if
						Case "clip1"
							SetClipChannel(1,CStr(data))
						Case "clip2"
							SetClipChannel(2,CStr(data))
						Case "clip3"
							SetClipChannel(3,CStr(data))
						Case "clip4"
							SetClipChannel(4,CStr(data))
						Case "cliploop1"
							SetClipLoop(1,CBool(data))
						Case "cliploop2"
							SetClipLoop(2,CBool(data))
						Case "cliploop3"
							SetClipLoop(3,CBool(data))
						Case "cliploop4"
							SetClipLoop(4,CBool(data))
						Case "object"
							s = CStr(data)
							s.Trim()
							if s == "" then
								dz.c.DeleteGeometry()
							else
								if dz.c.Geometry.VizId = -1 OR System.SendCommand("#" & dz.c.Geometry.VizId & "*LOCATION_PATH GET") <> s then
									dz.c.CreateGeometry( s )
								end if
							end if
						Case "key"
							dz.c.Key.DrawKey = CBool(data)
						Case "lookat"
							'<-1 #18936*LOOK_AT*AUTO_ROTATION SET BILLBOARD>
							s = CStr(data)
							s.Trim()
							s.MakeUpper()
							if s = "" then s = "NONE"
							'dz.c.GetFunctionPluginInstance("LOOK_AT").SetParameterString("AUTO_ROTATION", s)
							System.SendCommand("#" & dz.c.VizId & "*LOOK_AT*AUTO_ROTATION SET " & s)
							'if s = "NONE" then dz.c.Rotation.xyz = CVertex(0,0,0)
						Case Else
							dz.c.Geometry.Text = data
						End Select
						
						
					end if
				end if
			end if
		next
		order += 1
	next
End Sub

Function FindClipChannelKey(i As Integer) As Keyframe
	'Need to find way fo find keyframe independently of key... Temporarly, I leave this:
	FindClipChannelKey = Stage.FindDirector("Clip" & i).FindKeyframe("clip" & i)
End Function

Sub SetClipChannel(num As Integer, path As String)
'	num -= 1
'	s = system.GetClipChannel(num).GetClipName()
'	if s <> path then
'		system.GetClipChannel(num).FlushActive()
'		system.GetClipChannel(num).SetClipName(path)
'		system.GetClipChannel(num).FlushPending()
'	end if
	System.SendCommand("#" & arr_clipKeys[num].VizId & "*CLIPNAME SET " & path)
End Sub

Sub SetClipLoop(num As Integer, is_loop As Boolean)
'	num -= 1
'	system.GetClipChannel(num).LoopMode = is_loop
	arr_clipKeys[num].channel.PostLoopActive = is_loop
	System.SendCommand("#" & arr_clipKeys[num].channel.VizId & "*POST_LOOP_INFINITE SET " & CInt(is_loop))
End Sub
 
'-----------------------------------------------------------------------------------------------------------
'используемые глобальные переменные:
'(вместо "Titr" подставляется имя данного титра)
'
'System.Map["Titr_control"]
'возможнные значения:
'-1 - безусловно убрать данный титр! Чтобы не сработала логика взаимоубирания
'0 - убрать данный титр
'1 - по возможности выдать данный титр
'2 - выдать данный титр даже если он уже выдан, т.е. при необходимости проиграть анимацию смены
'3 - выдать/убрать, т.е. если убрано - выдать и если выдано - убрать...
'4 - превью титра, т.е. мнгновенно выдать! Фактически show(take_b)+continue
'5 - сброс переменной, чтобы скрипт отрегагировал на изменение в следующий раз
'
'System.Map["Titr_status"]
'то же что и 'System.Map["Titr_control"]
'только гарантированно хранит текущее состояние 0..1
'
'System.Map["Titr_fill"]
'в эту переменную помещается текст титра для последующей выдачи с помошью Titr_control
'так же это может быть серия титров разделенных символом Разделителем, по умолчанию |
'
'System.Map["Titr_value"]
'это конкретное значение выданного в данный момент титра
'когда титр убран это значение равно пустой строке!
'
'System.Map["Titr_previous"]
'это значения титра в предыдущий раз, такая переменная необходима 
'для определения логики выдачи серии титров:
'когда выдается серия ровно такая же как в предыдущий раз
'и установлен параметер "всегда начинать с первой" в Off
'титр продолжает выдачу со следующей серии!
'
'System.Map["Titr_curSeries"]
'это индекс текущей серии в серийном режиме
'
'
'...вместо "Titr" подставляется имя данного титра!
'----------------------------------------------------------
 
Sub OnInitParameters()	
	'println("------------------------------------------")
	'println("== ON INIT PARAMETERS " & titr_name & " ==")
	'println("------------------------------------------")
 
	RegisterParameterString("Name", "Name:", "", 30, 256, "")
	RegisterParameterBool("Mode", "Serial mode", false)
	RegisterParameterString("Separator", "└ Delimeter:", "\\n", 10, 32, "")
	RegisterParameterDouble("Pause", "└ Pause(sec):", 5, 0, 10000)
	RegisterParameterBool("Start_by_first", "└ Always begin from first", false)
	RegisterParameterBool("Start_by_previous", "    └ from previous", false)
	RegisterParameterBool("Takeout_by_last", "└ Takeout on the last", false)
	RegisterParameterString("Takeout_by_last_condis", "    └ Conditions 1&2", "", 40, 256, "")
	RegisterParameterInt("Takeout_by_last_countLoop", "    └ Cicle Count", 1, 1, 10000)
	RegisterParameterBool("LogicAutoTakeout", "Logic auto-takeouf", false)
	RegisterParameterString("Take", "└ When it take        1,2(3&4)", "", 65, 256, "")
	RegisterParameterString("Takeout", "└ When it takeout     1,2(3&4)", "", 65, 256, "")
	RegisterParameterString("TakeThis", "└ When anothers take 1,2(3&4)", "", 65, 256, "")
	RegisterParameterString("TakeoutThis", "└ When anothets takeout    1,2(3&4)", "", 65, 256, "")
	RegisterParameterBool("FeelFill", "Takeout if fill is empty", false)
	RegisterParameterBool("TakeByFill", "Take by changing of fill", false)
	RegisterParameterBool("AUTOTAKEOUT", "Timer of auto-takeout", false)
	RegisterParameterDouble("AUTOTAKEOUTPause", "└ delay (sec):", 0, 0, 10000)
	RegisterParameterContainer("root", "Root of element:")
	RegisterPushButton("rebuild", "Initialize", 1)
	RegisterInfoText(info)
	RegisterParameterText("console", "После изменения параметров следует
нажать Initialize чтобы они применились сразу.
При загрузки сцены это происходит автоматически!", 450, 50)
	RegisterParameterBool("TestFunctions", "Show test features", false)
	RegisterPushButton("TestTake","Take",11)
	RegisterPushButton("TestTakeout","Takeout",10)
	RegisterPushButton("TestChange","Change",12)
	RegisterPushButton("TestOnOff","On/Off",13)
	RegisterParameterText("TestFill", "", 450, 80)
	RegisterPushButton("TestMakeFill","Only fill",20)
	RegisterPushButton("FastPreview","Fast Preview",21)
End sub
'----------------------------------------------------------
 
sub OnInit()
	'опредлеям имя титра!
	titr_name = GetParameterString("Name")
	titr_name.trim()
	If titr_name = "" Then
		'Если в поле "имя титра" не вписано ничего, то попытаемся получить его исходя из имени контейнера [из квадратных скобок]
		If this.name.find("|") > 1 Then
			'Ура! Видимо тут раньше было имя и контейнер оформлен правильно — берем имя и логику из имени своего контейнера
			Dim arrNames As Array[String]
			titr_name = this.name
			titr_name.Trim()
			titr_name.Split("|",arrNames)
			titr_name = arrNames[0].GetSubstring(1,arrNames[0].Length-5)

			this.ScriptPluginInstance.SetParameterString("Name",titr_name)
			this.ScriptPluginInstance.SetParameterString("Take",arrNames[1])
			this.ScriptPluginInstance.SetParameterString("Takeout",arrNames[2])
			this.ScriptPluginInstance.SetParameterString("TakeThis",arrNames[3])
			this.ScriptPluginInstance.SetParameterString("TakeoutThis",arrNames[4])
			SendGuiRefresh()
		Else
			'если нельзя получить имя титра - нафиг надо его вычислять?! =)
			console &= "> Отсутвует имя титра! :(" & "\nБез него не буду ничего вычислять!\n"
			exit sub
		End If
	End If
	If titr_name.Find("_") <> -1 Then
		console &= "> Внимание! Символ_подчеркивания не гарантирует грамотную работу.\n"
	End If
	'------------------
	'cброс fill
	memory[titr_name & "_fill"] = ""
	local_memory[titr_name & "_fill"] = ""
	'будем реагировать на изменение этих двух переменных:
	memory.RegisterChangedCallback(titr_name & "_control")
	memory.RegisterChangedCallback(titr_name & "_fill")
	'дополнительно смотрим на локальные переменные
	local_memory.RegisterChangedCallback(titr_name & "_control")
	local_memory.RegisterChangedCallback(titr_name & "_fill")
	'и на глобальное обновление базы автобубирания
	memory.RegisterChangedCallback(prefix & "AUTOTAKEOUT_ALL_RECALCULATE")
 
	'вычисляем директор с его каналами и ключами
	CalculateDirector()
	'------------------
	take = GetParameterString("Take")
	takeout = GetParameterString("Takeout")
	takethis = GetParameterString("TakeThis")
	takeoutthis = GetParameterString("TakeoutThis")
	feelfill = GetParameterBool("FeelFill")
	take_by_fill = GetParameterBool("TakeByFill")
	cRoot = GetParameterContainer("root")
	if cRoot.name == "" then	cRoot = this
	mode = CInt(GetParameterBool("Mode"))
 
 
	separator = GetParameterString("Separator")
	separator.Trim()
	separator.MakeLower()
	If separator == "vbnewline" OR separator == "\\n" Then
		'если разделитель - обычный перенос строки
		separator = "\n"
	End If
	start_by_first = GetParameterBool("Start_by_first")
	start_by_previous = GetParameterBool("Start_by_previous")
	takeout_by_last = GetParameterBool("Takeout_by_last")
	pause = CInt(50.0 * GetParameterDouble("Pause"))
	takeout_by_last_condis = GetParameterString("Takeout_by_last_condis")
	takeout_by_last_countLoop = GetParameterInt("Takeout_by_last_countLoop")
 
'---параметры автоубирания через паузу
	auto_takeout = GetParameterBool("AUTOTAKEOUT")
	auto_takeout_pause = GetParameterDouble("AUTOTAKEOUTPause")
	auto_takeout_from = 0
 
'---создание массивов имен блоков для реакции
	take.Split(",",take_arr)
	takeout.Split(",",takeout_arr)
	takethis.Split(",",takethis_arr)
	takeoutthis.Split(",",takeoutthis_arr)
 
'---оформляю текущий контейнер (имя + цвет)
	titr_name.trim()
	If titr_name = "" Then
		this.name = "_none_"
	Else
		take.Substitute(" ","",TRUE)
		takeout.Substitute(" ","",TRUE)
		takethis.Substitute(" ","",TRUE)
		takeoutthis.Substitute(" ","",TRUE)
		this.name = "[" & titr_name & "]---|" & take & "|" & takeout & "|" & takethis & "|" & takeoutthis
	End If
	System.SendCommand("-1 THIS_SCENE*TREE*#" & this.VizId & "*GUI_COLOR_INDEX SET 2")
	this.SetChanged()
	this.Update()
	Scene.SetChanged()
	Stage.SetChanged()
	Scene.UpdateSceneTree()

	'отправляем массивы условий в глобальную часть
	SendConditionsToGlobal()
	
	'сбрасываем стартовые значения в локальной памяти
	local_memory[titr_name & "_value"] = ""
	local_memory[titr_name & "_control"] = 5
	local_memory[titr_name & "_status"] = 0
	'выключаем таймер
	passed = -1
 
	'ставим директор в нулевую позицию
	d_OnOff.Show(0)
	
	'set ClipChannel keys
	arr_clipKeys.Clear()
	arr_clipKeys.Push(null)
	for i=1 to 4
		arr_clipKeys.Push(FindClipChannelKey(i))
	next
	
	'устанавливаем все dropzon'ы
	SetDropzones()
'---инициализация завершена!
end sub

Sub SendConditionsToGlobal()
	'---заполняем массивы автоубирания/автопоявления
	Dim AUTOTAKEOUTonTAKE As Array[Array[String]]
	Dim AUTOTAKEOUTonTAKEOUT As Array[Array[String]]
	Dim cur_arr As Array[String]
	'-------Для Take
	cur_arr.Clear()
	If take_arr.UBound > -1 Then
		cur_arr.Push(titr_name)
		For i=0 to take_arr.UBound
			cur_arr.Push(take_arr[i])
		Next
		If cur_arr.UBound > 0 Then
			AUTOTAKEOUTonTAKE.Push(cur_arr)
		End If
	End If
	'-------Для TakeOut
	cur_arr.Clear()
	If takeout_arr.UBound > -1 Then
		cur_arr.Push(titr_name)
		For i=0 to takeout_arr.UBound
			cur_arr.Push(takeout_arr[i])
		Next
		If cur_arr.UBound > 0 Then
			AUTOTAKEOUTonTAKEOUT.Push(cur_arr)
		End If
	End If
	'-------Для TakeThis 
	For i=0 to takethis_arr.UBound
		cur_arr.Clear()
		cur = takethis_arr[i]
		cur.trim()
		If cur.Left(1) = "-" Then
			cur = cur.Right(cur.Length - 1)
			cur_arr.Push(cur)
			cur_arr.Push("+" & titr_name)
			AUTOTAKEOUTonTAKEOUT.Push(cur_arr)
		ElseIf cur.Left(1) = "+" Then
			cur = cur.Right(cur.Length - 1)
			cur_arr.Push(cur)
			cur_arr.Push("+" & titr_name)
			AUTOTAKEOUTonTAKE.Push(cur_arr)
		Else
			cur_arr.Push(cur)
			cur_arr.Push("+" & titr_name)
			AUTOTAKEOUTonTAKE.Push(cur_arr)
		End If
	Next
	'-------Для TakeOutThis
	For i=0 to takeoutthis_arr.UBound
		cur_arr.Clear()
		cur = takeoutthis_arr[i]
		cur.trim()
		If cur.Left(1) = "-" Then
			cur = cur.Right(cur.Length - 1)
			cur_arr.Push(cur)
			cur_arr.Push("-" & titr_name)
			AUTOTAKEOUTonTAKEOUT.Push(cur_arr)
		ElseIf cur.Left(1) = "+" Then
			cur = cur.Right(cur.Length - 1)
			cur_arr.Push(cur)
			cur_arr.Push("-" & titr_name)
			AUTOTAKEOUTonTAKE.Push(cur_arr)
		Else
			cur_arr.Push(cur)
			cur_arr.Push("-" & titr_name)
			AUTOTAKEOUTonTAKE.Push(cur_arr)
		End If
	Next
	
	'созданные массивы авто-управления и подмены
	'отправляем запись в общий скрипт для занесения в общий список взаимодействия
	local_memory[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = null
	local_memory[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"] = null
	
	If GetParameterBool("LogicAutoTakeout") Then
		local_memory[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = AUTOTAKEOUTonTAKE
		local_memory[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"] = AUTOTAKEOUTonTAKEOUT
	End If
End Sub
'----------------------------------------------------------
 
Sub OnExecAction(buttonId As Integer)
	'при нажатии на "Initialize"
	If buttonId = 1 Then
		console = ""
		'выполняем стандартные процедуры инициализации
		OnInitParameters()
		OnInit()
		'и заставляем пересчитать всю логику АВТОУБИРАНИЯ (взаимодействия титров)
		memory[prefix & "AUTOTAKEOUT_ALL_RECALCULATE"] = ""
		memory[prefix & "AUTOTAKEOUT_ALL_RECALCULATE"] = titr_name
 
		'выводим отчет о проделанной работе
		'если не было зафиксировано ошибок то OK
		If console = "" Then console = "OK"
		this.ScriptPluginInstance.SetParameterString("console",console)
		SendGuiRefresh()
	ElseIf buttonId >= 10 AND buttonId < 20 Then
		memory[titr_name & "_fill"] = CStr(GetParameterString("TestFill"))
		memory[titr_name & "_control"] = (buttonId-10)
	ElseIf buttonId == 20 Then
		memory[titr_name & "_fill"] = CStr(GetParameterString("TestFill"))
	ElseIf buttonId == 21 Then
		memory[titr_name & "_fill"] = CStr(GetParameterString("TestFill"))
		memory[titr_name & "_control"] = 4
	End If
End Sub
'----------------------------------------------------------
sub OnGuiStatus()
	mode = CInt(GetParameterBool("Mode"))
	'отслеживаем выбор серийного режима (SINGLE или SERIES)
	If mode == 1 Then
		SendGuiParameterShow("Separator", SHOW)
		SendGuiParameterShow("Pause", SHOW)
		SendGuiParameterShow("Start_by_first", SHOW)
		SendGuiParameterShow("Takeout_by_last", SHOW)
	Else
		SendGuiParameterShow("Separator", HIDE)
		SendGuiParameterShow("Pause", HIDE)
		SendGuiParameterShow("Start_by_first", HIDE)
		this.ScriptPluginInstance.SetParameterBool("Start_by_first",TRUE)
		SendGuiParameterShow("Start_by_previous", HIDE)
		SendGuiParameterShow("Takeout_by_last", HIDE)
		this.ScriptPluginInstance.SetParameterBool("Takeout_by_last",FALSE)
		SendGuiParameterShow("Takeout_by_last_condis", HIDE)
		SendGuiParameterShow("Takeout_by_last_countLoop", HIDE)
	End If
 
	firster = CInt(GetParameterBool("Start_by_first"))
	If firster == 1 Then
		SendGuiParameterShow("Start_by_previous", HIDE)
	Else
		SendGuiParameterShow("Start_by_previous", SHOW)
	End If
	
	If GetParameterBool("LogicAutoTakeout") Then
		SendGuiParameterShow("Take", SHOW)
		SendGuiParameterShow("Takeout", SHOW)
		SendGuiParameterShow("TakeThis", SHOW)
		SendGuiParameterShow("TakeoutThis", SHOW)
	Else
		SendGuiParameterShow("Take", HIDE)
		SendGuiParameterShow("Takeout", HIDE)
		SendGuiParameterShow("TakeThis", HIDE)
		SendGuiParameterShow("TakeoutThis", HIDE)
	End If
 
	firster = CInt(GetParameterBool("AUTOTAKEOUT"))
	If firster == 1 Then
		SendGuiParameterShow("AUTOTAKEOUTPause", SHOW)
	Else
		SendGuiParameterShow("AUTOTAKEOUTPause", HIDE)
	End If
 
	takeout_by_last = GetParameterBool("Takeout_by_last")
	If takeout_by_last Then
		SendGuiParameterShow("Takeout_by_last_condis", SHOW)
		SendGuiParameterShow("Takeout_by_last_countLoop", SHOW)
	Else
		SendGuiParameterShow("Takeout_by_last_condis", HIDE)
		SendGuiParameterShow("Takeout_by_last_countLoop", HIDE)
	End If
 
	testers = CInt(GetParameterBool("TestFunctions"))
	If testers == 1 Then
		SendGuiParameterShow("TestTake", SHOW)
		SendGuiParameterShow("TestTakeout", SHOW)
		SendGuiParameterShow("TestChange", SHOW)
		SendGuiParameterShow("TestOnOff", SHOW)
		SendGuiParameterShow("TestFill", SHOW)
		SendGuiParameterShow("TestMakeFill", SHOW)
	Else
		SendGuiParameterShow("TestTake", HIDE)
		SendGuiParameterShow("TestTakeout", HIDE)
		SendGuiParameterShow("TestChange", HIDE)
		SendGuiParameterShow("TestOnOff", HIDE)
		SendGuiParameterShow("TestFill", HIDE)
		SendGuiParameterShow("TestMakeFill", HIDE)
	End If
end sub
'----------------------------------------------------------
 
Sub OnSharedMemoryVariableChanged (map As SharedMemory, mapKey As String)
	If NOT ( Scene.IsBacklayer() OR Scene.IsFrontlayer() OR Scene.IsMainlayer() ) then
		'если сцена не в слое — то никак не реагировать
		Log("Scene is not in layer. Only in scene pool.")
		exit sub
	end if
	
	Log(CStr(map))
	' CONTROL
	If mapKey = titr_name & "_control" Then
		'реакция на управляющую переменную
		ctrl = map[titr_name & "_control"]
		Log(titr_name & "_control = " & ctrl)
		
		'сбрасываем через 2 кадра значение, чтобы реакция оставалась даже на одно и то же значение
		if ctrl <> "5" Then
			reset_control_with_delay()
		end if
		
		If ctrl = "1" Then
			'TAKE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			'Проверка - выдаем если только титр убран
			Log("TRY TAKE")
			
			If PlayheadIsNear(0) OR PlayheadIsMore(stoper_b) Then
				isCanChange = false
				isCanINtoOUT = false
				fill = map[titr_name & "_fill"]
				fill.trim()
				
				If ( feelfill AND fill == "" ) then 
					'OR ( PlayheadIsMore(stoper_b) AND d_OnOff.IsAnimationRunning() )
					d_OnOff.ContinueAnimation()
					local_memory[titr_name & "_status"] = 0
					Log("FAIL TAKE")
					exit sub
				End If
				'-------
				'выдаем уж точно:
				Log("DO TAKE")
				d_OnOff.Show(0)
				d_OnOff.ContinueAnimation()
				take()
			End If
			'объявляем текущее состояние
			local_memory[titr_name & "_status"] = 1
			'запускаем клипы
			StartClips()
			'запускаем анимацию геометрий
			StartAnimGeoms()
		ElseIf ctrl = "0" OR ctrl = "-1" Then
			'TAKEOUT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			'Проверка - Если ползунок времени находится в пределах выданного состояния, то убираем
			isCanChange = false
			isCanINtoOUT = false
			'если реагируем на fill, то обнуляем fill
			if take_by_fill then
				println(1,"CLEAR FILL")
				memory[titr_name & "_fill"] = ""
				local_memory[titr_name & "_fill"] = ""
			end if
			
			If PlayheadIsMore(0) AND PlayheadIsLess(end_time) Then
				If PlayheadIsLess(stoper_b) Then d_OnOff.Show(stoper_b)				
				d_OnOff.ContinueAnimation()
				takeout_change()
			End If
			'объявляем текущее состояние
			local_memory[titr_name & "_status"] = 0
			
			
		ElseIf ctrl = "2" Then
			'CHANGE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			'Проверка - если выдан, то проигрываем блок анимации loop(в котором меняется значение)
			fill = map[titr_name & "_fill"]
			fill.trim()
			If PlayheadBetweenAndIncludeLastStoper(0,stoper_b) Then
				If feelfill AND fill = "" then
					local_memory[titr_name & "_control"] = 0
					exit sub
				End If
				
				If stoper_a == stoper_b Then
					'если блок loop отсутствует
					d_OnOff.Show(0)
					local_memory[titr_name & "_control"] = 1
				Else
					'если есть блок loop
					if fill <> local_memory[titr_name & "_value"] then
						d_OnOff.Show(stoper_a)
						change()
						d_OnOff.ContinueAnimation()
					end if
				End If
				'сохраняеям текущее состояние
				local_memory[titr_name & "_status"] = 1
			ElseIf PlayheadIsNear(0) OR PlayheadIsMore(stoper_b) Then
				'если не выдан, надо просто выдать
				If fill <> "" OR NOT feelfill then
					local_memory[titr_name & "_control"] = 1
					exit sub
				End if
			End If
			'объявляем текущее состояние
			StartClips()
		ElseIf ctrl = "3" Then
			'ON/OFF !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			If PlayheadBetweenAndIncludeLastStoper(0,stoper_b) Then
				local_memory[titr_name & "_control"] = 0
			Else
				local_memory[titr_name & "_control"] = 1
			End If
		ElseIf ctrl = "4" Then
			'PREVIEW титра - быстро показать выданный момент титра
			isCanChange = false
			isCanINtoOUT = false
			d_OnOff.Show(stoper_a-0.04)
			d_OnOff.ContinueAnimation()
			take()
		ElseIf ctrl = "6" Then
			ContinueGeomsAnimations()
		End If
	
	' FILL
	ElseIf mapKey = titr_name & "_fill" Then
		Log(titr_name & "_fill = " & fill)
		fill = map[titr_name & "_fill"]
		fill.trim()
		take_by_fill = GetParameterBool("TakeByFill")
		
		if map == memory then	local_memory[titr_name & "_fill"] = memory[titr_name & "_fill"]
		
		If FeelFill AND fill == "" then
			local_memory[titr_name & "_control"] = 0
		elseif take_by_fill AND fill <> "" then
			local_memory[titr_name & "_control"] = 2
		End If
		
		
	ElseIf mapKey = prefix & "AUTOTAKEOUT_ALL_RECALCULATE" and memory[prefix & "AUTOTAKEOUT_ALL_RECALCULATE"] <> "" then
		If memory[prefix & "AUTOTAKEOUT_ALL_RECALCULATE"] <> titr_name Then
			OnInit()
		Else
			SendConditionsToGlobal()
		End If
	End If
End Sub
'----------------------------------------------------------

Sub CalculateDirector()
	'найти основной директор, его ключи...
 
	d_OnOff = Stage.FindDirector (titr_name)
	If d_OnOff = null Then
		'если основной директор НЕ найден :(
		console &= "> Не смог найти основной директор!\n" & "Он должен быть назван \"" & titr_name & "\"\n"
	Else
		'если основной директор НАЙДЕН!
		if d_OnOff.EventChannel.KeyframeCount < 1 then
			console &= "> В основном директоре нет стоперов!\n" & "Надо добавить минимум один стопер.\n"
			exit sub
		elseif d_OnOff.EventChannel.KeyframeCount > 2 then
			console &= "> В основном директоре многовато стоперов!\n" & "Надо сократить до 2-х!\n"
			exit sub
		end if
		
		stoper_a = d_OnOff.EventChannel.FirstKeyframe.Time
		stoper_b = d_OnOff.EventChannel.LastKeyframe.Time
		
		start_time = CDbl(  System.SendCommand("THIS_SCENE*STAGE*#" & d_OnOff.VizID & "*START_TIME GET")  )
		end_time   = CDbl(  System.SendCommand("THIS_SCENE*STAGE*#" & d_OnOff.VizID & "*END_TIME GET"  )  )
	End If
End Sub
'----------------------------------------------------------
 
'процедура реакции на выдачу титра
Sub take()
	If mode == MODE_SERIES Then
		If start_by_first == TRUE Then
			'если надо полюбому стартовать с первой
			local_memory[titr_name & "_curSeries"] = 0
		Else
			'если стартовать надо по ситуации 
			If local_memory[titr_name & "_value"] == local_memory[titr_name & "_previous"] Then
				'если ситуация сложилась (т.е. нововыданный титр идентичный предыдущему) и надо понять как действовать
				If start_by_previous == TRUE Then
					'если надо продолжать с выданной в предуыдущий раз серии
					'в этой ситуации ничего не надо трогать
				Else
					'если надо продолжать со следующей серии относительно выданный в предыдущий раз
					local_memory[titr_name & "_curSeries"] = local_memory[titr_name & "_curSeries"] + 1
				End If
			Else
				'если ситуация не сложилась, т.е. новый набор титров отличается от предыдущего
				local_memory[titr_name & "_curSeries"] = 0
			End If
		End If
 
		i_curLoop = 1
		fill = take_cur_series()
		start_delay_series()
	Else
		fill = memory[titr_name & "_fill"]
	End If
	local_memory[titr_name & "_value"] = fill
 
	If cRoot <> null Then
		SendFillToDropzones(fill,DZ_SIDE_FIRST)
		SendFillToDropzones(fill,DZ_SIDE_SECOND)
	End If
 
	'включаем таймер автоубирания, если надо
	auto_takeout = GetParameterBool("AUTOTAKEOUT")
	If auto_takeout == TRUE Then
		auto_takeout_pause = GetParameterDouble("AUTOTAKEOUTPause")
		StartTimerAutoTakeout(auto_takeout_pause)
	End If
	
	StartObjectDirectors()
End Sub
'----------------------------------------------------------
 
'процедура реакции на прееход с одного на другое значение
'выполняется в ключе loop_c
Sub change()
	If cRoot = null Then Exit Sub
	isCanINtoOUT = true
	If mode == MODE_SERIES Then
		isCanChange = false
		local_memory[titr_name & "_curSeries"] = local_memory[titr_name & "_curSeries"] + 1
		fill = take_cur_series()
		start_delay_series()
	Else
		fill = memory[titr_name & "_fill"]
	End If
	fill.trim()
	local_memory[titr_name & "_value"] = fill
	SendFillToDropzones(fill,DZ_SIDE_SECOND)
	
	StartObjectDirectors()
End Sub
'----------------------------------------------------------
 
'процедура выравнивания текста на обоих контейнерах
'выполняется в ключе loop_d
Sub INtoOUT()
	'TODO: перекинуть данные из 2 в 1
	If cRoot = null Then Exit Sub
	isCanINtoOUT = false
	fill = local_memory[titr_name & "_value"]
	fill.trim
	SendFillToDropzones(fill,DZ_SIDE_FIRST)
End Sub
'----------------------------------------------------------
 
'процедура реакции на убирание титра
Sub takeout_change()
	'по-любому вырубаем таймер
	stop_delay_series()
 
	'устанавливаем предыдущее значение
	fill = CStr(local_memory[titr_name & "_fill"])
	fill.Trim()
	local_memory[titr_name & "_previous"] = fill
 
	'а текущее значение в ничто
	local_memory[titr_name & "_value"] = ""
End Sub
'----------------------------------------------------------

'запуск дополнительных анимаций объектов
Sub StartObjectDirectors()
	for i=0 to arr_dirObjects.UBound
		arr_dirObjects[i].StartAnimation()
	next
	
End Sub

'запуск ClipChannel-видео
Sub StartClips()
	for y=0 to arr_dropzones.ubound
		if arr_dropzones[y].type.length == 5 AND arr_dropzones[y].type.left(4) == "clip" then
			i = CInt(arr_dropzones[y].type.right(1))
			'system.GetClipChannel(i-1).Play(0)
			arr_clipKeys[i].channel.director.StartAnimation()
		end if
	next
End Sub

Sub StartAnimGeoms()
	for y=0 to arr_dropzones.ubound
		if arr_dropzones[y].type == "object" then
			Stage.FindDirector(arr_dropzones[y].c.name).StartAnimation()
		end if
	next
End Sub

Sub ContinueGeomsAnimations()
	for y=0 to arr_dropzones.ubound
		if arr_dropzones[y].type == "object" then
			Stage.FindDirector(arr_dropzones[y].c.name).ContinueAnimation()
		end if
	next
End Sub
 
 
'-*****************************************************************************************-'
'-*****************************************************************************************-'
'-*****************************************************************************************-'
' РАБОТА С СЕРИЯМИ...
function take_cur_series() as String
	if MODE_SERIES == mode then
		'значит если СЕРИЙНЫЙ режим
		'надо понять какая серия была выдана и вернуть следующую
		if local_memory.ContainsKey(titr_name & "_curSeries") then
			curSeries = CInt(local_memory[titr_name & "_curSeries"])
			if curSeries < 0 Then curSeries = 0
		else
			curSeries = 0
		end if
		
		
		fill = local_memory[titr_name & "_fill"]
		fill.trim()
		if fill.find("=") > 0 then
			nametype = fill.left(fill.find("="))
			fill = fill.right(fill.length - fill.find("=") - 1)
		end if
		fill.split(separator, arr_fill)
		
		if arr_fill.size > 0 then
			'если есть хоть одна серия:
			for i = 0 to arr_fill.UBound
				s = arr_fill[i]
				s.Trim()
				If s == "" Then arr_fill.Erase(i)
			next
			
			if arr_fill.Size <= 1 then
				arr_fill.push("")
				curSeries = 0
			else
				'если текущая серия больше чем доступных серий, то сбросить
				if curSeries > arr_fill.UBound then
					curSeries = 0
					local_memory[titr_name & "_curSeries"] = 0
				end if
			end if
			
			nametype.Trim()
			if nametype == "" then
				take_cur_series = arr_fill[curSeries]
			else
				take_cur_series = nametype & "=" & arr_fill[curSeries]
			end if
		else
			'если нет ни одной серии
			local_memory[titr_name & "_curSeries"] = 0
			take_cur_series = ""
		end if
	else
		'в НЕсерийном режиме отдает пустую строку
		take_cur_series = ""
	end if
end function
 
'функция подачи следующей серии комманды с задеркой
sub take_next_series()
	'если таймер выключен - ничего не делать!
	'if passed < 0 Then Exit Sub
	passed = 0
	
	if MODE_SERIES == mode then
		'значит если серийный режим
		'надо понять какая серия была выдана и выдать следующую
		curSeries = CInt(local_memory[titr_name & "_curSeries"])
		'нулевая серия — первый элемент
		if curSeries < 0 Then curSeries = 0
 
		fill = memory[titr_name & "_fill"]
		fill.trim()
		fill.split(separator,arr_fill)
		
		if arr_fill.Size > 0 Then
			for i = 0 to arr_fill.UBound
				'удаляем пустые серии ;)
				s = arr_fill[i]
				s.Trim()
				If s == "" Then arr_fill.Erase(i)
			next
		end if
 		
		If arr_fill.UBound == 0 Then
			If arr_fill[0] == local_memory[titr_name & "_value"] Then
				'если серия одна и идентична той что в переменной - то анимация смены НЕ нужна
				exit sub
			Else
				'если серия одна, но она отлична от содержимого серии - то НАДО проиграть смену титров
			End If
		End If
 
		'условия автоубирания:
		takeout_by_last = GetParameterBool("Takeout_by_last")
		takeout_by_last_condis = GetParameterString("Takeout_by_last_condis")
		takeout_by_last_condis.Trim()
		
		If takeout_by_last == TRUE Then
			If curSeries >= arr_fill.UBound Then
				If start_by_previous == TRUE Then curSeries = 0
 
				i_curLoop += 1
				takeout_by_last_countLoop = GetParameterInt("Takeout_by_last_countLoop")
 
				If i_curLoop > takeout_by_last_countLoop Then
					If takeout_by_last_condis <> "" Then
						If CheckCondis(takeout_by_last_condis) == TRUE Then
							local_memory[titr_name & "_control"] = 0
						Else
							local_memory[titr_name & "_control"] = 2
						End If
					Else
						local_memory[titr_name & "_curSeries"] = curSeries
						local_memory[titr_name & "_control"] = 0
					End If
				Else
					local_memory[titr_name & "_control"] = 2
				End If
			Else
				local_memory[titr_name & "_control"] = 2
			End If
		Else
			If curSeries > arr_fill.UBound Then curSeries = 0
			local_memory[titr_name & "_control"] = 2
		End If
	End if
end sub
 
Function CheckCondis(s_conditions As String) As Boolean
	Dim arr_condis As Array[String]
	Dim s_condisItem As String
	Dim s_condisFill As String
	s_conditions.split("&",arr_condis)
 
	For i_condis = 0 to arr_condis.UBound
		s_condisItem = arr_condis[i_condis]
		s_condisItem.Trim()
 
 
 
		If s_condisItem.Find("_fill") > 0 Then
			'начало обработки FILL
 
			If s_condisItem.Left(1) = "+" Then
				s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				s_condisFill = CStr(memory[s_condisItem])
				s_condisFill.Trim()
				If s_condisFill == "" Then
					'условие не соблюдено! ыыы...
					CheckCondis = FALSE
					Exit Function
				End If
			Else
				If s_condisItem.Left(1) = "-" Then s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				s_condisFill = CStr(memory[s_condisItem])
				If s_condisFill <> "" Then
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
				If CInt(local_memory[s_condisItem & "_status"]) <> 1 Then
					'условие не соблюдено! ыыы...
					CheckCondis = FALSE
					Exit Function
				End If
			Else
				If s_condisItem.Left(1) = "-" Then s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				If CInt(local_memory[s_condisItem & "_status"]) <> 0 Then
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
 
sub start_delay_series()
	pause = CInt(50.0 * GetParameterDouble("Pause"))
	passed = 0
end sub
sub stop_delay_series()
	passed = -1
end sub

' ...РАБОТА С СЕРИЯМИ
'-*****************************************************************************************-'
'-*****************************************************************************************-'
'-*****************************************************************************************-'
 
sub StartTimerAutoTakeout(pause as double)
	auto_takeout_from = CInt((pause + stoper_a)*50.0)+10
end sub

function PlayheadIsLess(targetTime As Double) As Boolean
	playhead = d_OnOff.Time
	if playhead < targetTime-playheadTreshold then
		PlayheadIsLess = true
	else
		PlayheadIsLess = false
	end if
end function
function PlayheadIsNear(targetTime As Double) As Boolean
	playhead = d_OnOff.Time
	if playhead > targetTime-playheadTreshold AND playhead < targetTime+playheadTreshold then
		PlayheadIsNear = true
	else
		PlayheadIsNear = false
	end if
end function
function PlayheadIsMore(targetTime As Double) As Boolean
	playhead = d_OnOff.Time
	if playhead > targetTime+playheadTreshold then
		PlayheadIsMore = true
	else
		PlayheadIsMore = false
	end if
end function



function PlayheadBetween(timeA As Double, timeB As Double) As Boolean
	playhead = d_OnOff.Time
	if playhead > timeA+playheadTreshold AND playhead < timeB-playheadTreshold then
		PlayheadBetween = true
	else
		PlayheadBetween = false
	end if
end function

function PlayheadBetweenAndIncludeStoper(timeA As Double, timeB As Double) As Boolean
	playhead = d_OnOff.Time

	startInclude = playhead > timeA-playheadTreshold
	endInclude = playhead < timeB+playheadTreshold
	
	PlayheadBetweenAndIncludeStoper = startInclude AND endInclude
end function

function PlayheadBetweenAndIncludeLastStoper(timeA As Double, timeB As Double) As Boolean
	playhead = d_OnOff.Time

	startInclude = playhead > timeA+playheadTreshold
	endInclude = playhead < timeB+playheadTreshold
	
	PlayheadBetweenAndIncludeLastStoper = startInclude AND endInclude
end function


'''''''''''''''''''''''''''''''''''''''''''''''''''''
'обеспечение однокадровой паузы перед обнулением _control-переменной

dim reset_control_delay as integer = -1

sub reset_control_with_delay()
	reset_control_delay = 2
end sub

sub reset_control()
	memory[titr_name & "_control"] = 5
	local_memory[titr_name & "_control"] = 5
end Sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''

 
sub OnExecPerField()
 
	'считаем серийный счетчик:
	if MODE_SERIES == mode AND passed >= 0 then
		'значит если серийный режим
		'инкременируем счетчик
		passed += 1
		If passed == pause then
			'если счетчик досчтилал - выполнить:
			take_next_series()
		end if
	end if
	
	'если авто-убирание включено и действует отсчет:
	if auto_takeout == TRUE AND auto_takeout_from >= 0 then
		auto_takeout_from -= 1
		if auto_takeout_from <= 0 then
			local_memory[titr_name & "_control"] = 0
		end if
	end if
	
	'Ежекадровое понимание состояния директора
	if d_OnOff <> null then
		'определение в каком месте плейхед у d_OnOff
		playhead = d_OnOff.Time
		
		if isCanChange then
			if PlayheadIsNear(stoper_a) then	Change()
		end if
		
		if isCanINtoOUT then
			if PlayheadIsNear(stoper_b) then INtoOUT()
		end if
	end if
	
	'отслеживание однокадровой паузы перед обнулением _control переменной
	if reset_control_delay == 0 then
		reset_control_delay = -1
		reset_control()
	elseif reset_control_delay > 0 then
		reset_control_delay -= 1
	end if
end sub
