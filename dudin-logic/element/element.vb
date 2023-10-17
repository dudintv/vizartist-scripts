RegisterPluginVersion(4,3,8)
Dim info As String = "Developer: Dmitry Dudin
http://dudin.tv/scripts/logic
-------------------------------------------------------
Specify what elements leave the screen or vice versa be taken in.
Elements should be separated by comma.
All spaces are ignored.

1. When this appears: specify what to do when THIS ELEMENT IS ENTERED
2. When this disappear: specify what to do when THIS ELEMENT IS EXIT
3. When others appear: specify on what elements statuses THIS ELEMENT SHOULD ENTER
4. When others disappear: specify on what elements statuses THIS ELEMENT SHOULD EXIT

You can specify elements names with sign. +(plus) stands for another element ENTER, -(minus) for EXIT.
Example for 1 and 2 lines: +Logo, -Geo means to appear Logo element and disappear Geo.
Example for 3 and 4 lines: +Logo, -Geo means to react wether when Logo is entered or when Geo is exit.
If you do not specify the sign then -(minus) is used by default.

Additionally, you can specify triggering conditions [in square brackets]. By default it considers \"_status\" property of elements.
You can specify another property. As an example, you can react on the fill content — just specify \"_fill\", e.g. \"Geo_fill\"

Examples:
+Comments[+Comments_fill] — stands for appear Comments when this Comments element has the content to show.
-Comments,-Title,  +WeatherMarker — remove Comments and Title but enter WeatherMarker. Spaces are ignored!

------------------------------------------------
The element director should be call by exact its name. 
This director can have only 1 or 2 stop-points.
--------------------------------------------------
Serial mode enable the element work piece by piece. Reminds a flipping ticker.
It takes the element content from the \"Element_fill\" variable and split the value by Delimeter.
These pieces are taken one by one with the Pause.
When the element wants to show the next piece it plays the change animation between first and second stop-points.
----------------------------------------------------
If you enable \"Always start from the first\"
then: it always plays series from the very first item.
else: it appears the next piece after that it was onair when it was exit.

But, if the \"_fill\" content was changed (even one character)
then: it always plays from the very first piece.
"
 
 
'--------------------------------------------------------------------------------------
' You can use the prefix to distinguish two Dudin Logic systems on one machine.
' The prefix can be any. Keep in mind, it should be identical with the Global script of Dudin Logic.
Dim prefix = ""
Dim memory As SharedMemory = System.Map
Dim local_memory As SharedMemory = Scene.Map

Dim console As String           'place content for internal debug console in the script UI
 
Dim titr_name As String         'the element name
Dim separator As String         'delimiter for the Serial Mode
Dim mode      As Integer        '0 - means Single Mode, 1 - for Serial Mode
	Dim MODE_SINGLE As Integer = 0
	Dim MODE_SERIES As Integer = 1
Dim testers   As Integer        '0 - hide testing button, 1 - show the buttons
Dim firster   As Integer        '0 - it's allowed to start not from the first (see start_by_previous), 1 - always start from the first piece
Dim take_by_fill As Boolean
Dim d_OnOff   As Director       'the element director for all main animations: enter, change, exit
Dim nav_OnOff As Channel        'action-channel with "nav" name, stands for "navigation"
Dim feelfill  As Boolean        'should the elemend react on empty _fill. If feelfill=true then it's not possible to exter element with empty _fill
Dim isCanChange, isCanINtoOUT As Boolean  'one-time permissions to make Change or jump from "In to Out"
Dim is_needed_to_start_counter_for_taking_next_series As Boolean

Dim ctrl, fill, take, takeout, takethis, takeoutthis, cur As String
	
	'arrays keeping AutoTakeout logic rules
Dim take_arr, takeout_arr, takethis_arr, takeoutthis_arr As Array[String]
	
	'stop-points and border of the element director
Dim stoper_a, stoper_b As Double
Dim start_time, end_time As Double
	'do stop-points are included in the range
Dim startInclude, endInclude As Boolean
	'animation playhead time and its sensitivity treshold
Dim playhead As Double
Dim playheadTreshold As Double = 0.1

	'the main(root) container of the element where it looks for dropzones for the element content
