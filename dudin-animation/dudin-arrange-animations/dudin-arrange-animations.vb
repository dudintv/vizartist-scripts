RegisterPluginVersion(1,1,2)
Dim info As String = "Moving/arrange animation channels to certain directors. to Developer: Dmitry Dudin, dudin.tv"

Dim c_root As Container
Dim arr_c_parts, arr_c As Array[Container]
Dim arr_ch As Array[Channel]
Dim dir As Director
Dim dir_name As String
Dim offset_start, offset_step, offset As Double

Dim arr_type As Array[String]
arr_type.Push("To single director")
arr_type.Push("By childs name")
arr_type.Push("By childs name + index")
Dim ARRANGE_TO_SINGLE_DIR As Integer = 0
Dim ARRANGE_TO_DIRS_BY_CONTS_NAME As Integer = 1
Dim ARRANGE_TO_DIRS_BY_CONTS_NAME_PLUS_INDEX As Integer = 2

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterContainer("root", "Root container (or this)")
	RegisterRadioButton("type", "Where to place", 0, arr_type)
	RegisterParameterString("single_dir_name", "Director Name", "", 40, 999, "")
	RegisterParameterBool("offset_on", "Offset", false)
	RegisterParameterDouble("offset_start", "Offset start (sec)", 0, -999999, 999999)
	RegisterParameterDouble("offset_step", "Offset step (sec)", 0, -999999, 999999)
	RegisterPushButton("arrange", "Arrange animations", 1)
end sub

sub OnInit()
	if GetParameterContainer("root") <> null then
		c_root = GetParameterContainer("root")
	else
		c_root = this
	end if
	arr_c_parts.Clear()
	for i=1 to c_root.ChildContainerCount
		arr_c_parts.Push(c_root.GetChildContainerByIndex(i-1))
	next
	offset_start = GetParameterDouble("offset_start")
	offset_step = GetParameterDouble("offset_step")
	
	SendGuiParameterShow("single_dir_name", CInt(GetParameterInt("type")==0))
	SendGuiParameterShow("offset_start", CInt(GetParameterBool("offset_on")))
	SendGuiParameterShow("offset_step", CInt(GetParameterBool("offset_on")))
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecAction(buttonId As Integer)
	OnInit()
	if buttonId == 1 then
		ArrangeAnimation()
	end if
end sub

Sub ArrangeAnimation()
	for i=0 to arr_c_parts.ubound
		'setup director
		if GetParameterInt("type") == ARRANGE_TO_SINGLE_DIR then
			dir_name = GetParameterString("single_dir_name")
		elseif GetParameterInt("type") == ARRANGE_TO_DIRS_BY_CONTS_NAME then
			dir_name = arr_c_parts[i].name
		elseif GetParameterInt("type") == ARRANGE_TO_DIRS_BY_CONTS_NAME_PLUS_INDEX then
			dir_name = arr_c_parts[i].name & CStr(i+1)
		end if
		dir = Stage.FindDirector(dir_name)
		if dir == null then
			dir = Stage.RootDirector.AddDirector(TL_NEXT)
			dir.name = dir_name
		end if
		
		'setup offset
		if GetParameterBool("offset_on") then
			offset = offset_start + offset_step*i
			dir.offset = offset
		end if
		
		arr_c.Clear()
		arr_c_parts[i].GetContainerAndSubContainers(arr_c, false)
		for y=0 to arr_c.ubound
			if arr_c[y].GetChannelsOfObject(arr_ch) > 0 then
				for j=0 to arr_ch.ubound
					arr_ch[j].MoveToDirector(dir)
				next
			end if
		next
	next
End Sub
