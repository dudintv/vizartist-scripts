RegisterPluginVersion(5,1,5)
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
This director must have only 1 or 2 stop-points.
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

Structure FieldMapping
	name As String
	type As String
End Structure

Structure DataSet
	c As Container
	name As String
End Structure

Structure DropzoneGroup
	c As Container
	name As String
	order As Integer
	side As Integer
End Structure

Structure Dropzone
	c As Container   'changable container of the Dropzone
	group As Integer 'ordered group number for multi-Ticker
	order As Integer 'ordered index, from top to bottom, only within the element-root sub-tree
	side As Integer  'relation for the Change animation — wether the element has single dropsone or 1 and 2 for the smooth change DZ_MODE
	name As String   'dropzone's name
	type As String   'data type of the dropzone
	prop As String   'properpty of the type of the dropzone
	propType As String 'prop type of the dropzone, e.g. "string", "integer", "float", "boolean" (no shorthands)
End Structure

Dim arr_dropzones As Array[Dropzone]
'Dim arr_dropzoneContainers, arr_dropzoneGroups As Array[Container]
Dim arr_dropzoneGroups As Array[DropzoneGroup]
Dim arr_dataSets As Array[DataSet]
Dim arr_dz_mappings As Array[Array[FieldMapping]]

Function GetDropzoneGroupByOrderAndSide(dzgOrder As Integer, side As Integer) As DropzoneGroup
	for dzg=0 to arr_dropzoneGroups.ubound
		if arr_dropzoneGroups[dzg].order == dzgOrder AND arr_dropzoneGroups[dzg].side == side then
			GetDropzoneGroupByOrderAndSide = arr_dropzoneGroups[dzg]
			exit Function
		end if
	next
End Function
Function IsDropzoneGroupExistByOrder(dzgOrder As Integer) As Boolean
	for dzg=0 to arr_dropzoneGroups.ubound
		if arr_dropzoneGroups[dzg].order == dzgOrder then
			IsDropzoneGroupExistByOrder = true
			exit Function
		end if
	next
	IsDropzoneGroupExistByOrder = false
End Function
Function IsDropzoneGroupExistByName(dzName As String) As Boolean
	for dzg=0 to arr_dropzoneGroups.ubound
		if arr_dropzoneGroups[dzg].name == dzName then
			IsDropzoneGroupExistByName = true
			exit Function
		end if
	next
	IsDropzoneGroupExistByName = false
End Function
Function GetDropzoneGroupByName(dzName As String) As DropzoneGroup
	for dzg=0 to arr_dropzoneGroups.ubound
		if arr_dropzoneGroups[dzg].name == dzName then
			GetDropzoneGroupByName = arr_dropzoneGroups[dzg]
			exit Function
		end if
	next
End Function
Function GetCurrentDropzoneGroupsOrder() As Integer
	GetCurrentDropzoneGroupsOrder = arr_dropzoneGroups[arr_dropzoneGroups.ubound].order
End Function

Function GetDataSetByName(dataSetName As String) As Container
	for ds=0 to arr_dataSets.ubound
		if arr_dataSets[ds].name == dataSetName then
			GetDataSetByName = arr_dataSets[ds].c
			Exit Function
		end if
	next
	GetDataSetByName = Null
End Function

Sub SetOrderedMapping()
	if NOT GetParameterBool("use_mapping") then exit sub

	Dim arr_s_mappings, arr_s_mappring_line As Array[String]
	GetParameterString("mapping").split("\n", arr_s_mappings)

	arr_dz_mappings.Clear()
	for i=0 to arr_s_mappings.ubound
		arr_s_mappings[i].split("=", arr_s_mappring_line)
		arr_s_mappring_line[0].trim()
		if arr_s_mappring_line.ubound >= 1 then arr_s_mappring_line[1].trim()
		Dim mapping As FieldMapping
		Dim separatedDropzoneName As Array[String]
		arr_s_mappring_line[1].split(":", separatedDropzoneName)
		mapping.name = separatedDropzoneName[0]

		if separatedDropzoneName.ubound >= 1 then mapping.type = separatedDropzoneName[1]
		if mapping.type == "" OR mapping.type == "string" then mapping.type="text"
		if mapping.type == "image" then mapping.type = "texture"
		if mapping.type == "color" then mapping.type = "material"

		Dim cur_col_index = CInt(arr_s_mappring_line[0])
		arr_dz_mappings.size = Max(arr_dz_mappings.size, cur_col_index+1)

		if arr_dz_mappings[cur_col_index].size == 0 then
			Dim _mappings As Array[FieldMapping]
			_mappings.Push(mapping)
			arr_dz_mappings.insert(cur_col_index, _mappings)
		else
			arr_dz_mappings[cur_col_index].Push(mapping)
		end if
	next