Dim cRoot As Container

	'additional animation directors. They are needed in case using objects DZ_TYPE_OBJECT with internal animations.
Dim arr_dirObjects As Array[Director]

	'clipchannels keyframes
Dim arr_clipKeys As Array[Keyframe]
 
'-**********************************************************************************-'
	'keep the current index for the Serial Mode
Dim curSeries As Integer

	'array to store _fill pieces in case of Serial Mode
Dim arr_fill As Array[String]

	'pause between pieces in Serial Mode (in frames, in PAL system 50 means one second)
Dim pause As Integer

	'elapsed time counter after triggering start_delay_series()
	'value "-1" means do not calculate
	'incremented until the value in the "pause" variable
Dim passed As Integer = -1

	'should is always start fro mthe first piece?
Dim start_by_first As Boolean

	'if to not start from the first, than start from the last visible or from the next one?
Dim start_by_previous As Boolean

	'should to exit if the last piece is finished
Dim takeout_by_last As Boolean

	'auto-exit by timer?
Dim auto_takeout As Boolean
	'duration for auto-exit — the starting value for "auto_takeout_from"
Dim auto_takeout_pause As Double
	'countdown until 0(zero)
Dim auto_takeout_from As Integer


	'conditions for auto-exit
Dim takeout_by_last_condis As String
	'loops amout before auto-exit (works for Serial Mode)
Dim takeout_by_last_countLoop As Integer
Dim i_curLoop As Integer = 1
'-***********************************************************************-'

'temporal varialbes (use with awarness)
Dim s, nametype As String
Dim i AS Integer

'Examples of "_fill":
'Geo:text=Amsterdam
'Name=Dmitry Dudin|Status=working on that

'LOG:
Sub Log(message As String)
	if System.Map["DUDIN_LOGIC_LOG"] == "1" then
		println(3, "DL::" & message)
	end if
End Sub

'DROPZONES A/B SIDES:
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

' Supported dropzones types:
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
	c As Container   'changable container of the Dropzone
	order As Integer 'ordered index, from top to bottom, only within the element-root sub-tree
	side As Integer  'relation for the Change animation — wether the element has single dropsone or 1 and 2 for the smooth change DZ_MODE
	name As String   'dropzone's name
	type As String   'data type of the dropzone
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
		
		'it searched dropzones by the following convention:
		'dropzones containers names start from "=" sign
		if name_child.left(1) <> "=" then
			all_childs.Erase(i)
			i -= 1
		end if
	next
	
	for i = 0 to all_childs.ubound
		
		Dim dz As Dropzone
		dz.c = all_childs[i]
		
		name_child = all_childs[i].name
		
		'storing merged-object animations to play them accordingly
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
			dz.side = DZ_SIDE_ONLY_ONE
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
			if side == dz.side OR dz.side == DZ_SIDE_ONLY_ONE then
				'when the name, dropzone type and side are the same
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
	'Need to discover a way fo find keyframes independently of key... Temporarly, I leave this:
	FindClipChannelKey = Stage.FindDirector("Clip" & i).FindKeyframe("clip" & i)
End Function

Sub SetClipChannel(num As Integer, path As String)
	System.SendCommand("#" & arr_clipKeys[num].VizId & "*CLIPNAME SET " & path)
End Sub

Sub SetClipLoop(num As Integer, is_loop As Boolean)
	arr_clipKeys[num].channel.PostLoopActive = is_loop
	System.SendCommand("#" & arr_clipKeys[num].channel.VizId & "*POST_LOOP_INFINITE SET " & CInt(is_loop))
End Sub
 
