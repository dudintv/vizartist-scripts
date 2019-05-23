RegisterPluginVersion(1,0,0)
Dim info As String = "Moving/arrange animation channels to certain directors. to Developer: Dmitry Dudin, dudin.tv"
Dim c_root As Container
Dim arr_c_parts, arr_c As Array[Container]
Dim arr_ch As Array[Channel]
Dim dir As Director
Dim dir_name As String

Dim arr_type As Array[String]
arr_type.Push("To single director")
arr_type.Push("By child container name")
arr_type.Push("By child container name + index")

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterContainer("root", "Root container (or this)")
	RegisterRadioButton("type", "Where to place", 0, arr_type)
	RegisterParameterString("single_dir_name", "Director Name", "", 40, 999, "")
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
	SendGuiParameterShow("single_dir_name", CInt(GetParameterInt("type")==0))
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecAction(buttonId As Integer)
	OnInit()
	if buttonId == 1 then
		for i=0 to arr_c_parts.ubound
			if GetParameterInt("type") == 0 then
				dir_name = GetParameterString("single_dir_name")
			elseif GetParameterInt("type") == 1 then
				dir_name = arr_c_parts[i].name
			elseif GetParameterInt("type") == 2 then
				dir_name = arr_c_parts[i].name & CStr(i+1)
			end if
			dir = Stage.FindDirector(dir_name)
			if dir == null then
				dir = Stage.RootDirector.AddDirector(TL_NEXT)
				dir.name = dir_name
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
	end if
end sub