End Sub

Sub SetDropzones()
	arr_dropzoneGroups.Clear()
	Dim defaultGroup As DropzoneGroup
	arr_dropzoneGroups.Push(defaultGroup)
	arr_dataSets.Clear()
	arr_dropzones.Clear()
	arr_dirObjects.Clear()
	Dim all_childs As Array[Container]
	Dim name_child, types, type, prop, propType As String
	Dim arr_types As Array[String]
	Dim order1, order2, groupOrder As Integer
	cRoot.GetContainerAndSubContainers(all_childs, false)
	for i = 0 to all_childs.ubound
		name_child = all_childs[i].name
		name_child.Trim()

		'it searched dropzones by the following convention:
		'  sets containers names start with "set="
		'  dropzones containers names start with one "=" sign
		'  dropzone groups conatiners names start with "=="

		Dim isDataSet = name_child.left(4) == "set="
		if isDataSet then
			Dim s_dataSetNames = all_childs[i].name.GetSubstring(4, all_childs[i].name.length - 4)
			Dim arr_dataSetsNames As Array[String]
			s_dataSetNames.split(",", arr_dataSetsNames)
			for ds=0 to arr_dataSetsNames.ubound
				Dim dataSet As DataSet
				dataSet.c = all_childs[i]
				dataSet.name = arr_dataSetsNames[ds]
				arr_dataSets.Push(dataSet)
			next
		end if

		Dim isChildDropzoneGroup = name_child.left(2) == "=="
		if isChildDropzoneGroup then
			name_child.Erase(0, 2)
			Dim dropzoneGroup As DropzoneGroup
			dropzoneGroup.c = all_childs[i]

			if name_child.StartsWith("1") OR name_child.StartsWith("2") then
				dropzoneGroup.side = CInt(name_child.left(1))
				name_child.Erase(0, 1)
			else
				dropzoneGroup.side = DZ_SIDE_ONLY_ONE
			end if
			dropzoneGroup.name = name_child
			if IsDropzoneGroupExistByName(name_child) then
				dropzoneGroup.order = GetDropzoneGroupByName(name_child).order
			else
				groupOrder += 1
				dropzoneGroup.order = groupOrder
			end if

			arr_dropzoneGroups.push(dropzoneGroup)

			'reset orders for dropzones
			order1 = 0
			order2 = 0
		end if

		Dim isChildDropzone = name_child.left(1) == "=" AND name_child.left(2) <> "=="
		if isChildDropzone then
			Dim dz As Dropzone
			dz.c = all_childs[i]
			dz.group = GetCurrentDropzoneGroupsOrder()

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
					if type == "" then type="text"

					if type.find(".") > 0 then
						'has defined property for the dropzone
						Dim arr_type_with_props as Array[String]
						type.split(".", arr_type_with_props)
						type = arr_type_with_props[0]
						prop = arr_type_with_props[1]
						if arr_type_with_props.ubound >= 2 then
							propType = arr_type_with_props[2]
							if propType == "int" OR propType == "i" then propType = "integer"
							if propType == "str" OR propType == "s" then propType = "string"
							if propType == "double" OR propType == "f" then propType = "float"
							if propType == "bool" OR propType == "b" then propType = "boolean"
						else
							propType = "" 'fallback default type
						end if
					else
						'no prop
						prop = ""
						propType = ""
					end if

					if type == "string" then type="text"
					if type == "image" then type = "texture"
					if type == "color" then type = "material"

					dz.type = type
					dz.prop = prop
					dz.propType = propType
					Dim ddzz As Dropzone = dz
					arr_dropzones.Push(ddzz)
				next
			else
				dz.name = name_child
				dz.type = "text"
				arr_dropzones.Push(dz)
			end if
		end if
	next

	if arr_dataSets.size > 0 then
		console &= "\nSETS:\n"
		for i=0 to arr_dataSets.ubound
			console &= "  " & arr_dataSets[i].name & "\n"
		next
	end if

	if arr_dropzones.size > 0 then
		console &= "\nFIELDS:\n"
		Dim current_group = 0
		for i=0 to arr_dropzones.ubound
			if arr_dropzones[i].group > 0 AND arr_dropzones[i].group <> current_group then
				console &= "  Group ["& arr_dropzones[i].group & "]:\n"
				current_group = arr_dropzones[i].group
			end if
			if arr_dropzones[i].group > 0 then
				console &= "  "
			end if
			console &= "  " & arr_dropzones[i].name & "[" & arr_dropzones[i].side & "]" & " : " & arr_dropzones[i].type
			if arr_dropzones[i].prop <> "" then
				console &= " : " & arr_dropzones[i].prop
			end if
			if arr_dropzones[i].propType <> "" then
				console &= "." & arr_dropzones[i].propType
			end if
			'console &= " order: " & arr_dropzones[i].order
			console &= "\n"
		next
	Else
		console &= "\nNO FIELDS\n"
	end if