'-----------------------------------------------------------------------------------------------------------
'Shared Memory variables conventions:
'(imagine your element name insteal of "Element")
'
'System.Map["Element_control"]
'considerables values:
'-1 - unconditionaly exit this element. Disables all AutoTakeout logic.
'0 - exit this element
'1 - enter this element if possible
'2 - do Change, if it's exit then enter the lement
'3 - switch the element, from on to off, from off to on
'4 - preview the element, it means show the element immediately. Technically it does show(take_b)+continue
'5 - for the variable resetting allows to react on the variable changing
'
'System.Map["Element_status"]
'almost similar 'System.Map["Element_control"]
'but it guaranties to keep only one of two values: 0 or 1
'
'System.Map["Element_fill"]
'this variable stores the content for the Element,
'for the Serial Mode it can contains data pieces separated by the Delimiter.
'
'System.Map["Element_value"]
'it contains specific value of the current moment.
'it contains ""(empty string) when the element is exit
'
'System.Map["Element_previous"]
'is contains the elment fill of the previous _fill value
'it's useful to define Serial Mode logic
'
'System.Map["Element_curSeries"]
'the inder of the current piece in Serial Mode
'
'
'...instead of "Element" insert the Element name!
'----------------------------------------------------------
 
Sub OnInitParameters()	
	RegisterParameterString("Name", "Name:", "", 30, 256, "")
	RegisterParameterBool("Mode", "Serial mode", false)
	RegisterParameterString("Separator", "        └ Delimeter (newline is \\\\n):", "\\n", 10, 32, "")
	RegisterParameterDouble("Pause", "        └ Pause (sec):", 5, 0, 10000)
	RegisterParameterBool("Start_by_first", "└ Always start from the first", false)
	RegisterParameterBool("Start_by_previous", "    └─ from the last onee", false)
	RegisterParameterBool("Takeout_by_last", "└ Takeout after the last", false)
	RegisterParameterString("Takeout_by_last_condis", "            └─ Conditions a&b", "", 40, 256, "")
	RegisterParameterInt("Takeout_by_last_countLoop", "            └─ Cicle Count", 1, 1, 10000)
	RegisterParameterBool("LogicAutoTakeout", "AutoTakeout logic", false)
	RegisterParameterString("Take", "└ When it take        a,b(c&d)", "", 65, 256, "")
	RegisterParameterString("Takeout", "└ When it takeout     a,b(c&d)", "", 65, 256, "")
	RegisterParameterString("TakeThis", "└ When anothers take a,b(c&d)", "", 65, 256, "")
	RegisterParameterString("TakeoutThis", "└ When anothets takeout    a,b(c&d)", "", 65, 256, "")
	RegisterParameterBool("FeelFill", "Takeout if fill is empty", false)
	RegisterParameterBool("TakeByFill", "Take by changing of fill", false)
	RegisterParameterBool("AUTOTAKEOUT", "Timer of auto-takeout", false)
	RegisterParameterDouble("AUTOTAKEOUTPause", "└ delay (sec):", 0, 0, 10000)
	RegisterParameterContainer("root", "Root of element:")
	RegisterPushButton("rebuild", "Initialize", 1)
	RegisterInfoText(info)
	RegisterParameterText("console", "After changing the parameters, click [Initialize] 
so that they are applied immediately.
It happens also when the scene is loaded.", 450, 50)
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
	'define the Element name
	titr_name = GetParameterString("Name")
	titr_name.trim()
	If titr_name = "" Then
		'if the Name field is empty, then let's try to get it from the container name
		If this.name.find("|") > 1 Then
			'smart-name is detected — let's take the name and AutoTakeout logic from the container name
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
			'if it's not possible to detect the name
			console &= "> There is no element name! :(" & "\nI can't work without the name!\n"
			exit sub
		End If
	End If
	If titr_name.Find("_") <> -1 Then
		console &= "> Attention! Underscore does not guarantie stable work.\n"
	End If
	'------------------
	'reset _fill
	memory[titr_name & "_fill"] = ""
	local_memory[titr_name & "_fill"] = ""
	'let's react on these variables
	memory.RegisterChangedCallback(titr_name & "_control")
	memory.RegisterChangedCallback(titr_name & "_fill")
	'additionally checks these variables
	local_memory.RegisterChangedCallback(titr_name & "_control")
	local_memory.RegisterChangedCallback(titr_name & "_fill")
	'and watch the global variable
	memory.RegisterChangedCallback(prefix & "AUTOTAKEOUT_ALL_RECALCULATE")
 
	'enable director with its channes and keyframes
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
		'cosider newline delimiter
		separator = "\n"
	End If
	start_by_first = GetParameterBool("Start_by_first")
	start_by_previous = GetParameterBool("Start_by_previous")
	takeout_by_last = GetParameterBool("Takeout_by_last")
	pause = CInt(50.0 * GetParameterDouble("Pause"))
	takeout_by_last_condis = GetParameterString("Takeout_by_last_condis")
	takeout_by_last_countLoop = GetParameterInt("Takeout_by_last_countLoop")
 
'---parameters of AutoTakeout after pause
	auto_takeout = GetParameterBool("AUTOTAKEOUT")
	auto_takeout_pause = GetParameterDouble("AUTOTAKEOUTPause")
	auto_takeout_from = 0
 
'---create arrays of elements names for reactions
	take.Split(",",take_arr)
	takeout.Split(",",takeout_arr)
	takethis.Split(",",takethis_arr)
	takeoutthis.Split(",",takeoutthis_arr)
 
'---apply the element container name and UI color
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

	'send the AutoTakeout to the Dudin Logic global script
	SendConditionsToGlobal()
	
	'reset starting values in the local variables
	local_memory[titr_name & "_value"] = ""
	local_memory[titr_name & "_control"] = 5
	local_memory[titr_name & "_status"] = 0
	'disable the timer
	passed = -1
	is_needed_to_start_counter_for_taking_next_series = false
	
 
	'reset the animation
	d_OnOff.Show(0)
	
	'set ClipChannel keys
	arr_clipKeys.Clear()
	arr_clipKeys.Push(null)
	for i=1 to 4
		arr_clipKeys.Push(FindClipChannelKey(i))
	next
	
	'setup all dropzons
	SetDropzones()
'---initialization is finished
end sub

Sub SendConditionsToGlobal()
	'---fill arrays for AutoTakeout logic
	Dim AUTOTAKEOUTonTAKE As Array[Array[String]]
	Dim AUTOTAKEOUTonTAKEOUT As Array[Array[String]]
	Dim cur_arr As Array[String]
	
	'-------for ENTER
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
	
	'-------for EXIT
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
	
	'-------for ENTER THIS
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
	
	'-------for EXIT THIS
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
	
	'sent the arrays to the global DL script
	local_memory[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = null
	local_memory[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"] = null
	
	If GetParameterBool("LogicAutoTakeout") Then
		local_memory[prefix & "AddTo_AUTOTAKEOUTonTAKE"] = AUTOTAKEOUTonTAKE
		local_memory[prefix & "AddTo_AUTOTAKEOUTonTAKEOUT"] = AUTOTAKEOUTonTAKEOUT
	End If
End Sub
'----------------------------------------------------------
 
Sub OnExecAction(buttonId As Integer)
	If buttonId = 1 Then
		'press "Initialize"
		console = ""
		'run standard initializations
		OnInitParameters()
		OnInit()
		're-calculate all the Autotakeout logic
		memory[prefix & "AUTOTAKEOUT_ALL_RECALCULATE"] = ""
		memory[prefix & "AUTOTAKEOUT_ALL_RECALCULATE"] = titr_name
 
		'print out the result of the work
		'in case if everything is fine then prnt "OK"
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
	'follow selection of SINGLE or SERIES mode
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
		SendGuiParameterShow("FastPreview", SHOW)
	Else
		SendGuiParameterShow("TestTake", HIDE)
		SendGuiParameterShow("TestTakeout", HIDE)
		SendGuiParameterShow("TestChange", HIDE)
		SendGuiParameterShow("TestOnOff", HIDE)
		SendGuiParameterShow("TestFill", HIDE)
		SendGuiParameterShow("TestMakeFill", HIDE)
		SendGuiParameterShow("FastPreview", HIDE)
	End If
end sub
'----------------------------------------------------------
 
Sub OnSharedMemoryVariableChanged (map As SharedMemory, mapKey As String)
	Dim test_point As Vertex = Scene.ScreenPosToWorldPos(99987,99987)
	if test_point.x == 0 AND test_point.y == 0 AND test_point.z == 0 then
		Log("Scene is not in render layer. It's stored in the scene pool.")
		exit sub
	end if
	
	Log(CStr(map))
	' CONTROL
	If mapKey = titr_name & "_control" Then
		'triggers by the _control variable
		ctrl = map[titr_name & "_control"]
		Log(titr_name & "_control = " & ctrl)
		
		'auto-reset control after two frames in order to keep opportunity to react on the same value
		if ctrl <> "5" Then
			reset_control_with_delay()
		end if
		
		If ctrl = "1" Then
			'TAKE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			'check if we really can enter the element
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
				'enter for sure:
				Log("DO TAKE")
				d_OnOff.Show(0)
				d_OnOff.ContinueAnimation()
				take()
			End If
			'reflect in the status
			local_memory[titr_name & "_status"] = 1
			'run ClipChannels
			StartClips()
			'run merged geometries animations
			StartAnimGeoms()
		ElseIf ctrl = "0" OR ctrl = "-1" Then
			'TAKEOUT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			'Check if the playhead in the "in" range then exit
			isCanChange = false
			isCanINtoOUT = false
			'если реагируем на fill, то обнуляем fill
			if take_by_fill then
				memory[titr_name & "_fill"] = ""
				local_memory[titr_name & "_fill"] = ""
			end if
			
			If PlayheadIsMore(0) AND PlayheadIsLess(end_time) Then
				If PlayheadIsLess(stoper_b) Then d_OnOff.Show(stoper_b)	
				d_OnOff.ContinueAnimation()
				takeout_change()
			End If
			'reflect in the status
			local_memory[titr_name & "_status"] = 0
			
			
		ElseIf ctrl = "2" Then
			'CHANGE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			'Check if the Element is entered then play Change animation
			fill = map[titr_name & "_fill"]
			fill.trim()
			If PlayheadBetweenAndIncludeLastStoper(0,stoper_b) Then
				If feelfill AND fill = "" then
					local_memory[titr_name & "_control"] = 0
					exit sub
				End If
				
				If stoper_a == stoper_b Then
					'if there is no Change animation
					d_OnOff.Show(0)
					local_memory[titr_name & "_control"] = 1
				Else
					'если есть блок loop
					'if there is Change animation
					if fill <> local_memory[titr_name & "_value"] then
						d_OnOff.Show(stoper_a)
						change()
						d_OnOff.ContinueAnimation()
					end if
				End If
				'reflect in the status
				local_memory[titr_name & "_status"] = 1
			ElseIf PlayheadIsNear(0) OR PlayheadIsMore(stoper_b) Then
				'in case the Element is exit then enter it
				If FillIsExist() OR NOT feelfill then
					local_memory[titr_name & "_control"] = 1
					exit sub
				End if
			End If
			StartClips()
		ElseIf ctrl = "3" Then
			'ON/OFF !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			If PlayheadBetweenAndIncludeLastStoper(0,stoper_b) Then
				local_memory[titr_name & "_control"] = 0
			Else
				local_memory[titr_name & "_control"] = 1
			End If
		ElseIf ctrl = "4" Then
			'PREVIEW - quickly show entered Element, without animation
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
		
		If FeelFill AND NOT FillIsExist() then
			local_memory[titr_name & "_control"] = 0
		elseif take_by_fill AND FillIsExist() then
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
	'find the Element director and its keyframes
 
	d_OnOff = Stage.FindDirector (titr_name)
	If d_OnOff = null Then
		'if it's not possible to find :(
		console &= "> Не смог найти основной директор!\n" & "Он должен быть назван \"" & titr_name & "\"\n"
	Else
		'it's found!
		if d_OnOff.EventChannel.KeyframeCount < 1 then
			console &= "> В основном директоре нет стоперов!\n" & "Надо добавить минимум один стопер.\n"
			exit sub
		elseif d_OnOff.EventChannel.KeyframeCount > 2 then
			console &= "> В основном директоре многовато стоперов!\n" & "Надо сократить до 2-х!\n"
			exit sub
		end if
		
		stoper_a = d_OnOff.EventChannel.FirstKeyframe.Time
		stoper_b = d_OnOff.EventChannel.LastKeyframe.Time
		
		start_time = CDbl(  System.SendCommand("#" & Scene.VizId & "*STAGE*#" & d_OnOff.VizID & "*START_TIME GET")  )
		end_time   = CDbl(  System.SendCommand("#" & Scene.VizId & "*STAGE*#" & d_OnOff.VizID & "*END_TIME GET"  )  )
	End If
End Sub

'----------------------------------------------------------

Sub take()
	If mode == MODE_SERIES Then
		If start_by_first == TRUE Then
			'if we have to start from the very first piece
			local_memory[titr_name & "_curSeries"] = 0
		Else
			'start by situation
			If local_memory[titr_name & "_value"] == local_memory[titr_name & "_previous"] Then
				'if the current _fill is equal to the previous
				If start_by_previous == TRUE Then
					'don't do anything
				Else
					local_memory[titr_name & "_curSeries"] = local_memory[titr_name & "_curSeries"] + 1
				End If
			Else
				'in case the current is different to the previous
				local_memory[titr_name & "_curSeries"] = 0
			End If
		End If
 
		i_curLoop = 1
		fill = take_cur_series()
		start_delay_series()
	Else
		fill = memory[titr_name & "_fill"]
	End If
	fill.Trim()
	local_memory[titr_name & "_value"] = fill
 
	If cRoot <> null Then
		SendFillToDropzones(fill,DZ_SIDE_FIRST)
		SendFillToDropzones(fill,DZ_SIDE_SECOND)
	End If
 
	'enable the auto-exit timer, if needed
	auto_takeout = GetParameterBool("AUTOTAKEOUT")
	If auto_takeout == TRUE Then
		auto_takeout_pause = GetParameterDouble("AUTOTAKEOUTPause")
		StartTimerAutoTakeout(auto_takeout_pause)
	End If
	
	StartObjectDirectors()
End Sub

'----------------------------------------------------------
 
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
 
'procedure for sinchronization of Change animation keyframes
'it runs in the "loop_d" keyframe
Sub INtoOUT()
	If cRoot = null Then Exit Sub
	isCanINtoOUT = false
	fill = local_memory[titr_name & "_value"]
	fill.trim()
	SendFillToDropzones(fill,DZ_SIDE_FIRST)
End Sub

'----------------------------------------------------------
 
Sub takeout_change()
	'disable timer in any case
	stop_delay_series()
 
	'set the previous value
	fill = CStr(local_memory[titr_name & "_fill"])
	fill.Trim()
	local_memory[titr_name & "_previous"] = fill
 
	'empty the current value
	local_memory[titr_name & "_value"] = ""
End Sub

'----------------------------------------------------------

Sub StartObjectDirectors()
	for i=0 to arr_dirObjects.UBound
		arr_dirObjects[i].StartAnimation()
	next
	
End Sub

'start ClipChannels
Sub StartClips()
	for y=0 to arr_dropzones.ubound
		if arr_dropzones[y].type.length == 5 AND arr_dropzones[y].type.left(4) == "clip" then
			i = CInt(arr_dropzones[y].type.right(1))
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
' WORK WITH SERIES...
function take_cur_series() as String
	if mode == MODE_SERIES then
		'let's calculate which piece is shown and return the next one
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
			'if there is at least one piece:
			for i = 0 to arr_fill.UBound
				s = arr_fill[i]
				s.Trim()
				If s == "" Then arr_fill.Erase(i)
			next
			
			if arr_fill.Size <= 1 then
				arr_fill.push("")
				curSeries = 0
			else
				'if the current piece index is more then reset
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
			'if there is no pieces
			local_memory[titr_name & "_curSeries"] = 0
			take_cur_series = ""
		end if
	else
		'in case non-series mode (Single mode)
		take_cur_series = ""
	end if
end function
 
'take the next piece with a delay
sub take_next_series()
	is_needed_to_start_counter_for_taking_next_series = true
	Log("SET is_needed_to_start_counter_for_taking_next_series = true")
	
	if mode == MODE_SERIES then
		'figure out what piece is extered and enter the next one
		curSeries = CInt(local_memory[titr_name & "_curSeries"])
		'zero index means the first piece
		if curSeries < 0 Then curSeries = 0
 
		fill = memory[titr_name & "_fill"]
		fill.trim()
		fill.split(separator,arr_fill)
		
		if arr_fill.Size > 0 Then
			for i = 0 to arr_fill.UBound
				'remove empty pieces
				s = arr_fill[i]
				s.Trim()
				If s == "" Then arr_fill.Erase(i)
			next
		end if
 		
		If arr_fill.UBound == 0 Then
			If arr_fill[0] == local_memory[titr_name & "_value"] Then
				'in case the piece is only one and identical to the previous — no Change animation
				exit sub
			Else
				'if there is only one piece and it's different — play the Change animation
			End If
		End If
 
		'Takeout conditions:
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
			'start FILL
 
			If s_condisItem.Left(1) = "+" Then
				s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				s_condisFill = CStr(memory[s_condisItem])
				s_condisFill.Trim()
				If s_condisFill == "" Then
					'condition not met 
					CheckCondis = FALSE
					Exit Function
				End If
			Else
				If s_condisItem.Left(1) = "-" Then s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				s_condisFill = CStr(memory[s_condisItem])
				If s_condisFill <> "" Then
					'condition not met 
					CheckCondis = FALSE
					Exit Function
				End If
			End If
			'end FILL
		Else
			'start STATUS
			If s_condisItem.Left(1) = "+" Then
				s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				If CInt(local_memory[s_condisItem & "_status"]) <> 1 Then
					'condition not met 
					CheckCondis = FALSE
					Exit Function
				End If
			Else
				If s_condisItem.Left(1) = "-" Then s_condisItem = s_condisItem.Right(s_condisItem.Length - 1)
				If CInt(local_memory[s_condisItem & "_status"]) <> 0 Then
					'condition not met 
					CheckCondis = FALSE
					Exit Function
				End If
			End If
			'end STATUS
		End If
	Next
 
	CheckCondis = TRUE
End Function

Function FillIsExist() As Boolean
	Dim arr_data As Array[String]
	fill.split("|",arr_data)
	for i=0 to arr_data.ubound
		if arr_data[i].find("=") < arr_data[i].length-1 then
			FillIsExist = true
			Exit Function
		end if
	next
	FillIsExist = false
End Function
 
sub start_delay_series()
	pause = CInt(50.0 * GetParameterDouble("Pause"))
	is_needed_to_start_counter_for_taking_next_series = true
	Log("SET is_needed_to_start_counter_for_taking_next_series = true IN start_delay_series()")
end sub
sub stop_delay_series()
	passed = -1
	is_needed_to_start_counter_for_taking_next_series = false
end sub

' ...END WORKING WITH SERIES
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
'provide one-frame pause before resetting of _control variable

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
	if MODE_SERIES == mode AND passed >= 0 then
		'increment the counter in case Series Mode
		passed += 1
		If passed == pause then
			'when the counter reach the pause value
			take_next_series()
		end if
	end if
	
	if auto_takeout == TRUE AND auto_takeout_from >= 0 then
		'if AutoTakeout is enabled and counter is working
		auto_takeout_from -= 1
		if auto_takeout_from <= 0 then
			local_memory[titr_name & "_control"] = 0
		end if
	end if
	
	'every frame consider the director
	if d_OnOff <> null then
		'find the playhead
		playhead = d_OnOff.Time
		
		'if isCanChange then
		'	if PlayheadIsNear(stoper_a) then	Change()
		'end if
		
		if isCanINtoOUT then
			if PlayheadIsNear(stoper_b) then INtoOUT()
		end if
		
		if mode == MODE_SERIES AND is_needed_to_start_counter_for_taking_next_series then
			if PlayheadIsNear(stoper_a) OR PlayheadIsNear(stoper_b) then
				passed = 0
				is_needed_to_start_counter_for_taking_next_series = false
			end if
		end if
	end if
	
	'consider one-frame pause for resetting _control variable
	if reset_control_delay == 0 then
		reset_control_delay = -1
		reset_control()
	elseif reset_control_delay > 0 then
		reset_control_delay -= 1
	end if
end sub