End Sub

Sub FillDropzones(fill As String, side As Integer)
	' TODO: process array type in fill for groups...

	if GetParameterBool("use_groups") then
		Dim arr_grouped_fills As Array[String]
		fill.split(separator, arr_grouped_fills)

		arr_grouped_fills.size = Max(arr_grouped_fills.size, GetCurrentDropzoneGroupsOrder())

		for group_index=0 to arr_grouped_fills.ubound
			Dim group = group_index + 1 '"i+1" because defined groups starts from "1"
			Dim currDropzoneGroup = GetDropzoneGroupByOrderAndSide(group, side)
			Dim isGroupSideOk = currDropzoneGroup.side == side OR currDropzoneGroup.side == DZ_SIDE_ONLY_ONE
			if isGroupSideOk then
				Dim p_groupOmo As PLuginInstance = currDropzoneGroup.c.GetFunctionPluginInstance("Omo")
				if p_groupOmo <> null then
					p_groupOmo.SetParameterInt("vis_con", CInt(arr_grouped_fills[group_index] <> ""))
				else
					GetDropzoneGroupByOrderAndSide(group_index, side).c.active = fill <> ""
				end if
			end if

			SendSingleFillToDropzones(arr_grouped_fills[group_index], side, group)
		next
	else
		SendSingleFillToDropzones(fill, side, 0) '"0" is for undefined (default) group
	end if
End Sub

Sub SendSingleFillToDropzones(fill As String, side As Integer, group As Integer)
	Dim arr_data As Array[String]
	Dim name, type, data As String
	Dim dz As Dropzone
	Dim arr_xyz As Array[String]
	Dim dataFieldOrder, precision As Integer

	fill.split("|", arr_data)
	dataFieldOrder = 1

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
		if type == "image" then type = "texture"
		if type == "color" then type = "material"
		data.Trim()

		for y=0 to arr_dropzones.ubound
			dz = arr_dropzones[y]

			'println(":::: group = " & group & " =?= dz.group = " & dz.group)
			Dim isGroupOk = group == dz.group
			Dim isDropzoneOk = false
			if GetParameterBool("use_mapping") then
				for i_dz_map=0 to arr_dz_mappings[dataFieldOrder].ubound
					if arr_dz_mappings[dataFieldOrder][i_dz_map].name == dz.name then
						isDropzoneOk = True
					end if
				next
			else
				isDropzoneOk = name == dz.name OR (name == "" AND dataFieldOrder == dz.order)
			end if
			Dim isSideOk = side == dz.side OR dz.side == DZ_SIDE_ONLY_ONE
			Dim isTypeOk = type == dz.type
			if GetParameterBool("use_mapping") then
				for i_dz_map=0 to arr_dz_mappings[dataFieldOrder].ubound
					if arr_dz_mappings[dataFieldOrder][i_dz_map].type == dz.type then
						isTypeOk = True
					end if
				next
			end if
			if isGroupOk AND isDropzoneOk AND isSideOk AND isTypeOk then
				'println("---> name = " & dz.side & dz.name & " | type = " & dz.type & " | prop = " & dz.prop & " | propType = " & dz.propType)
				'println("data = " & data)
				Select Case dz.type
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
				Case "text"
					dz.c.Geometry.Text = data
				Case "integer"
					s = CStr(CInt(data))
					dz.c.Geometry.Text = s
				Case "float"
					data.Substitute(",", ".", true)
					if dz.prop <> "" then
						precision = CInt(dz.prop)
					else
						precision = 2 'fallback default precision
					end if
					if data.FindLastOf(".") >= 0 then precision = data.length - data.FindLastOf(".") - 1
					s = DoubleToString(  CDbl(data), precision  )
					s.Substitute("\\.", ",", true)
					dz.c.Geometry.Text = s
				Case "texture"
					if dz.prop == "" then
						dz.c.CreateTexture(data)
					elseif dz.prop == "set" AND dz.propType <> "" then
						dz.c.Texture = GetDataSetByName(dz.propType).GetChildContainerByIndex(CInt(data)).Texture
					end if
					dz.c.Update()
				Case "material"
					if dz.prop == "" then
						dz.c.CreateMaterial(data)
					elseif dz.prop == "set" and dz.propType <> "" then
						dz.c.Material = GetDataSetByName(dz.propType).GetChildContainerByIndex(CInt(data)).Material
					end if
					dz.c.Update()
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
				Case "script"
					'for the script dropzone you have to provide the prop name and type
					'e.g. "=parliament:script.input.string"
					if dz.prop <> "" AND dz.propType <> "" then
						Dim pScript = dz.c.ScriptPluginInstance
						Select Case dz.propType
						Case "integer"
							pScript.SetParameterInt(dz.prop, CInt(data))
						Case "float"
							pScript.SetParameterDouble(dz.prop, CDbl(data))
						Case "boolean"
							pScript.SetParameterBool(dz.prop, CBool(data))
						Case Else
							pScript.SetParameterString(dz.prop, CStr(data))
						End Select
					end if
				Case "shm"
					'to change a SHM variable directly without extra scripts
					'e.g. "=election:shm.day_of_month.int"
					if dz.prop <> "" AND dz.propType <> "" then
						Select Case dz.propType
						Case "integer"
							System.Map[dz.prop] = CInt(data)
						Case "float"
							System.Map[dz.prop] = CDbl(data)
						Case "boolean"
							System.Map[dz.prop] = CBool(data)
						Case Else
							System.Map[dz.prop] = CStr(data)
						End Select
					end if
				Case Else
					dz.c.Geometry.Text = data
				End Select
			end if
		next
		dataFieldOrder += 1
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

	RegisterParameterBool("use_groups", "Enable groups", false)
	RegisterParameterBool("Mode", "Ticker mode", false)
	RegisterParameterString("series_separator", "        └ Delimeter (newline is \\\\n):", "\\n", 10, 32, "")
	RegisterParameterDouble("Pause", "        └ Pause (sec):", 5, 0, 10000)
	RegisterParameterBool("group_fill_tail", "└ Fill empty tail (avoid empty groups)", false)
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

	RegisterParameterContainer("root", "Root of the element (or this):")
	RegisterParameterBool("use_mapping", "Use ordered mapping (e.g. \'1=name\')", false)
	RegisterParameterText("mapping", "", 999, 999)
	RegisterPushButton("rebuild", "Initialize", 1)
	RegisterInfoText(info)
	RegisterParameterText("console", "After changing the parameters, click [Initialize]
so that they are applied immediately.
It happens also when the scene is loaded.", 450, 50)

	RegisterParameterBool("TestFunctions", "Show controls", false)
	RegisterPushButton("TestTake","Fill + Take [1]",11)
	RegisterPushButton("TestTakeout","Takeout [0]",10)
	RegisterPushButton("TestChange","Fill + Change [2]",12)
	RegisterPushButton("TestOnOff","Fill + On/Off [3]",13)
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

			this.ScriptPluginInstance.SetParameterString("Name", titr_name)
			this.ScriptPluginInstance.SetParameterString("Take", arrNames[1])
			this.ScriptPluginInstance.SetParameterString("Takeout", arrNames[2])
			this.ScriptPluginInstance.SetParameterString("TakeThis", arrNames[3])
			this.ScriptPluginInstance.SetParameterString("TakeoutThis", arrNames[4])
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


	separator = GetParameterString("series_separator")
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
	SetOrderedMapping()
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
	'follow selection of SINGLE or SERIES mode
	mode = CInt(GetParameterBool("Mode"))
	SendGuiParameterShow("series_separator", mode)
	SendGuiParameterShow("Pause", mode)
	SendGuiParameterShow("Start_by_first", mode)
	SendGuiParameterShow("Takeout_by_last", mode)
	SendGuiParameterShow("group_amount", mode)
	SendGuiParameterShow("group_fill_tail", CInt(  CBool(mode) AND GetParameterBool("use_groups")  ))
	If mode == HIDE Then
		this.ScriptPluginInstance.SetParameterBool("Start_by_first",TRUE)
		SendGuiParameterShow("Start_by_previous", HIDE)
		this.ScriptPluginInstance.SetParameterBool("Takeout_by_last",FALSE)
		SendGuiParameterShow("Takeout_by_last_condis", HIDE)
		SendGuiParameterShow("Takeout_by_last_countLoop", HIDE)
	End If
	SendGuiParameterShow("mapping", CInt(GetParameterBool("use_mapping")))

	SendGuiParameterShow("Start_by_previous", CInt(NOT GetParameterBool("Start_by_first")))

	Dim hasLogicAutoTakeout = CInt(GetParameterBool("LogicAutoTakeout"))
	SendGuiParameterShow("Take", hasLogicAutoTakeout)
	SendGuiParameterShow("Takeout", hasLogicAutoTakeout)
	SendGuiParameterShow("TakeThis", hasLogicAutoTakeout)
	SendGuiParameterShow("TakeoutThis", hasLogicAutoTakeout)

	SendGuiParameterShow("AUTOTAKEOUTPause", CInt(GetParameterBool("AUTOTAKEOUT")))

	takeout_by_last = GetParameterBool("Takeout_by_last")
	SendGuiParameterShow("Takeout_by_last_condis", CInt(takeout_by_last))
	SendGuiParameterShow("Takeout_by_last_countLoop", CInt(takeout_by_last))

	Dim hasTests = CInt(GetParameterBool("TestFunctions"))
	SendGuiParameterShow("TestTake", hasTests)
	SendGuiParameterShow("TestTakeout", hasTests)
	SendGuiParameterShow("TestChange", hasTests)
	SendGuiParameterShow("TestOnOff", hasTests)
	SendGuiParameterShow("TestFill", hasTests)
	SendGuiParameterShow("TestMakeFill", hasTests)
	SendGuiParameterShow("FastPreview", hasTests)
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
			'if we reach on "_fill" then reset it
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
					'if there is a "loop" section
					if fill <> local_memory[titr_name & "_value"] then
						'if there is Change animation
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
		console &= "> Can't find the element director!\n" & "It has to be named as: \"" & titr_name & "\"\n"
	Else
		'it's found!
		if d_OnOff.EventChannel.KeyframeCount < 1 then
			console &= "> The element director has no stop-points!\n" & "Add at least one stop-point.\n"
			exit sub
		elseif d_OnOff.EventChannel.KeyframeCount > 2 then
			console &= "> There are too many stop-points in the element director.\n" & "Please, reduce them to 2.\n" & "This logic supports only 1 or 2.\n"
			exit sub
		end if

		stoper_a = d_OnOff.EventChannel.FirstKeyframe.Time
		stoper_b = d_OnOff.EventChannel.LastKeyframe.Time

		start_time = CDbl(  System.SendCommand("#" & Scene.VizId & "*STAGE*#" & d_OnOff.VizID & "*START_TIME GET")  )
		end_time   = CDbl(  System.SendCommand("#" & Scene.VizId & "*STAGE*#" & d_OnOff.VizID & "*END_TIME GET"  )  )
	End If

	If console = "" Then console = "OK\n"
End Sub

'----------------------------------------------------------

Sub ChangeCurrentSeriesIndex()
	if GetParameterBool("use_groups") then
			local_memory[titr_name & "_curSeries"] = local_memory[titr_name & "_curSeries"] + GetCurrentDropzoneGroupsOrder()
		if GetParameterBool("group_fill_tail") AND local_memory[titr_name & "_curSeries"] > GetCurrentDropzoneGroupsOrder() then
			local_memory[titr_name & "_curSeries"] = local_memory[titr_name & "_curSeries"] Mod GetCurrentDropzoneGroupsOrder()
		end if
	else
		local_memory[titr_name & "_curSeries"] = local_memory[titr_name & "_curSeries"] + 1
	end if
End Sub

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
					ChangeCurrentSeriesIndex()
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
		FillDropzones(fill, DZ_SIDE_FIRST)
		FillDropzones(fill, DZ_SIDE_SECOND)
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
		ChangeCurrentSeriesIndex()
		fill = take_cur_series()
		start_delay_series()
	Else
		fill = memory[titr_name & "_fill"]
	End If
	local_memory[titr_name & "_value"] = fill
	FillDropzones(fill, DZ_SIDE_SECOND)
	StartObjectDirectors()
End Sub

'----------------------------------------------------------

'procedure for sinchronization of Change animation keyframes
'it runs in the "loop_d" keyframe
Sub INtoOUT()
	If cRoot = null Then Exit Sub
	isCanINtoOUT = false
	fill = local_memory[titr_name & "_value"]
	FillDropzones(fill, DZ_SIDE_FIRST)
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
	if mode <> MODE_SERIES then
		'in case non-series mode (Single mode)
		take_cur_series = ""
		exit function
	end if

	'let's calculate which piece is shown and return the next one
	if local_memory.ContainsKey(titr_name & "_curSeries") then
		curSeries = CInt(local_memory[titr_name & "_curSeries"])
		if curSeries < 0 Then curSeries = 0
	else
		curSeries = 0
	end if

	fill = local_memory[titr_name & "_fill"]
	fill.trim()
	fill.split(separator, arr_fill)

	if arr_fill.size <= 0 then
		'if there is no pieces
		local_memory[titr_name & "_curSeries"] = 0
		take_cur_series = ""
		exit function
	end if

	'-------------------------------
	'if there is at least one piece:
	for i = 0 to arr_fill.ubound
		s = arr_fill[i]
		s.Trim()
		If s == "" Then arr_fill.Erase(i)
	next

	if arr_fill.size <= 1 then
		arr_fill.Push("")
		curSeries = 0
	else
		'if the current piece index is more then reset
		if curSeries > arr_fill.UBound then
			curSeries = 0
			local_memory[titr_name & "_curSeries"] = 0
		end if
	end if

	Dim cur_grouped_fills As Array[String]
	if GetParameterBool("use_groups") then
		for i=1 to GetCurrentDropzoneGroupsOrder()
			Dim curSeriesIndex = curSeries + i - 1
			if curSeriesIndex <= arr_fill.ubound then
				cur_grouped_fills.Push(arr_fill[curSeriesIndex])
			elseif GetParameterBool("group_fill_tail") then
				curSeriesIndex = curSeriesIndex Mod arr_fill.size
				cur_grouped_fills.Push(arr_fill[curSeriesIndex])
			else
				cur_grouped_fills.Push("")
			end if
		next
	else
		Dim cur_fill = arr_fill[curSeries]
		if cur_fill.find("=") > 0 then
			nametype = cur_fill.left(fill.find("="))
			cur_fill = cur_fill.right(cur_fill.length - cur_fill.find("=") - 1)
		end if

		nametype.Trim()
		if nametype == "" then
			cur_grouped_fills.Push(cur_fill)
		else
			cur_grouped_fills.Push(nametype & "=" & cur_fill)
		end if
	end if


	Dim result As String
	result.Join(cur_grouped_fills, separator)
	take_cur_series = result
end function

'take the next piece with a delay
sub take_next_series()
	is_needed_to_start_counter_for_taking_next_series = true
	Log("SET is_needed_to_start_counter_for_taking_next_series = true")

	if mode <> MODE_SERIES then
		exit sub
	end if

	'figure out what piece is extered and enter the next one
	curSeries = CInt(local_memory[titr_name & "_curSeries"])
	'zero index means the first piece
	if curSeries < 0 Then curSeries = 0

	fill = memory[titr_name & "_fill"]
	fill.trim()
	fill.split(separator, arr_fill)

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

	If GetParameterBool("Takeout_by_last") Then
		If curSeries >= arr_fill.UBound Then
			If start_by_previous Then curSeries = 0

			if GetParameterBool("use_groups") then
				i_curLoop += GetCurrentDropzoneGroupsOrder()
			else
				i_curLoop += 1
			end if
			takeout_by_last_countLoop = GetParameterInt("Takeout_by_last_countLoop")

			If i_curLoop > takeout_by_last_countLoop Then
				takeout_by_last_condis = GetParameterString("Takeout_by_last_condis")
				takeout_by_last_condis.Trim()
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
